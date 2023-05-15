using System.Collections.Generic;

namespace Assets.Scripts.PathFinder
{
    public sealed class PathFindNode : IComparer<PathFindNode>
    {
        public PathFindNode Parent;

        public int X;

        public int Y;

        //当前Tile的评分 = Tile本身分数+Tile距终点的评分(霍夫曼评分) 分数越小越优先选择
        public int Score;
        public bool IsWalkable;

        public void Reset()
        {
            X = Y = -1;
            Parent = null;
            Score = 0;
            IsWalkable = false;
        }


        public int Compare(PathFindNode x, PathFindNode y)
        {
            return x.Score > y.Score ? 1 : -1;
        }
    }
}