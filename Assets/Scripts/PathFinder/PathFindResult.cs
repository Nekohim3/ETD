using UnityEngine;

namespace Assets.Scripts.PathFinder
{
    public class PathFindResult
    {
        public bool IsHavePath;
        public Vector2Int[] Paths;

        public override string ToString()
        {
            var str = "IsHavePath: " + IsHavePath + "\n";

            if (Paths != null)
            {
                for (var i = 0; i < Paths.Length; i++)
                {
                    str += "Step " + i + ":   ( " + Paths[i].x + " , " + Paths[i].y + " )\n";
                }
            }

            return str;
        }
    }
}