namespace Assets.Scripts.PathFinder
{
    public class PathFindMapAgent
    {
        public enum TileSearchType
        {
            FourDirection,
            EightDirection,
            EightDirectionFixCorner
        }


        /// <summary>
        /// 取得某个节点的相关属性
        /// </summary>
        /// <param name="tileX">Tile x.</param> 当前Tile的X
        /// <param name="tileY">Tile y.</param> 当前Tile的Y
        /// <param name="start">Start.</param> 搜索的起点
        /// <param name="end">End.</param> 搜索的终点
        /// <param name="isWalkable">Is walkable.</param> 当前Tile是否可以行走
        /// <param name="score">Score. </param>  Tile的整体评分 (Tile本身分数+Tile距终点的评分(霍夫曼评分))
        public virtual void GetTileProperty(int tileX, int tileY,
            PathFindNode start, PathFindNode end,
            out bool isWalkable, out int score)
        {
            isWalkable = true;
            score = 1;
        }

        public virtual bool IsTileWalkable(int tileX, int tileY)
        {
            return true;
        }

        public virtual TileSearchType GetTileSearchType()
        {
            return TileSearchType.EightDirectionFixCorner;
        }
    }
}