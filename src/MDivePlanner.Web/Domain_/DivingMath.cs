using System;

namespace MDivePlannerWeb.Domain
{
    public static class DivingMath
    {
        public const double SeaLevelPreasureBars = 1.01325;
        public const double FreshWaterDensity = 1000.0;
        public const double SaltWaterDensity = 1030.0;
        public const double GravityG = 9.81;
        public const double PascalsPerBar = 100000.0;
        public const double WaterVapourPreasureBars = 0.023; // for t 20C

        public static double DepthToPreasureBars(DepthFactor depthFactor)
        {
            if (depthFactor.Depth <= 0)
                return SeaLevelPreasureBars;

            if (depthFactor.WaterDensity == default(double))
                depthFactor.WaterDensity = FreshWaterDensity;

            var weightDensity = depthFactor.WaterDensity * GravityG;
            return SeaLevelPreasureBars + depthFactor.Depth * weightDensity / PascalsPerBar;
        }

        public static DepthFactor PreasureBarsToDepth(double preasure, double waterDensity = FreshWaterDensity)
        {
            if (preasure <= SeaLevelPreasureBars)
                return new DepthFactor(0, waterDensity);

            preasure = preasure - SeaLevelPreasureBars;
            var weightDensity = waterDensity * GravityG;

            return new DepthFactor(preasure * PascalsPerBar / weightDensity, waterDensity);
        }

        public static double BarsToAtm(double bars)
        {
            return bars / SeaLevelPreasureBars;
        }

        public static bool CompareDouble(double val, double comp)
        {
            return val >= (comp - double.Epsilon) && val <= (comp + double.Epsilon);
        }

        public static double CalculateEND(DepthFactor depthF, BreathGas gas)
        {
            var air = new BreathGas();
            var ppN = DivingMath.SeaLevelPreasureBars * BreathGas.GetGasPartialPreasureForDepth(depthF, gas.PpN);
            var depth = DivingMath.PreasureBarsToDepth(ppN / air.PpN, depthF.WaterDensity);

            return Math.Round(depth.Depth, 1);
        }
    }
}
