using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extension;
using UnityEngine;

namespace Assets.Scripts.DungeonGenerator
{
    public class Pass
    {
        public Room          StartRoom { get; set; }
        public Room          EndRoom   { get; set; }
        public List<RectInt> LineList  { get; set; }

        public Pass(Room startRoom, Room endRoom, params RectInt[] lineList)
        {
            StartRoom = startRoom;
            EndRoom   = endRoom;
            LineList  = lineList.Select(_ => _.Normalized()).ToList();
        }

        public Pass(Room startRoom, Room endRoom)
        {
            StartRoom = startRoom;
            EndRoom   = endRoom;
            LineList  = new List<RectInt>();
        }

        public void AddLine(RectInt line)
        {
            LineList.Add(line.Normalized());
        }

    }
}
