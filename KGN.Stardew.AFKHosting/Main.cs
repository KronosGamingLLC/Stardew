using KGN.Stardew.AFKHosting.DialogAutomation;
using KGN.Stardew.AFKHosting.Events;
using KGN.Stardew.Framework;
using KGN.Stardew.Framework.API;
using KGN.Stardew.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using static KGN.Stardew.Framework.StardewAPI;

namespace KGN.Stardew.AFKHosting
{
    public class Main : KGNMod<AFKHostingState>
    {
        public Config Config { get; private set; }
        public DialogAutomationConfig DialogAutomationConfig { get; private set; }
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<Config>();
            DialogAutomationConfig = Helper.ReadJsonFile<DialogAutomationConfig>(Path.Combine(Helper.DirectoryPath,$"{nameof(DialogAutomationConfig)}.json"));

            if(DialogAutomationConfig == null)
            {
                DialogAutomationConfig = new DialogAutomationConfig();
                Helper.WriteJsonFile(Path.Combine(Helper.DirectoryPath, $"{nameof(DialogAutomationConfig)}.json"), DialogAutomationConfig);
            }

            State = new AFKHostingState(Config.StartInAFKHostingMode, false);
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
        #region ModStartupAndTermination

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

                Monitor.Log($"AFK Hosting initialized for '{FarmName}'", LogLevel.Info);
            }
            else
                Monitor.Log("AFK Hosting disabled in single player mode.", LogLevel.Info);
        }

        

        public void HookupStardewEvents()
        {
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick += GameEvents_QuarterSecondTick;
            GameEvents.UpdateTick += GameEvents_UpdateTick;
        }

        public void ReleaseStardewEvents()
        {
            InputEvents.ButtonReleased -= InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick -= GameEvents_QuarterSecondTick;
        }

        #endregion

        #region Events
        //fast enough that it seems near instant but more performant since it doesn't run as often
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            if (State.AFKHostingOn)
                AFKHostingRoutine();
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            UpdateTick();
        }

        //handle key presses
        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady && e.Button == Config.ToggleAFKKey)
                BroadcastEvent(new ToggleAFKStatus());
        }
        #endregion

        //todo: factor out this logic from main and add wentToFestival to mod state
        //TODO: add trace log
        public void AFKHostingRoutine()
        {
            //TODO: need to cancel waiting for player dialog if other players have quit
            //should be able to ignore events and cutscenes since that should be handled by Context.PlayerCanMove
            //TODO: cancel cutscenens when they occur
            //TODO: move player outside house in morning to trigger any cutscenes that might occur

            //TODO: pause game if no players are online
#if DEBUG
            if (!(IsThisPlayerFree && Context.IsMultiplayer))
                return;
#else
            if (!(IsThisPlayerFree && RemotePlayersAreOnline && Context.IsMultiplayer))
                return;
#endif
            //teleport player to festival when its ready if they have not been yet
            if (IsFestivalDay && IsFestivalReady && !IsThisPlayerAtFestival && !State.WentToTodaysFestival)
            {
                var festivalLocation = WhereIsFestival();
                if (festivalLocation != Location.None)
                {
                     

                    void DebouncedTeleportToNPC(object sender, EventArgs args)
                    {
                        RateLimiting.Debounce(nameof(DebouncedTeleportToNPC), 1000, () =>
                        {
                            ViewportChanged -= DebouncedTeleportToNPC;
                            var testDialog = new NPCDialogAutomater("Lewis", true, Monitor, Helper,
                                new DialogAction[] {
                                    new DialogAction {
                                        DialogActionType = DialogActionType.Start
                                    },
                                    new DialogAction {
                                        DialogActionType = DialogActionType.Next
                                    },
                                    new DialogAction {
                                        DialogActionType = DialogActionType.Next
                                    },
                                    new DialogAction {
                                        DialogActionType = DialogActionType.Start
                                    },
                                    new DialogAction {
                                        DialogActionType = DialogActionType.Answer,
                                        AnswerIndex = 0
                                    }
                                });

                            ViewportChanged += DebouncedRunActions;
                            testDialog.TeleportToNPC();
                            
                            void DebouncedRunActions(object s, EventArgs e)
                            {
                                RateLimiting.Debounce(nameof(DebouncedRunActions), 1000, () =>
                                {
                                    ViewportChanged -= DebouncedRunActions;
                                    testDialog.RunActions();
                                });
                            }
                        });
                    }

                    ViewportChanged += DebouncedTeleportToNPC;
                    TeleportThisPlayer(festivalLocation, 0, 0);
                    return;
                }
            }

            //should not have to handle where player is waiting for other players to enter festival
            //as that should be taken care of by StardewAPI.IsThisPlayerFree
            if(IsFestivalDay && IsThisPlayerAtFestival && !State.WentToTodaysFestival)
            {
                //var tile = Game1.currentLocation?.currentEvent?.getActorByName("Lewis")?.getTileLocation();
                //if (!tile.HasValue) return;
                //Game1.player.setTileLocation(new Vector2(tile.Value.X, tile.Value.Y+1));

                State.With(s => s.WentToTodaysFestival, true);
                //Game1.player.team.SetLocalReady("festivalEnd", true);
                //Game1.activeClickableMenu = (IClickableMenu)new ReadyCheckDialog("festivalEnd", true, new ConfirmationDialog.behavior(Game1.currentLocation.currentEvent.forceEndFestival), (ConfirmationDialog.behavior)null);
            }

            //this should run once to trigger the festival leave waiting screen which should teleport to farm
            //(or just start here if there is no festival) then it will run again to move player to bed if they are not in it
            //and one more time to trigger wait for sleep
            //TODO: test how this is affected by time change when festival ends
            if (!IsFestivalDay || (IsFestivalDay && !IsThisPlayerAtFestival && State.WentToTodaysFestival))
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
            if (!IsFestivalDay && State.WentToTodaysFestival)
                State.With(s => s.WentToTodaysFestival, false);

            //if (Game1.activeClickableMenu is DialogueBox dialogueBox)
            //{
            //    var isQuestion = Helper.Reflection.GetField<bool>(dialogueBox, "isQuestion").GetValue();
            //    if(isQuestion)
            //    {
            //        //TODO: auto answer questions based on a json event/dialog/person configuration file
            //        return;
            //    }
            //    CloseDialogOrMenu();
            //}
                
        }

        private void StartDialogDebounced(object sender, EventArgs args)
        {
            RateLimiting.Debounce(nameof(StartDialogDebounced), 1000, () => {
                var tile = Game1.currentLocation?.currentEvent?.getActorByName("Lewis")?.getTileLocation();
                if (tile != null)
                {
                    Vector2 mousePosition = tile.Value.ConvertTileToMouseCoords();
                    var mouseState = new MouseState(Convert.ToInt32(mousePosition.X), Convert.ToInt32(mousePosition.Y), 0, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released);
                    Game1.setMousePosition(new Point(Convert.ToInt32(mousePosition.X), Convert.ToInt32(mousePosition.Y)));
                    Game1.pressActionButton(new KeyboardState(), mouseState, new GamePadState());
                    ViewportChanged -= StartDialogDebounced;
                }
            });
        }
    }
}
