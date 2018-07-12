using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.API
{
    //TODO: Make functions in the api (like teleport) to run these. I think I can make this a fluent api
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
        public static string BuildWarpCommand(Location location, int x, int y) => BuildWarpCommand(location.ToString(), x, y);
        public static string BuildWarpCommand(string location, int x, int y) => $"{warp} {location} {x} {y}";

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
