using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TargetLines
{
    public static class MathUtils {
        public static readonly float RAD2DEG = 57.29577951308232f;
        public static readonly float DEG2RAD = 0.017453292519943f;

        public static float Lerpf(float lhs, float rhs, float t) {
            return (1 - t) * lhs + t * rhs;
        }
    }
}
