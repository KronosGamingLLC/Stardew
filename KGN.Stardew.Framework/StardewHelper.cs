using KGN.Stardew.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;

namespace KGN.Stardew.AFKHosting
{
    /// <summary>
    /// Helper methods and properties for interacting the the StardewGame
    /// </summary>
    // TODO: look into how SMAPI Context player specific values can me made not player specific so reusable methods can be created
    public class StardewHelper
    {
        /// <summary>
        /// Indicates if any players other than the current player are online
        /// </summary>
        public static bool RemotePlayersAreOnline => Game1.getOnlineFarmers().Count() > 1;

        public static bool IsThisPlayerInBed => IsPlayerInBed(Game1.player);

        public static bool IsThisPlayerSleeping => !Context.CanPlayerMove && IsThisPlayerInBed;

        public static string ThisPlayerName => GetPlayerName(Game1.player);

        public static string FarmName => Game1.MasterPlayer?.farmName?.Value ?? String.Empty;

        public static long ThisPlayerId => GetPlayerId(Game1.player);

        public static bool ThisPlayerCanSleep => Context.IsWorldReady && Context.CanPlayerMove && IsThisPlayerInBed;

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
        /// Closes any top level, active, bloacking UI element (such as a menu or dialog) with the default exit functionality of that dialog, if there is one active
        /// </summary>
        public static void CloseDialogOrMenu() => Game1.activeClickableMenu?.exitFunction();

        /// <summary>
        /// Triggers the local player's sleep routine
        /// </summary>
        /// <remarks>This may have unexpected results if run when sleep is not normally avaible</remarks>
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
        /// Teleports a player to their bed
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
    }
}
