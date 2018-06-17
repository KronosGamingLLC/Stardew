using KGN.Stardew.AFKHosting.Events;
using KGN.Stardew.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;

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

        //TODO: need to cancel sleep dialog if other players have quit
        //fast enough that it seems near instant but more performant since it doesn't run as often
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            //trigger sleep when possible if afk mode is on
            if (State.AFKHostingOn && StardewHelper.ThisPlayerCanSleep && StardewHelper.RemotePlayersAreOnline)
            {
                StardewHelper.StartSleepForThisPlayer(Helper);
                Monitor.Log($"AFK sleep has been triggered for player '{StardewHelper.ThisPlayerName}' ({StardewHelper.ThisPlayerId}).", LogLevel.Trace);
            }
        }

        //handle key presses
        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady && e.Button == Config.ToggleAFKKey)
                BroadcastEvent(new AFKHostingKeyPress());
        }
    }
}
