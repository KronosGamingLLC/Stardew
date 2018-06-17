using KGN.Stardew.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.AFKHosting
{
    public class AFKHostingHelper
    {
        public static bool RemotePlayersOnline => Game1.getOnlineFarmers().Count() > 1;

        public static bool PlayerInBed => Game1.player.isInBed.Value;

        public static void StartSleep(IModHelper helper)
        {
            var currentLocation = Game1.player.currentLocation;
            helper.Reflection.GetMethod(currentLocation, StardewMethods.GameLocationMethods.startSleep).Invoke();
        }
    }
}
