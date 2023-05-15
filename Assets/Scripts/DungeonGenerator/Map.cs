using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extension;
using RogueSharp;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.ShaderData;
using Random = System.Random;

namespace Assets.Scripts.DungeonGenerator
{
    public class Map
    {
        private static Range  _roomWidth;
        private static Range  _roomHeight;
        private static Range  _roomCount;
        private static Range  _distanceBetweenRooms;
        private static Range  _passWidth;
        private static int    _passPercent;
        private static int    _seed;
        private static Random _rand;

        public bool       IsReady { get; set; }
        public List<Room> Rooms   { get; set; }
        public List<Pass> Passes  { get; set; }
        public int        Width   { get; set; }
        public int        Height  { get; set; }

        public Map()
        {
            Rooms  = new List<Room>();
            Passes = new List<Pass>();
        }

        public Map(Range roomWidth,
                   Range roomHeight,
                   Range roomCount,
                   Range distanceBetweenRooms,
                   Range passWidth,
                   int   passPercent,
                   int   seed)
        {
            Rooms  = new List<Room>();
            Passes = new List<Pass>();
            Generate(roomWidth, roomHeight, roomCount, distanceBetweenRooms, passWidth, passPercent, seed);
        }

        public void Generate(Range roomWidth,
                             Range roomHeight,
                             Range roomCount,
                             Range distanceBetweenRooms,
                             Range passWidth,
                             int   passPercent,
                             int   seed)
        {
            _roomWidth            = roomWidth;
            _roomHeight           = roomHeight;
            _roomCount            = roomCount;
            _distanceBetweenRooms = distanceBetweenRooms;
            _passWidth            = passWidth;
            _passPercent          = passPercent;
            _seed                 = seed == -1 ? new Random().Next(int.MinValue, int.MaxValue) : seed;
            _rand                 = new Random(_seed);
            if (GenerateRooms())
            {
                GeneratePasses();
                var area = GetCurrentArea();
                Width   = area.width;
                Height  = area.height;
                IsReady = true;
            }
        }

        public RectInt GetCurrentArea() => Rooms.Select(_ => _.Bounds).UnionRectIntList();

        public int[,] ToArray()
        {
            var area = GetCurrentArea();
            var arr  = new int[area.width, area.height];
            foreach (var x in Rooms)
            {
                foreach (var c in x.Bounds.allPositionsWithin)
                {
                    arr[c.x, c.y] = 1;
                }
            }

            foreach (var x in Passes)
            {
                foreach (var c in x.LineList)
                {
                    foreach (var v in c.allPositionsWithin)
                    {
                        arr[v.x, v.y] = 1;
                    }
                }
            }

            //var map = ToMap();
            //var pf  = new RogueSharp.PathFinder(map, Math.Sqrt(2));

            //var path = pf.TryFindShortestPath(map.GetCell((int)Rooms[0].MidPoint.x - 8, (int)Rooms[0].MidPoint.y - 10), map.GetCell((int)Rooms[1].MidPoint.x, (int)Rooms[1].MidPoint.y));
            //foreach (var x in path.Steps)
            //{
            //    arr[x.X, x.Y] = 2;
            //}
            //var pf = PathFind.FindPath((int)Rooms[0].MidPoint.x, (int)Rooms[0].MidPoint.y, (int)Rooms[0].MidPoint.x + 5, (int)Rooms[0].MidPoint.y + 5, new PathFinderAgent(arr)); 
            //foreach (var x in pf.Paths)
            //{
            //    arr[x.x, x.y] = 2;
            //}
            return arr;
        }

        public Map<Cell> ToMap()
        {
            var area = GetCurrentArea();
            var map  = new RogueSharp.Map(area.width, area.height);
            foreach (var x in Rooms)
            {
                foreach (var c in x.Bounds.allPositionsWithin)
                {
                    map.SetCellProperties(c.x, c.y, true, true);
                }
            }

            foreach (var x in Passes)
            {
                foreach (var c in x.LineList)
                {
                    foreach (var v in c.allPositionsWithin)
                    {
                        map.SetCellProperties(v.x, v.y, true, true);
                    }
                }
            }

            return map;
        }

