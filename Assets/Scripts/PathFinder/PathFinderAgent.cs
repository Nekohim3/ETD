using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.PathFinder
{
    public class PathFinderAgent : PathFindMapAgent
    {
        public int[,] Map { get; set; }

        public PathFinderAgent(int[,] map)
        {
            Map = map;
        }

        public override void GetTileProperty(int tileX, int tileY, PathFindNode start, PathFindNode end, out bool isWalkable, out int score)
        {
            isWalkable = false;
            score      = int.MaxValue;
            if (tileX < 0 || tileY < 0 || tileX >= Map.GetLength(0) || tileY >= Map.GetLength(1))
                return;

            try
            {
                switch (Map[tileX, tileY])
                {
                    case 1:
                        isWalkable = true;
                        score      = 1;
                        break;
                }
            }
            catch (Exception e)
            {

            }
        }

        public override bool IsTileWalkable(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Map.GetLength(0) || tileY >= Map.GetLength(1))
                return false;
            switch (Map[tileX, tileY])
            {
                case 1:
                    return true;
            }

            return false;
        }

        public override TileSearchType GetTileSearchType()
        {
            return TileSearchType.FourDirection;
        }
    }
}
