using MDivePlanner.Domain.Logic;
using System;

namespace MDivePlanner.Domain.Entities
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
                const double percents = 100.0;
                return PpHe < double.Epsilon ?
                    Math.Round(PpO * percents) == 21 ? "Air" : string.Format("EAN {0}", Math.Round(PpO * percents, 1)) :
                    string.Format("Tx {0}/{1}", Math.Round(PpO * percents, 1), Math.Round(PpHe * percents, 1));
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
            PpN = ppN;
            PpHe = ppHe;
        }

        public bool ValidateGas()
        {
            if (PpO < double.Epsilon || PpO > 1.0)
                return false;

            return DivingMath.CompareDouble(PpO + PpHe + PpN, 1.0);
        }

        public static double GetGasPartialPreasureForDepth(DepthFactor depth, double partialPreasure)
        {
            var gasPreasure = DivingMath.BarsToAtm(DivingMath.DepthToPreasureBars(depth));
            return gasPreasure * partialPreasure;
        }

        public override bool Equals(object obj)
        {
            return this.IsEqual(obj as BreathGas);
        }

        public override int GetHashCode()
        {
            return PpO.GetHashCode() ^ PpN.GetHashCode();
        }

        public bool IsEqual(BreathGas gas)
        {
            if (gas == null)
                return false;

            return DivingMath.CompareDouble(PpHe, gas.PpHe) &&
                DivingMath.CompareDouble(PpN, gas.PpN) &&
                DivingMath.CompareDouble(PpO, gas.PpO);
        }
    }
}
