using KGN.Stardew.AFKHosting.Events;
using KGN.Stardew.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using static KGN.Stardew.AFKHosting.StardewHelper;

namespace KGN.Stardew.AFKHosting
{
    public class Main : KGNMod<AFKHostingState>
    {
        public Config Config { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<Config>();
            State = new AFKHostingState(Config.StartInAFKHostingMode);
            LoadEvents();

            //todo: add framework for loading custom commands
            //todo: how to remove commands
            Helper.ConsoleCommands.Add("afk", "toggles player's afk status for AFKHosting mod", (cmd, args) =>
            {
                if(Context.IsWorldReady)
                    BroadcastEvent(new ToggleAFKStatus());
            });

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterCreate += SaveEvents_AfterCreate;

            //is this too late to disable mod?
            SaveEvents.AfterReturnToTitle += (s, e) =>
            {
                Monitor.Log($"AFK Hosting mod stopped due to game exit.", LogLevel.Info);
                SaveEvents.AfterLoad += SaveEvents_AfterLoad;
                SaveEvents.AfterCreate += SaveEvents_AfterCreate;
            };
        }

        //methods that start and stop the mod when loading/closing games
        #region Initialization

        private void SaveEvents_AfterCreate(object sender, EventArgs e)
        {
            Initialize();
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            SaveEvents.AfterLoad -= SaveEvents_AfterLoad;
            SaveEvents.AfterCreate -= SaveEvents_AfterCreate;

            if (Context.IsMultiplayer)
            {
                HookupStardewEvents();

                Monitor.Log($"AFK Hosting initialized for '{StardewHelper.FarmName}'", LogLevel.Info);
            }
            else
                Monitor.Log("AFK Hosting disabled in single player mode.", LogLevel.Info);
        }

        #endregion

        public void HookupStardewEvents()
        {
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick += GameEvents_QuarterSecondTick;
        }

        public void ReleaseStardewEvents()
        {
            InputEvents.ButtonReleased -= InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick -= GameEvents_QuarterSecondTick;
        }
   
        //fast enough that it seems near instant but more performant since it doesn't run as often
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            if (State.AFKHostingOn)
                AFKHostingRoutine();
        }

        //handle key presses
        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady && e.Button == Config.ToggleAFKKey)
                BroadcastEvent(new ToggleAFKStatus());
        }

        //todo: factor out this logic from main and add wentToFestival to mod state
        //TODO: add trace log
        private bool wentToFestival = false;
        private bool testRunOnce = true;
        public void AFKHostingRoutine()
        {
            //TODO: need to cancel waiting for player dialog if other players have quit
            //should be able to ignore events and cutscenes since that should be handled by Context.PlayerCanMove
            //TODO: cancel cutscenens when they occur
            //TODO: move player outside house in morning to trigger any cutscenes that might occur

            //TODO: pause game if no players are online
            if (!(IsThisPlayerFree && RemotePlayersAreOnline && Context.IsMultiplayer))
                return;

            //TODO: test if this tries to teleport player more than once
            if(IsFestivalDay && IsFestivalReady && !IsThisPlayerAtFestival && !wentToFestival)
            {
                //make this a helper function
                var festivalLocation = WhereIsFestival();
                if(festivalLocation != Location.None)
                {
                    TeleportThisPlayer(festivalLocation, 0, 0);
                }
            }

            //should not have to handle where player is waiting for other players to enter festival
            //as that should be taken care of by StardewHelper.IsThisPlayerFree

            if(IsFestivalDay && IsThisPlayerAtFestival && !wentToFestival)
            {
                wentToFestival = true;
                Game1.player.team.SetLocalReady("festivalEnd", true);
                Game1.activeClickableMenu = (IClickableMenu)new ReadyCheckDialog("festivalEnd", true, new ConfirmationDialog.behavior(Game1.currentLocation.currentEvent.forceEndFestival), (ConfirmationDialog.behavior)null);
            }

            //this should run once to trigger the festival leave waiting screen which should teleport to farm
            //(or just start here if there is no festival) then it will run again to move player to bed if they are not in it
            //and one more time to trigger wait for sleep
            //TODO: test how this is affected by time change when festival ends
            if (!IsFestivalDay || (IsFestivalDay && !IsThisPlayerAtFestival && wentToFestival))
            {
                //TODO: this doesnt work when at a festival, tries to tele repeatedly but nothing happens
                //test if this waits until the teleport is finished
                if (!IsThisPlayerInBed)
                {
                    TeleportThisPlayerToBed();
                }
                else
                {
                    StartSleepForThisPlayer(Helper);
                }
            }

            //reset festival status on next day
            //TODO: will this work for night market?
            if (!IsFestivalDay && wentToFestival)
                wentToFestival = false;
        }
    }
}
