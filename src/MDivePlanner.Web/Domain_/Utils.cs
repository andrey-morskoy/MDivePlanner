using System;
using System.Collections.Generic;
using System.Globalization;

namespace MDivePlannerWeb.Domain
{
    public static class Utils
    {
        private static readonly CultureInfo defaultCulture = new CultureInfo("en-US");

        public static string DoubleToString(double value, int roundDigits = 2)
        {
            return Math.Round(value, roundDigits).ToString(defaultCulture);
        }
    }
}
