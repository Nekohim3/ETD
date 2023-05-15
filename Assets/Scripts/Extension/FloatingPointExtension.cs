using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extension
{
    public static class FloatingPointExtension
    {
        #region Float

        public static bool IsLessThan(this           float a, float b, float tolerance = 0.00005f) => a - b < -tolerance;
        public static bool IsLessThanOrEqual(this    float a, float b, float tolerance = 0.00005f) => a - b < -tolerance || Math.Abs(a - b) < tolerance;
        public static bool IsGreaterThan(this        float a, float b, float tolerance = 0.00005f) => a - b > tolerance;
        public static bool IsGreaterThanOrEqual(this float a, float b, float tolerance = 0.00005f) => a - b > tolerance || Math.Abs(a - b) < tolerance;
        public static bool IsEqual(this              float a, float b, float tolerance = 0.00005f) => Math.Abs(a - b) < tolerance;

        #endregion

        #region Double

        public static bool IsLessThan(this           double a, double b, double tolerance = 0.00005f) => a - b < -tolerance;
        public static bool IsLessThanOrEqual(this    double a, double b, double tolerance = 0.00005f) => a - b < -tolerance || Math.Abs(a - b) < tolerance;
        public static bool IsGreaterThan(this        double a, double b, double tolerance = 0.00005f) => a - b > tolerance;
        public static bool IsGreaterThanOrEqual(this double a, double b, double tolerance = 0.00005f) => a - b > tolerance || Math.Abs(a - b) < tolerance;
        public static bool IsEqual(this              double a, double b, double tolerance = 0.00005f) => Math.Abs(a - b) < tolerance;

        #endregion
    }
}
