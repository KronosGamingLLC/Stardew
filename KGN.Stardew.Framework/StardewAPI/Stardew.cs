using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework
{
    //notes, the following will need a lot of reflection/injection/modification, possibly using Harmony, to tie into
    //event is finished after Game1.EventFinished is called
    //
    //Game1.shouldTimePass() - wether or not time should pass
    //
    //the result of shouldTimePass() has the following effect on Game1.UpdateGameClock()
    //if it is true, Game1.gameTimeInterval is incremented by the elapsed milliseconds since [?]
    //once gameTimeInterval reaches 7000 (hardcoded), it is reset to 0 if Game1.panMode is true, otherwise the clock moves forward ten minutes
    //Additionaly, locations may have add an amount(which can be negative) to the 7000 milliseconds required, it is accessed by GameLocation.getExtraMillisecondsPerInGameMinuteForThisLocation() which is an overideable method

    public class StardewPropertyInfo
    {
        public PropertyInfo CurrentPropertyInfo { get; set; }
        public FieldInfo PreviousFieldInfo { get; set; }
        public FieldInfo EventInfo { get; set; }
    }

    //pragma tags are to ignore the 'never used' warning for events since they are called via reflection
    public class StardewAPI
    {
        private const string eventSuffix = "Changed";
        private const string previousValuePrefix = "previous";
        private static readonly Type genericStardewEventArgsType = typeof(StardewPropertyChangedEventArgs<>);

        private static readonly IReadOnlyList<StardewPropertyInfo> propertiesWithChangedEvent;

        static StardewAPI()
        {
            //this is cause im lazy and don't feel like manually adding the code to check all the properties
            //but this is not the best because it implies a standard naming convention which can't be enforced
            var thisType = typeof(StardewAPI);
            var publicProperties = thisType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            var privateFields = thisType.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var currentProperties = publicProperties.Where(c => 
                privateFields.Any(e => e.Name == $"{c.Name}{eventSuffix}")
                && privateFields.Any(p => p.Name == $"{previousValuePrefix}{c.Name}")
            );
            propertiesWithChangedEvent = currentProperties.Select(c => new StardewPropertyInfo {
                CurrentPropertyInfo = c,
                PreviousFieldInfo = privateFields.FirstOrDefault(p => p.Name == $"{previousValuePrefix}{c.Name}"),
                EventInfo = privateFields.FirstOrDefault(e => e.Name == $"{c.Name}{eventSuffix}")
            }).ToList();
        }

        public static void UpdateAPI()
        {
            //TODO: can this be parrallel?
            foreach (var property in propertiesWithChangedEvent)
            {
                var currentValue = property.CurrentPropertyInfo.GetMethod.Invoke(null, null);
                var previousValue = property.PreviousFieldInfo.GetValue(null);

                if (currentValue.Equals(previousValue))
                    continue;

                var @event = property.EventInfo.GetValue(null) as MulticastDelegate;

                if (@event == null) return;

                var argsType = genericStardewEventArgsType.MakeGenericType(property.CurrentPropertyInfo.PropertyType);
                var eventArgs = Activator.CreateInstance(argsType, previousValue, currentValue);

                @event?.DynamicInvoke(null, eventArgs);
            }
        }

        /// <summary>
        /// Indicates if a game has been loaded and the player is the host.
        /// </summary>
        public static bool IsHost => Context.IsMainPlayer;

        /// <summary>
        /// Indicates if the game is paused
        /// </summary>
        public static bool IsPaused => (!Context.IsMultiplayer && Game1.paused) || Game1.HostPaused;
        private static bool previousIsPaused = IsPaused;
#pragma warning disable 0169
        public static event EventHandler<StardewPropertyChangedEventArgs<bool>> IsPausedChanged;
#pragma warning restore 0169

        /// <summary>
        /// Indicates if any players other than the current player are online
        /// </summary>
        public static bool AreRemotePlayersOnline => (Game1.otherFarmers?.Count ?? 0) > 0;
        private static bool previousAreRemotePlayersOnline = AreRemotePlayersOnline;
#pragma warning disable 0169
        public static event EventHandler<StardewPropertyChangedEventArgs<bool>> AreRemotePlayersOnlineChanged;
#pragma warning restore 0169

        /// <summary>
        /// If the local player is touching the bed
        /// </summary>
        public static bool IsThisPlayerInBed => IsPlayerInBed(Game1.player);

        /// <summary>
        /// If the local player is sleeping. ie, control has returned to the player after triggering sleep
        /// </summary>
        //TODO: i feel like there is a better way to determine this. 
        public static bool IsThisPlayerSleeping => !Context.CanPlayerMove && IsThisPlayerInBed;

        /// <summary>
        /// The local players farmer name
        /// </summary>
        public static string ThisPlayerName => GetPlayerName(Game1.player);

        /// <summary>
        /// The name of the farm
        /// </summary>
        //TODO: does this include "Farm" appended to the end?
        public static string FarmName => Game1.MasterPlayer?.farmName?.Value ?? String.Empty;

        /// <summary>
        /// The local players UniqueMultiplayerId
        /// </summary>
        public static long ThisPlayerId => GetPlayerId(Game1.player);

        public static bool IsThisPlayerFree => Context.IsWorldReady && (Context.CanPlayerMove || IsThisPlayerAtFestival); //TODO: the or @festival is a temporary workaround as Context.IsPlayerFree is false while at a festival, bug report already submitted

        /// <summary>
        /// The current day, year, and season
        /// </summary>
        public static SDate Today => SDate.Now();

        /// <summary>
        /// The current time of day in military format, ie 0-2400
        /// </summary>
        public static int CurrentTime => Game1.timeOfDay;

        /// <summary>
        /// If the current game day is a festival day
        /// </summary>
        //TODO: verify if day is 0 or 1 based for this function
        public static bool IsFestivalDay => Utility.isFestivalDay(Today.Day, Today.Season);

        /// <summary>
        /// If the festival is ready (not being set up, if the time it is open has passed)
        /// </summary>
        //TODO: return false if festival is over
        public static bool IsFestivalReady => IsFestivalDay && Game1.timeOfDay >= GetFestivalStartTime();

        /// <summary>
        /// Wether or not the local player is in a game location that is a festival
        /// </summary>
        public static bool IsThisPlayerAtFestival => Game1.isFestival();

        /// <summary>
        /// Gets the location of todays festival.
        /// </summary>
        /// <returns>The location of todays festival. Returns Location.None if there is no festival.</returns>
        public static Location WhereIsFestival()
        {
            if (String.IsNullOrWhiteSpace(Game1.whereIsTodaysFest))
                return Location.None;

            var location = Location.None;
            Enum.TryParse(Game1.whereIsTodaysFest, out location);

            return location;
        }

        /// <summary>
        /// Wether or not the currently displayed dialog is the waiting for other players dialog. Returns false if there is no dialog.
        /// </summary>
        public static bool CurrentDialogIsWaitForPlayers => Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog;

        /// <summary>
        /// Wethor or not the local player is in an event (cutscene)
        /// </summary>
        public static bool InEvent => Game1.CurrentEvent != null || Game1.eventUp;

        /// <summary>
        /// If there is an event at the local player's location. Always true if IsPlayerAtFestival is true.
        /// </summary>
        public static bool IsInEventLocation => Game1.currentLocation?.currentEvent != null;

        /// <summary>
        /// Gets a player's name
        /// </summary>
        /// <param name="player">the player to get the name of</param>
        /// <returns>the player's name</returns>
        /// <exception cref="ArgumentNullException">Thrown if player is null</exception>
        public static string GetPlayerName(Farmer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return player.Name;
        }

        /// <summary>
        /// Gets a player's UniqueMultiplayerId
        /// </summary>
        /// <param name="player">the player to get the id of</param>
        /// <returns>the player's UniqueMultiplarId</returns>
        /// <exception cref="ArgumentNullException">Thrown if player is null</exception>
        public static long GetPlayerId(Farmer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return player.UniqueMultiplayerID;
        }

        /// <summary>
        /// Checks if a player is in bed
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>A boolean representing if the player is in bed</returns>
        public static bool IsPlayerInBed(Farmer player)
        {
            if (player == null) return false;
            //throw new ArgumentNullException(nameof(player));

            return player.isInBed.Value;
        }

        /// <summary>
        /// Closes any top level, active, blocking UI element (such as a menu or dialog) with the default exit functionality of that dialog, if there is one active
        /// </summary>
        //TODO: or should Game1.exitActiveMenu() be used?
        public static void CloseDialogOrMenu() => Game1.activeClickableMenu?.exitFunction();

        /// <summary>
        /// Closes all dialogs with the emergencyShutDown method. Usually used to prepare player for an immediate, forced event.
        /// </summary>
        //TODO: make sure this closes ALL dialogs
        public static void EmergencyCloseDialogs() => Game1.activeClickableMenu?.emergencyShutDown();

        /// <summary>
        /// Triggers the local player's sleep routine. This may have unexpected results if run when sleep is not normally avaible.
        /// </summary>
        /// <param name="helper">The SMAPI mod helper</param>
        public static void StartSleepForThisPlayer(IModHelper helper)
        {
            StartSleepForPlayer(Game1.player, helper);
        }

        /// <summary>
        /// Triggers a player's sleep routine
        /// </summary>
        /// <remarks>This may have unexpected results if run when sleep is not normally avaible, and probably shouldn't be used for remote players</remarks>
        /// <param name="player">The player to run the sleep routine on</param>
        /// <param name="helper">The SMAPI mod helper</param>
        /// <exception cref="ArgumentNullException">Thrown when player is null</exception>
        public static void StartSleepForPlayer(Farmer player, IModHelper helper)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (player.currentLocation == null) return;

            helper.Reflection.GetMethod(player.currentLocation, StardewMethodNames.GameLocationMethodNames.startSleep).Invoke();
        }

        /// <summary>
        /// Teleports the local player to their bed.
        /// </summary>
        public static void TeleportThisPlayerToBed()
        {
            TeleportPlayerToBed(Game1.player);
        }

        /// <summary>
        /// Teleports a player to their bed. May have unexpected results if used on a remote player.
        /// </summary>
        /// <param name="player">the player to teleport</param>
        /// <exception cref="ArgumentNullException">Thrown if player is null</exception>
        public static void TeleportPlayerToBed(Farmer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            //no need to teleport if we are already in bed
            if (IsPlayerInBed(player)) return;

            //taken from game code Farmer.PassOutFromTired
            Vector2 vector2 = Utility.PointToVector2(Utility.getHomeOfFarmer(player).getBedSpot()) * 64f;
            vector2.X -= 64f;
            Game1.warpFarmer(player.homeLocation.Value, (int)vector2.X / 64, (int)vector2.Y / 64, 2, false);
            player.currentLocation.lastTouchActionLocation = vector2;
        }

        /// <summary>
        /// Teleports the local player to specific coordinates of a location using the warp console command. May have unexepected results if used on a remote player.
        /// This method allows the original game code to flag a player as waiting for other players when entering a festival location.
        /// </summary>
        /// <param name="location">The name of the location</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void TeleportThisPlayer(Location location, int x, int y)
        {
            //TODO: throw exception to be caught by logging higher up
            if (location == Location.None)
                return;

            ConsoleCommands.RunCommand(ConsoleCommands.BuildWarpCommand(location, x, y));
        }

        /// <summary>
        /// Pauses the game. Only works for host.
        /// </summary>
        public static void PauseGame()
        {
            if (!IsHost || IsPaused) return;

            Game1.paused = true;
            Game1.netWorldState.Value.IsPaused = true;
        }

        /// <summary>
        /// Unpause the game. Only works for host.
        /// </summary>
        public static void UnpauseGame()
        {
            if (!IsHost || !IsPaused) return;

            Game1.paused = false;
            Game1.netWorldState.Value.IsPaused = false;
        }

        /// <summary>
        /// Skips an event (cutscene) if it is skippable
        /// </summary>
        public static void SkipEvent()
        {
            if (!Game1.CurrentEvent.skippable) return;

            Game1.CurrentEvent.skipped = true;
            Game1.CurrentEvent.skipEvent();
            Game1.freezeControls = false;
        }

        //TODO: refactor
        public static int GetFestivalStartTime()
        {
            const string festivalDataPath = "Data\\Festivals\\";

            var todaysFestivalPath = $"{festivalDataPath}{Today.Season}{Today.Day}";

            int startTime = 0;

            try
            {
                var festivalData = Game1.temporaryContent.Load<Dictionary<string, string>>(todaysFestivalPath);
                var startTimeString = festivalData["conditions"].Split('/')[1].Split(' ')[0];
                Int32.TryParse(startTimeString, out startTime);
            }
            catch { }

            return startTime;
        }

        public enum Location
        {
            None,
            FarmHouse,
            Farm,
            FarmCave,
            Town,
            JoshHouse,
            HaleyHouse,
            SamHouse,
            Blacksmith,
            ManorHouse,
            SeedShop,
            Saloon,
            Trailer,
            Hospital,
            HarveyRoom,
            Beach,
            ElliotHouse,
            Mountain,
            ScienceHouse,
            SebastionRoom,
            Tent,
            Forest,
            WizardHouse,
            AnimalShop,
            LeahHouse,
            BusStop,
            Mine,
            Sewer,
            BugLand,
            Desert,
            Club,
            SandyHouse,
            ArchaeologyHouse,
            WizardHouseBasement,
            AdventureGuild,
            Woods,
            Railroad,
            WitchSwamp,
            WitchHut,
            WitchWarpCave,
            Summit,
            FishShop,
            BathHouse_Entry,
            BathHouse_MensLocker,
            BathHouse_WomensLocker,
            BathHouse_Pool,
            CommunityCenter,
            JojaMart,
            Greenhouse,
            SkullCave,
            Backwoods,
            Tunnel,
            Trailer_Big,
            Celler,
            BeachNightMarket,
            MermaidHouse,
            Submarine,

        }

        /// <summary>
        /// Gets a GameLocation object with a Location enum
        /// </summary>
        /// <param name="location">The location to retrieve</param>
        /// <returns>A GameLocation object of the location</returns>
        public static GameLocation GetLocation(Location location) => Game1.getLocationFromName(location.ToString());

        //TODO: Make functions in the helper (like teleport) to run these. I think I can make this a fluent api
        public class ConsoleCommands
        {
            public const string canmove = "canmove";
            public const string die = "die";
            public const string ee = "ee";
            public const string eventOver = "eventOver";
            public const string eventseen = "eventseen";
            public const string fenceDecay = "fenceDecay";
            public const string netclear = "netclear";
            public const string netdump = "netdump";
            public const string netlog = "netlog";
            public const string noSave = "noSave";
            public const string r = "r";
            public const string warpHome = "warpHome";
            public const string where = "where";

            public const string warp = "warp";
            public static string BuildWarpCommand(Location location, int x, int y) => $"{warp} {location} {x} {y}";

            public const string minigame = "minigame";

            public enum Minigame
            {
                cowbow,
                blastoff,
                minecart,
                grandpa,
            }
            public static string BuildMinigameCommand(Minigame minigame) => $"{ConsoleCommands.minigame} {minigame}";

            public const string time = "time";
            public static string BuildTimeCommand(int militaryTime) => $"{time} {militaryTime}";

            public const string year = "year";
            public static string BuildYearCommand(int yearNumber) => $"{year} {yearNumber}";

            public static void RunCommand(string command) => Game1.game1.parseDebugInput(command);

            /*
             * c - stops players current action (not events) and sets player to moveable
             * die - kills player
             * ee - ends event (endEvent does the same thing just more?)
             * endEvent - ends event
             * eventOver - calls Game1.eventFinished()
             * eventseen [id] - marks event as seen by the local player
             * fenceDecay - trigger fence decay
             * minigame [gameString]- starts minigame (cowboy,blastoff,minecart,grandpa)
             * netclear - clear multiplayer net log
             * netdump - dump multiplayer net log
             * nethost - start multiplayer server
             * netjoin - sets menu to FarmhandMenu?
             * netlog - toggles net log and debug output for net log on/off
             * ns - toggles saving on/off
             * r - resets location i think?
             * time [24 hour time] - sets time of day
             * warp [locationName] [x] [y] - teleports player to coords at location
             * wh - teleports player to home location, possibly bed?
             * where - prints debug output of current location of character
             * year [int] - sets the year
             */
        }
    }
}
