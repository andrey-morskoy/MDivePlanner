using System;
using System.Collections.Generic;

namespace MDivePlannerWeb.Domain
{
    public class DiveParameters
    {
        public DepthFactor DepthFactor { get; set; }

        public double Time { get; set; }

        public double IntervalTime { get; set; }

        public BreathGas Gas { get; set; }

        public IEnumerable<BreathGas> DecoGases { get; set; } = new List<BreathGas>();

        public DiveConfig DiveConfig { get; set; } = new DiveConfig();

        public IEnumerable<string> GetDiveParamsInfo()
        {
            var decoGasStr = new List<string>();

            foreach (var decoGas in DecoGases)
                decoGasStr.Add(string.Format("Deco Gas: {0}", decoGas.Name));

            var infoBlocks = new List<string>
            {
                string.Format("Depth: {0} m", Math.Round(DepthFactor.Depth, 1)),
                string.Format("Time: {0} min", Math.Round(Time, 1)),
                string.Format("Gas : {0}", Gas.Name),
                string.Join(", ", decoGasStr),
                string.Format("Water: {0}", DepthFactor.WaterDensity <= DivingMath.FreshWaterDensity ? "fresh" : "salt"),
                string.Format("Interval: {0} min", Math.Round(IntervalTime)),
                string.Format("RMV: {0}/{1} lt/min", Math.Round(DiveConfig.BottomRmv), Math.Round(DiveConfig.DecoRmv)),
                string.Format("Algo: ZHL-16{0}", DiveConfig.AlgoSubType),
                string.Format("GF: {0}/{1}", Math.Round(DiveConfig.GradFactorLow * 100.0), Math.Round(DiveConfig.GradFactorHigh * 100.0))
            };

            infoBlocks.RemoveAll(b => string.IsNullOrEmpty(b));
            return infoBlocks;
        }
    }
}
