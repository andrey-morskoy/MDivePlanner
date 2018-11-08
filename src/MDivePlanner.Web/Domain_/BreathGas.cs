using System;

namespace MDivePlannerWeb.Domain
{
    public class BreathGas
    {
        const double AirPpO = 0.21;
        const double AirPpN = 0.79;

        public double PpO { get; set; }

        public double PpN { get; set; }

        public double PpHe { get; set; }

        public string Name
        {
            get
            {
                if (PpHe < double.Epsilon)
                    return Math.Round(PpO * 100.0) == 21 ? "Air" : string.Format("EAN {0}", Math.Round(PpO * 100.0, 1));
                else
                    return string.Format("Tx {0}/{1}", Math.Round(PpO * 100.0, 1), Math.Round(PpHe * 100.0, 1));
            }
        }

        public BreathGas()
        {
            // air by default
            PpO = AirPpO;
            PpN = AirPpN;
            PpHe = 0;
        }

        public BreathGas(double ppO, double ppN, double ppHe)
        {
            PpO = ppO;
            PpN = Math.Abs(ppN);
            PpHe = Math.Abs(ppHe);
        }

        public bool Validate()
        {
            if (PpO < double.Epsilon || PpO > 1.0)
                return false;

            return DivingMath.CompareDouble(PpO + PpHe + PpN, 1.0);
        }

        public static double GetGasPartialPreasureForDepth(DepthFactor depth, double partPreasure)
        {
            var gasPreasure = DivingMath.BarsToAtm(DivingMath.DepthToPreasureBars(depth));
            return gasPreasure * partPreasure;
        }

        public bool CompareTo(BreathGas gas)
        {
            return DivingMath.CompareDouble(PpHe, gas.PpHe) &&
                DivingMath.CompareDouble(PpN, gas.PpN) &&
                DivingMath.CompareDouble(PpO, gas.PpO);
        }
    }
}