        public Map<Cell> ToMapExcludePass(Pass pass)
        {
            var area = GetCurrentArea();
            var map  = new RogueSharp.Map(area.width, area.height);
            foreach (var x in Rooms)
            {
                foreach (var c in x.Bounds.allPositionsWithin)
                {
                    map.SetCellProperties(c.x, c.y, true, true);
                }
            }

            foreach (var x in Passes)
            {
                if (x == pass)
                    continue;
                foreach (var c in x.LineList)
                {
                    foreach (var v in c.allPositionsWithin)
                    {
                        map.SetCellProperties(v.x, v.y, true, true);
                    }
                }
            }

            return map;
        }

        #region Room

        private bool GenerateRooms(int testSet = -1)
        {
            if (testSet == -1)
            {
                var rc = _rand.GetRand(_roomCount);
                while (Rooms.Count < rc)
                {
                    if (Rooms.Count == 0)
                        Rooms.Add(new Room(new RectInt(0, 0, _rand.GetRand(_roomWidth), _rand.GetRand(_roomHeight)), (Rooms.Count + 1).ToString()));
                    else
                    {
                        var room = RepeatableCode.RepeatResult(() =>
                                                               {
                                                                   var room = GenerateRoom((Rooms.Count + 1).ToString());
                                                                   return CheckRoom(room) ? room : null;
                                                               }, 100000);

                        if (room == null)
                            return false;

                        Rooms.Add(room);
                    }

                    NormalizeRooms();
                }

                return true;
            }
            else if (testSet == 0)
            {
                Rooms.Add(new Room(new RectInt(new Vector2Int(30, 30), new Vector2Int(16, 16)), "Center"));

                //Rooms.Add(new Room(new RectInt(new Vector2Int(4,  33), new Vector2Int(10, 10)), "1Left"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(33, 4),  new Vector2Int(10, 10)), "1Top"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(62, 33), new Vector2Int(10, 10)), "1Right"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(33, 62), new Vector2Int(10, 10)), "1Bottom"));

                Rooms.Add(new Room(new RectInt(new Vector2Int(0,  54), new Vector2Int(10, 10)), "3LeftBottom"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(0,  12), new Vector2Int(10, 10)), "3LeftTop"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(12, 0),  new Vector2Int(10, 10)), "3TopLeft"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(54, 0),  new Vector2Int(10, 10)), "3TopRight"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(66, 12), new Vector2Int(10, 10)), "3RightTop"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(66, 54), new Vector2Int(10, 10)), "3RightBottom"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(54, 66), new Vector2Int(10, 10)), "3BottomRight"));
                Rooms.Add(new Room(new RectInt(new Vector2Int(12, 66), new Vector2Int(10, 10)), "3BottomLeft"));

                //Rooms.Add(new Room(new RectInt(new Vector2Int(11, 50), new Vector2Int(10, 5)),  "2LeftBottom"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(11, 21), new Vector2Int(10, 5)),  "2LeftTop"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(21, 11), new Vector2Int(5,  10)), "2TopLeft"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(50, 11), new Vector2Int(5,  10)), "2TopRight"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(55, 21), new Vector2Int(10, 5)),  "2RightTop"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(55, 50), new Vector2Int(10, 5)),  "2RightBottom"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(50, 55), new Vector2Int(5,  10)), "2BottomRight"));
                //Rooms.Add(new Room(new RectInt(new Vector2Int(21, 55), new Vector2Int(5,  10)), "2BottomLeft"));

                return true;
            }

            return false;
        }

        private Room GenerateRoom(string name)
        {
            var area = GetGenerateArea();
            return new Room(new RectInt(_rand.GetRand(area.xMin, area.xMax), _rand.GetRand(area.yMin, area.yMax), _rand.GetRand(_roomWidth), _rand.GetRand(_roomHeight)), name);
        }

        public RectInt GetGenerateArea()
        {
            var r = Rooms.Select(_ => _.Bounds).UnionRectIntList();
            return r.ExpandAll(_distanceBetweenRooms.End.Value).ExpandLeft(_roomWidth.End.Value).ExpandTop(_roomHeight.End.Value);
        }

        private bool CheckRoom(Room r) => Rooms.All(_ => _.Bounds.GetDistanceToRect(r.Bounds) >= _distanceBetweenRooms.Start.Value) &&
                                          Rooms.Any(_ => _.Bounds.GetDistanceToRect(r.Bounds) <= _distanceBetweenRooms.End.Value)   &&
                                          Rooms.All(_ => !_.Bounds.Overlaps(r.Bounds))                                              &&
                                          (Rooms.Count < 3 || GetNearestRooms(r).Count > 1);

        public List<Room> GetNearestRooms(Room r) => Rooms.Where(_ => r != _).Where(_ => r.Bounds.GetDistanceToRect(_.Bounds) <= _distanceBetweenRooms.End.Value).ToList();

        public void NormalizeRooms()
        {
            var minX = Rooms.Min(_ => _.Left);
            var minY = Rooms.Min(_ => _.Top);
            foreach (var x in Rooms)
                x.Bounds = x.Bounds.OffsetRectInt(-minX, -minY);
        }

        #endregion

        #region Pass

        private void GeneratePasses()
        {
            var checkedPassed = new List<(Room x, Room c)>();
            var area          = GetCurrentArea();
            foreach (var x in Rooms)
            {
                foreach (var c in GetNearestRooms(x))
                {
                    //if (PassExist(x, c))
                    if (checkedPassed.Contains((x, c)) || checkedPassed.Contains((c, x)))
                        continue;
                    //if (x.Name != "Center") // && !c.Name.Contains("2"))
                    //    continue;
                    //if (!c.Name.Contains("3"))
                    //    continue;
                    var passWidth = _rand.GetRand(_passWidth);
                    var passType  = GetPassPointTypes(x, c, passWidth);
                    var pass      = new Pass(x, c);
                    switch (passType.s.Line)
                    {
                        case RectSide.Left:
                        {
                            switch (passType.s.Side)
                            {
                                case RectSide.Bottom:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Top: //2 LeftBottom - TopRight
                                        {
                                            var passX = _rand.GetRand(c.Right  - (c.Right  - c.Left) / 3, c.Right  - passWidth);
                                            var passY = _rand.GetRand(x.Bottom - (x.Bottom - x.Top)  / 3, x.Bottom - passWidth);

                                            if (passX + passWidth > c.Right || passY + passWidth > x.Bottom)
                                                passWidth = Math.Min(c.Right - passX, x.Bottom - passWidth);

                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY), new Vector2Int(x.Left - passX, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY), new Vector2Int(passWidth,      c.Top - passY)));
                                        }
                                            break;
                                        case RectSide.Right: //3
                                        {
                                            var passX  = _rand.GetRand(c.Right  + (x.Left   - c.Right) / 3, x.Left                         - (x.Left - c.Right) / 3 - passWidth);
                                            var passYx = _rand.GetRand(x.Bottom - (x.Bottom - x.Top)   / 3, x.Bottom                       - passWidth);
                                            var passYc = _rand.GetRand(c.Top,                               c.Top + (c.Bottom - c.Top) / 3 - passWidth);

                                            if (passYx + passWidth > x.Bottom)
                                                passWidth = x.Bottom - passYx;
                                            if (passYc < c.Top)
                                                passYc = c.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYx), new Vector2Int(x.Left - passX, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYx), new Vector2Int(passWidth,      passYc - passYx)));
                                            pass.AddLine(new RectInt(new Vector2Int(c.Right, passYc), new Vector2Int(passX - c.Right        + passWidth, passWidth)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                case RectSide.None: //1
                                {
                                    var topEdge    = Math.Max(x.Top, c.Top);
                                    var bottomEdge = Math.Min(x.Bottom, c.Bottom);
                                    var passY      = _rand.GetRand(topEdge + (bottomEdge - topEdge) / 3, bottomEdge - passWidth - (bottomEdge - topEdge) / 3);

                                    pass.AddLine(bottomEdge - topEdge <= passWidth
                                                     ? new RectInt(new Vector2Int(c.Right, topEdge), new Vector2Int(x.Left - c.Right, bottomEdge - topEdge))
                                                     : new RectInt(new Vector2Int(c.Right, passY),   new Vector2Int(x.Left - c.Right, passWidth)));
                                }
                                    break;
                                case RectSide.Top:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Right: //3
                                        {
                                            var passX  = _rand.GetRand(c.Right + (x.Left - c.Right) / 3, x.Left                         - (x.Left - c.Right) / 3 - passWidth);
                                            var passYx = _rand.GetRand(x.Top,                            x.Top + (x.Bottom - x.Top) / 3 - passWidth);
                                            var passYc = _rand.GetRand(c.Bottom                                                         - (c.Bottom - c.Top) / 3, c.Bottom - passWidth);

                                            if (passYc + passWidth > c.Bottom)
                                                passWidth = c.Bottom - passYc;
                                            if (passYx < x.Top)
                                                passYx = x.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(c.Right, passYc), new Vector2Int(passX - c.Right, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYc), new Vector2Int(passWidth,       passYx - passYc)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYx), new Vector2Int(x.Left                  - passX, passWidth)));
                                        }
                                            break;
                                        case RectSide.Bottom: //2 LeftTop - BottomRight
                                        {
                                            var passX = _rand.GetRand(c.Right - (c.Right - c.Left) / 3, c.Right                        - passWidth);
                                            var passY = _rand.GetRand(x.Top,                            x.Top + (x.Bottom - x.Top) / 3 - passWidth);

                                            if (passX + passWidth > c.Right)
                                                passWidth = c.Right - passX;
                                            if (passY < x.Top)
                                                passY = x.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY),    new Vector2Int(x.Left - passX, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX, c.Bottom), new Vector2Int(passWidth,      passY - c.Bottom)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        case RectSide.Top:
                        {
                            switch (passType.s.Side)
                            {
                                case RectSide.Left:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Right: //2 TopLeft - RightBottom
                                        {
                                            var passX = _rand.GetRand(x.Left, x.Left + (x.Right - x.Left) / 3 - passWidth);
                                            var passY = _rand.GetRand(c.Bottom                                - (c.Bottom - c.Top) / 3, c.Bottom - passWidth);

                                            if (passX < x.Left)
                                                passX = x.Left;
                                            if (passY + passWidth > c.Bottom)
                                                passWidth = c.Bottom - passY;

                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passY), new Vector2Int(passWidth, x.Top - passY)));
                                            pass.AddLine(new RectInt(new Vector2Int(c.Right, passY), new Vector2Int(passX            - c.Right, passWidth)));
                                        }
                                            break;
                                        case RectSide.Bottom: //3
                                        {
                                            var passXx = _rand.GetRand(x.Left, x.Left + (x.Right - x.Left) / 3 - passWidth);
                                            var passXc = _rand.GetRand(c.Right                                 - (c.Right - c.Left)   / 3, c.Right - passWidth);
                                            var passY  = _rand.GetRand(c.Bottom                                + (x.Top   - c.Bottom) / 3, x.Top   - (x.Top - c.Bottom) / 3 - passWidth);

                                            if (passXc + passWidth > c.Right)
                                                passWidth = c.Right - passXc;
                                            if (passXx < x.Left)
                                                passXx = x.Left;

                                            pass.AddLine(new RectInt(new Vector2Int(passXx, passY),    new Vector2Int(passWidth, x.Top - passY)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, passY),    new Vector2Int(passXx           - passXc, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, c.Bottom), new Vector2Int(passWidth,                 passY - c.Bottom)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                case RectSide.None: //1
                                {
                                    var leftEdge  = Math.Max(x.Left, c.Left);
                                    var rightEdge = Math.Min(x.Right, c.Right);
                                    var passX     = _rand.GetRand(leftEdge + (rightEdge - leftEdge) / 3, rightEdge - passWidth - (rightEdge - leftEdge) / 3);

                                    pass.AddLine(rightEdge - leftEdge <= passWidth
                                                     ? new RectInt(new Vector2Int(leftEdge, c.Bottom), new Vector2Int(rightEdge - leftEdge, x.Top - c.Bottom))
                                                     : new RectInt(new Vector2Int(passX,    c.Bottom), new Vector2Int(passWidth,            x.Top - c.Bottom)));
                                }
                                    break;
                                case RectSide.Right:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Bottom: //3
                                        {
                                            var passXx = _rand.GetRand(x.Right - (x.Right - x.Left) / 3, x.Right                         - passWidth);
                                            var passXc = _rand.GetRand(c.Left,                           c.Left + (c.Right - c.Left) / 3 - passWidth);
                                            var passY  = _rand.GetRand(c.Bottom                                                          + (x.Top - c.Bottom) / 3, x.Top - (x.Top - c.Bottom) / 3 - passWidth);

                                            if (passXx + passWidth > x.Right)
                                                passWidth = x.Right - passXx;
                                            if (passXc < c.Left)
                                                passXc = c.Left;

                                            pass.AddLine(new RectInt(new Vector2Int(passXx, passY),    new Vector2Int(passWidth, x.Top - passY)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, passY),    new Vector2Int(passXx           - passXc, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, c.Bottom), new Vector2Int(passWidth,                 passY - c.Bottom + passWidth)));
                                        }
                                            break;
                                        case RectSide.Left: //2
                                        {
                                            var passX = _rand.GetRand(x.Right  - (x.Right  - x.Left) / 3, x.Right  - passWidth);
                                            var passY = _rand.GetRand(c.Bottom - (c.Bottom - c.Top)  / 3, c.Bottom - passWidth);

                                            if (passX + passWidth > x.Right || passY + passWidth > c.Bottom)
                                                passWidth = Math.Min(x.Right - passX, c.Bottom - passY);

                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY), new Vector2Int(passWidth, x.Top - passY)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY), new Vector2Int(c.Left           - passX, passWidth)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        case RectSide.Right:
                        {
                            switch (passType.s.Side)
                            {
                                case RectSide.Top:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Bottom: //2
                                        {
                                            var passY = _rand.GetRand(x.Top,  x.Top  + (x.Bottom - x.Top)  / 3 - passWidth);
                                            var passX = _rand.GetRand(c.Left, c.Left + (c.Right  - c.Left) / 3 - passWidth);

                                            if (passX < c.Left)
                                                passX = c.Left;
                                            if (passY < x.Top)
                                                passY = x.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(x.Right, passY),    new Vector2Int(passX - x.Right, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   c.Bottom), new Vector2Int(passWidth,       passY - c.Bottom + passWidth)));
                                        }
                                            break;
                                        case RectSide.Left: //3
                                        {
                                            var passX  = _rand.GetRand(x.Right + (c.Left - x.Right) / 3, c.Left                         - (c.Left - x.Right) / 3 - passWidth);
                                            var passYx = _rand.GetRand(x.Top,                            x.Top + (x.Bottom - x.Top) / 3 - passWidth);
                                            var passYc = _rand.GetRand(c.Bottom                                                         - (c.Bottom - c.Top) / 3, c.Bottom - passWidth);

                                            if (passYc + passWidth > c.Bottom)
                                                passWidth = c.Bottom - passYc;
                                            if (passYx < x.Top)
                                                passYx = x.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(x.Right, passYx), new Vector2Int(passX - x.Right + passWidth, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYc), new Vector2Int(passWidth,                   passYx - passYc)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYc), new Vector2Int(c.Left                              - passX, passWidth)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                case RectSide.None: //1
                                {
                                    var topEdge    = Math.Max(x.Top, c.Top);
                                    var bottomEdge = Math.Min(x.Bottom, c.Bottom);
                                    var passY      = _rand.GetRand(topEdge + (bottomEdge - topEdge) / 3, bottomEdge - passWidth - (bottomEdge - topEdge) / 3);

                                    pass.AddLine(bottomEdge - topEdge <= passWidth
                                                     ? new RectInt(new Vector2Int(x.Right, topEdge), new Vector2Int(c.Left - x.Right, bottomEdge - topEdge))
                                                     : new RectInt(new Vector2Int(x.Right, passY),   new Vector2Int(c.Left - x.Right, passWidth)));
                                }
                                    break;
                                case RectSide.Bottom:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Left: //3
                                        {
                                            var passX  = _rand.GetRand(x.Right  + (c.Left   - x.Right) / 3, c.Left                         - (c.Left - x.Right) / 3 - passWidth);
                                            var passYx = _rand.GetRand(x.Bottom - (x.Bottom - x.Top)   / 3, x.Bottom                       - passWidth);
                                            var passYc = _rand.GetRand(c.Top,                               c.Top + (c.Bottom - c.Top) / 3 - passWidth);

                                            if (passYx + passWidth > x.Bottom)
                                                passWidth = x.Bottom - passYx;
                                            if (passYc < c.Top)
                                                passYc = c.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(x.Right, passYx), new Vector2Int(passX - x.Right, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYx), new Vector2Int(passWidth,       passYc - passYx)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passYc), new Vector2Int(c.Left                  - passX, passWidth)));
                                        }
                                            break;
                                        case RectSide.Top: //2
                                        {
                                            var passX = _rand.GetRand(c.Left, c.Left + (c.Right - c.Left) / 3 - passWidth);
                                            var passY = _rand.GetRand(x.Bottom                                - (x.Bottom - x.Top) / 3, x.Bottom - passWidth);

                                            if (passX < c.Left)
                                                passX = c.Left;
                                            if (passY + passWidth > x.Bottom)
                                                passWidth = x.Bottom - passY;

                                            pass.AddLine(new RectInt(new Vector2Int(x.Right, passY), new Vector2Int(passX - x.Right, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX,   passY), new Vector2Int(passWidth,       c.Top - passY)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        case RectSide.Bottom:
                        {
                            switch (passType.s.Side)
                            {
                                case RectSide.Right:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Left: //2
                                        {
                                            var passX = _rand.GetRand(x.Right - (x.Right - x.Left) / 3, x.Right                        - passWidth);
                                            var passY = _rand.GetRand(c.Top,                            c.Top + (c.Bottom - c.Top) / 3 - passWidth);

                                            if (passX + passWidth > x.Right)
                                                passWidth = x.Right - passX;
                                            if (passY < c.Top)
                                                passY = c.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(passX, x.Bottom), new Vector2Int(passWidth, passY - x.Bottom)));
                                            pass.AddLine(new RectInt(new Vector2Int(passX, passY),    new Vector2Int(c.Left           - passX, passWidth)));
                                        }
                                            break;
                                        case RectSide.Top: //3
                                        {
                                            var passXx = _rand.GetRand(x.Right - (x.Right - x.Left) / 3, x.Right                         - passWidth);
                                            var passXc = _rand.GetRand(c.Left,                           c.Left + (c.Right - c.Left) / 3 - passWidth);
                                            var passY  = _rand.GetRand(x.Bottom                                                          + (c.Top - x.Bottom) / 3, c.Top - (c.Top - x.Bottom) / 3 - passWidth);

                                            if (passXx + passWidth > x.Right)
                                                passWidth = x.Right - passXx;
                                            if (passXc < c.Left)
                                                passXc = c.Left;

                                            pass.AddLine(new RectInt(new Vector2Int(passXx, x.Bottom), new Vector2Int(passWidth, passY - x.Bottom)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXx, passY),    new Vector2Int(passXc           - passXx, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, passY),    new Vector2Int(passWidth,                 c.Top - passY)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                case RectSide.None: //1
                                {
                                    var leftEdge  = Math.Max(x.Left, c.Left);
                                    var rightEdge = Math.Min(x.Right, c.Right);
                                    var passX     = _rand.GetRand(leftEdge + (rightEdge - leftEdge) / 3, rightEdge - passWidth - (rightEdge - leftEdge) / 3);

                                    pass.AddLine(rightEdge - leftEdge <= passWidth
                                                     ? new RectInt(new Vector2Int(leftEdge, x.Bottom), new Vector2Int(rightEdge - leftEdge, c.Top - x.Bottom))
                                                     : new RectInt(new Vector2Int(passX,    x.Bottom), new Vector2Int(passWidth,            c.Top - x.Bottom)));
                                }
                                    break;
                                case RectSide.Left:
                                {
                                    switch (passType.e.Line)
                                    {
                                        case RectSide.Top: //3
                                        {
                                            var passXx = _rand.GetRand(x.Left, x.Left + (x.Right - x.Left) / 3 - passWidth);
                                            var passXc = _rand.GetRand(c.Right                                 - (c.Right - c.Left)   / 3, c.Right - passWidth);
                                            var passY  = _rand.GetRand(x.Bottom                                + (c.Top   - x.Bottom) / 3, c.Top   - (c.Top - x.Bottom) / 3 - passWidth);

                                            if (passXc + passWidth > c.Right)
                                                passWidth = c.Right - passXc;
                                            if (passXx < x.Left)
                                                passXx = x.Left;

                                            pass.AddLine(new RectInt(new Vector2Int(passXx, x.Bottom), new Vector2Int(passWidth, passY - x.Bottom + passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, passY),    new Vector2Int(passXx                      - passXc, passWidth)));
                                            pass.AddLine(new RectInt(new Vector2Int(passXc, passY),    new Vector2Int(passWidth,                            c.Top - passY)));
                                        }
                                            break;
                                        case RectSide.Right: //2
                                        {
                                            var passX = _rand.GetRand(x.Left, x.Left + (x.Right  - x.Left) / 3 - passWidth);
                                            var passY = _rand.GetRand(c.Top,  c.Top  + (c.Bottom - c.Top)  / 3 - passWidth);

                                            if (passX < x.Left)
                                                passX = c.Left;
                                            if (passY < c.Top)
                                                passY = c.Top;

                                            pass.AddLine(new RectInt(new Vector2Int(passX,   x.Bottom), new Vector2Int(passWidth, passY - x.Bottom)));
                                            pass.AddLine(new RectInt(new Vector2Int(c.Right, passY),    new Vector2Int(passX - c.Right  + passWidth, passWidth)));
                                        }
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (!Rooms.Any(_ => pass.LineList.Any(__ => __.Overlaps(_.Bounds))) && !Passes.SelectMany(_ => _.LineList).Any(_ => pass.LineList.Any(__ => __.Overlaps(_))))
                        Passes.Add(pass);

                    checkedPassed.Add((x, c));
                }
            }

            for (var i = 0; i < Passes.Count; i++)
            {
                var x = Passes[i];
                if (x.LineList.Count == 1)
                    continue;
                var mapWithoutPass  = ToMapExcludePass(x);
                var pfWithoutPass   = new RogueSharp.PathFinder(mapWithoutPass, Math.Sqrt(2));
                var pathWithoutPass = pfWithoutPass.TryFindShortestPath(mapWithoutPass.GetCell((int) x.StartRoom.MidPoint.x, (int) x.StartRoom.MidPoint.y), mapWithoutPass.GetCell((int) x.EndRoom.MidPoint.x, (int) x.EndRoom.MidPoint.y));
                if (pathWithoutPass == null)
                    continue;
                var mapWithPass  = ToMap();
                var pfWithPass   = new RogueSharp.PathFinder(mapWithPass, Math.Sqrt(2));
                var pathWithPass = pfWithPass.TryFindShortestPath(mapWithPass.GetCell((int) x.StartRoom.MidPoint.x, (int) x.StartRoom.MidPoint.y), mapWithPass.GetCell((int) x.EndRoom.MidPoint.x, (int) x.EndRoom.MidPoint.y));
                var withoutDist  = pathWithoutPass.GetPathDistance();
                var withDist     = pathWithPass.GetPathDistance();
                if (!(withDist + withDist * _passPercent / 100 > withoutDist))
                    continue;
                Passes.RemoveAt(i);
                i--;
            }
        }

        public (PassPointType s, PassPointType e) GetPassPointTypes(Room x, Room c, int passWidth)
        {
            //vertical
            if (x.Left <= c.Right - passWidth && x.Right >= c.Left + passWidth)
                return (new PassPointType(x.Top > c.Top ? RectSide.Top : RectSide.Bottom, RectSide.None), new PassPointType(x.Top < c.Top ? RectSide.Top : RectSide.Bottom, RectSide.None));

            //horizontal
            if (x.Top <= c.Bottom - passWidth && x.Bottom >= c.Top + passWidth)
                return (new PassPointType(x.Left > c.Left ? RectSide.Left : RectSide.Right, RectSide.None), new PassPointType(x.Left < c.Left ? RectSide.Left : RectSide.Right, RectSide.None));

            //diagonal
            var fIntersections = x.Bounds.GetRectLines().Select(_ => GeometricExtensions.Intersect(_.s, _.e, x.Bounds.center, c.Bounds.center)).ToList();
            var fPoint         = fIntersections.FirstOrDefault(_ => _ != default);
            if (fPoint == default)
                throw new Exception();
            var fIndex = fIntersections.IndexOf(fPoint);

            var sIntersections = c.Bounds.GetRectLines().Select(_ => GeometricExtensions.Intersect(_.s, _.e, c.Bounds.center, x.Bounds.center)).ToList();
            var sPoint         = sIntersections.FirstOrDefault(_ => _ != default);
            if (sPoint == default)
                throw new Exception();
            var sIndex = sIntersections.IndexOf(sPoint);

            return (new PassPointType((RectSide) fIndex, fIndex % 2 == 0 ? x.Bounds.center.y > fPoint.y ? RectSide.Top : RectSide.Bottom : x.Bounds.center.x > fPoint.x ? RectSide.Left : RectSide.Right),
                    new PassPointType((RectSide) sIndex, sIndex % 2 == 0 ? c.Bounds.center.y > sPoint.y ? RectSide.Top : RectSide.Bottom : c.Bounds.center.x > sPoint.x ? RectSide.Left : RectSide.Right));
        }

        //public bool PassExist(Room r1, Room r2) => Passes.Count(_ => (_.StartRoom == r1 && _.EndRoom == r2) || (_.StartRoom == r2 && _.EndRoom == r1)) != 0;

        #endregion
    }
}
