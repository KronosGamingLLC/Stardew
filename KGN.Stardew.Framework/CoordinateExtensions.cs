using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KGN.Stardew.Framework.StardewAPI;

namespace KGN.Stardew.Framework
{
    //TODO: add other coordinate conversions (tile,screen,absolute) for vector2, point, x,y, etc
    public static class CoordinateExtensions
    {
        //just some syntatical/naming sugar
        public static Vector2 ConvertTileToMouseCoords(this Vector2 tile) => ConvertTileToScreenCoords(tile);

        public static Vector2 ConvertTileToScreenCoords(this Vector2 tile)
        {
            return tile * new Vector2(TileSize) - new Vector2(Viewport.X, Viewport.Y);
        }

        public static Vector2 ConvertTileToAbsoluteCoords(this Vector2 tile)
        {
            return tile * new Vector2(TileSize);
        }
    }
}
