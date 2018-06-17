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
    /// <summary>
    /// Helper methods and properties for interacting the the StardewGame
    /// </summary>
    //TODO: move to framework
    public class AFKHostingHelper
    {
        /// <summary>
        /// Indicates if any players other than the current player are online
        /// </summary>
        public static bool RemotePlayersAreOnline => Game1.getOnlineFarmers().Count() > 1;

        public static bool PlayerIsInBed => Game1.player?.isInBed?.Value ?? false;

        public static bool PlayerIsSleeping => !Context.CanPlayerMove && PlayerIsInBed;

        public static string PlayerName => Game1.player?.Name ?? String.Empty;

        public static string FarmName => Game1.MasterPlayer?.farmName?.Value ?? String.Empty;

        public static long PlayerId => Game1.player?.UniqueMultiplayerID ?? default(long);

        /// <summary>
        /// Closes any top level, active, bloacking UI element (such as a menu or dialog) with the default exit functionality of that dialog, if there is one active
        /// </summary>
        public static void CloseDialogOrMenu() => Game1.activeClickableMenu?.exitFunction();

        /// <summary>
        /// Triggers the player sleep routine
        /// </summary>
        /// <remarks>This may have unexpected results if run when sleep is not normally avaible</remarks>
        /// <param name="helper">The SMAPI mod helper</param>
        public static void StartSleep(IModHelper helper)
        {
            var currentLocation = Game1.player?.currentLocation;

            if (currentLocation == null) return;

            helper.Reflection.GetMethod(currentLocation, StardewMethodNames.GameLocationMethodNames.startSleep).Invoke();
        }
    }
}
