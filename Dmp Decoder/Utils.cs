using System;

namespace Dmp_Decoder
{
    public static class Utils
    {
        public static int HexToInt(this string hex) => Convert.ToInt32(hex, 16);

        public static double MapValue(double x, double xleft, double xright, double resLeft, double resRight)
        {
            if (xleft == xright)
            {
                return resLeft;
            }
            return resLeft + (x - xleft) * (resRight - resLeft) / (xright - xleft);

        }
    }
}
