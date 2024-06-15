using UnityEngine;

namespace LB
{
    [CreateAssetMenu(fileName="New Tileset", menuName="Level Builder/Tileset", order=1)]
    public class Tileset : ScriptableObject
    {
        public float TileSize = 3f;
        public Tile FloorTile;
        public Tile LineWallTile;
        public Tile CornerWallTile;
        public Tile TWallTile;
        public Tile XWallTile;
        public Tile ColumnTile;
        public Tile EndWallTile;

        public Tile GetTileByType(WP wp)
        {
            switch (wp)
            {
                case WP.Floor:
                    return FloorTile;
                case WP.Line:
                    return LineWallTile;
                case WP.Corner:
                    return CornerWallTile;
                case WP.T:
                    return TWallTile;
                case WP.X:
                    return XWallTile;
                case WP.Column:
                    return ColumnTile;
                case WP.End:
                    return EndWallTile;
                default: 
                    Debug.LogError("WallPiece Type not recognized");
                    return FloorTile;
            }
        }
    }
}