using UnityEngine;

namespace Assets.Scripts.DungeonGenerator
{
    public class Room
    {
        public string  Name     { get; set; }
        public RectInt Bounds   { get; set; }
        public Vector2 MidPoint => Bounds.center;
        public int     Left     => Bounds.xMin;
        public int     Top      => Bounds.yMin;
        public int     Right    => Bounds.xMax;
        public int     Bottom   => Bounds.yMax;

        public Room(RectInt r, string name)
        {
            Bounds = r;
            Name   = name;
        }
    }
}
