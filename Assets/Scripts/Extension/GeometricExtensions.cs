using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Extension
{
    //public enum PassPointType
    //{
    //    LeftBottom,
    //    Left,
    //    LeftTop,
    //    TopLeft,
    //    Top,
    //    TopRight,
    //    RightTop,
    //    Right,
    //    RightBottom,
    //    BottomRight,
    //    Bottom,
    //    BottomLeft
    //}

    public enum RectSide
    {
        None   = -1,
        Left   = 0,
        Top    = 1,
        Right  = 2,
        Bottom = 3
    }

    public struct PassPointType
    {
        public RectSide Line;
        public RectSide Side;

        public PassPointType(RectSide line, RectSide side)
        {
            Line = line;
            Side = side;
        }
    }

    public static class GeometricExtensions
    {
        public static RectInt OffsetRectInt(this RectInt r, int offsetX, int offsetY) => new(new(r.position.x + offsetX, r.position.y + offsetY), r.size);

        public static RectInt UnionRectInt(this RectInt r1, RectInt r2) => new(Vector2Int.Min(r1.min, r2.min), Vector2Int.Max(r1.max, r2.max));

        public static RectInt UnionRectIntList(this IEnumerable<RectInt> list) => list?.Aggregate(new RectInt(), (current, x) => current.UnionRectInt(x)) ?? new RectInt();

        public static RectInt ExpandLeft(this   RectInt r, int n) => new(new(r.position.x - n, r.position.x), new(r.size.x + n, r.size.y));
        public static RectInt ExpandTop(this    RectInt r, int n) => new(new(r.position.x, r.position.x - n), new(r.size.x + n, r.size.y + n));
        public static RectInt ExpandRight(this  RectInt r, int n) => new(new(r.position.x, r.position.x), new(r.size.x + n, r.size.y));
        public static RectInt ExpandBottom(this RectInt r, int n) => new(new(r.position.x, r.position.x), new(r.size.x, r.size.y + n));
        public static RectInt ExpandWidth(this  RectInt r, int width)  => r.ExpandLeft(width).ExpandRight(width);
        public static RectInt ExpandHeight(this RectInt r, int height) => r.ExpandTop(height).ExpandBottom(height);
        public static RectInt ExpandAll(this    RectInt r, int n)      => r.ExpandWidth(n).ExpandHeight(n);

        public static RectInt Normalized(this RectInt r) => new(new(r.size.x < 0 ? r.position.x + r.size.x : r.position.x, r.size.y < 0 ? r.position.y + r.size.y : r.position.y), new(Math.Abs(r.size.x), Math.Abs(r.size.y)));

        public static List<Vector2Int> GetRectPoints(this RectInt r) => new() { new Vector2Int(r.xMin, r.yMin), new Vector2Int(r.xMin, r.yMax), new Vector2Int(r.xMax, r.yMax), new Vector2Int(r.xMax, r.yMin) };

        public static float GetDistanceToRect(this RectInt r, RectInt r1)
        {
            if (r1.xMax > r.xMin+ 2 && r1.xMin< r.xMax- 2)
                return Math.Min(Math.Abs(r.yMin - r1.yMax), Math.Abs(r1.yMin - r.yMax));
            if (r1.yMax > r.yMin + 2 && r1.yMin < r.yMax - 2)
                return Math.Min(Math.Abs(r.xMin - r1.xMax), Math.Abs(r1.xMin - r.xMax));
            return r.GetRectPoints().SelectMany(x => r1.GetRectPoints().Select(c => (x - c).magnitude)).Min();
        }

        public static List<(Vector2Int s, Vector2Int e)> GetRectLines(this RectInt r) => new()
                                                                                         {
                                                                                             (new Vector2Int(r.xMin, r.yMin), new Vector2Int(r.xMin, r.yMax)),
                                                                                             (new Vector2Int(r.xMin, r.yMin), new Vector2Int(r.xMax, r.yMin)),
                                                                                             (new Vector2Int(r.xMax, r.yMin), new Vector2Int(r.xMax, r.yMax)),
                                                                                             (new Vector2Int(r.xMin, r.yMax), new Vector2Int(r.xMax, r.yMax))
                                                                                         };




        public static Vector2 Intersect(Vector2 s1, Vector2 e1, Vector2 s2, Vector2 e2, float tolerance = 0.00005f)
        {
            bool IsInsideLine(Vector2 s, Vector2 e, Vector2 p, float tol)
            {
                float x = p.x, y = p.y;

                var leftX = s.x;
                var leftY = s.y;

                var rightX = e.x;
                var rightY = e.y;

                return ((x.IsGreaterThanOrEqual(leftX, tol) && x.IsLessThanOrEqual(rightX, tol))
                        || (x.IsGreaterThanOrEqual(rightX, tol) && x.IsLessThanOrEqual(leftX, tol)))
                       && ((y.IsGreaterThanOrEqual(leftY, tol) && y.IsLessThanOrEqual(rightY, tol))
                           || (y.IsGreaterThanOrEqual(rightY, tol) && y.IsLessThanOrEqual(leftY, tol)));
            }

            if (s1 == s2 && e1 == e2)
                throw new Exception("Both lines are the same.");

            if (s1.x.CompareTo(s2.x) > 0)
                (s1, e1, s2, e2) = (s2, e2, s1, e1);
            else if (s1.x.CompareTo(s2.x) == 0)
                if (s1.y.CompareTo(s2.y) > 0)
                    (s1, e1, s2, e2) = (s2, e2, s1, e1);

            float x1 = s1.x, y1 = s1.y;
            float x2 = e1.x, y2 = e1.y;
            float x3 = s2.x, y3 = s2.y;
            float x4 = e2.x, y4 = e2.y;

            if (x1.IsEqual(x2) && x3.IsEqual(x4) && x1.IsEqual(x3))
            {
                var firstIntersection = new Vector2(x3, y3);
                if (IsInsideLine(s1, e1, firstIntersection, tolerance) &&
                    IsInsideLine(s2, e2, firstIntersection, tolerance))
                    return new Vector2(x3, y3);
            }

            if (y1.IsEqual(y2) && y3.IsEqual(y4) && y1.IsEqual(y3))
            {
                var firstIntersection = new Vector2(x3, y3);
                if (IsInsideLine(s1, e1, firstIntersection, tolerance) &&
                    IsInsideLine(s2, e2, firstIntersection, tolerance))
                    return new Vector2(x3, y3);
            }

            if (x1.IsEqual(x2) && x3.IsEqual(x4))
                return default;

            if (y1.IsEqual(y2) && y3.IsEqual(y4))
                return default;

            float x, y;
            if (x1.IsEqual(x2))
            {
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = x1;
                y = c2 + m2 * x1;
            }
            else if (x3.IsEqual(x4))
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                if (!((-m1 * x + y).IsEqual(c1)
                      && (-m2 * x + y).IsEqual(c2)))
                    return default;
            }

            var result = new Vector2(x, y);

            if (IsInsideLine(s1, e1, result, tolerance) &&
                IsInsideLine(s2, e2, result, tolerance))
                return result;

            return default;
        }


    }
}
