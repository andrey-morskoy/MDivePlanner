using System;
using System.Linq;
using System.Collections.Generic;
using MDivePlanner.Domain.Logic;

namespace MDivePlanner.Domain.Entities
{
    public class DiveParameters
    {
        public IEnumerable<DiveLevel> Levels { get; set; }

        public IEnumerable<DiveLevel> DecoLevels { get; set; }

        public double IntervalTime { get; set; }

        public DiveConfig DiveConfig { get; set; } = new DiveConfig();

        public IEnumerable<string> GetDiveParamsInfo()
        {
            var decoGasStr = new List<string>();

            foreach (var decoGas in DecoLevels)
            {
                decoGasStr.Add(string.Format("Deco Gas: {0}", decoGas.Gas.Name));
            }

            var infoBlocks = new List<string>
            {
                string.Format("Depth: {0} m", Math.Round(Levels.Max(l => l.Depth), 1)),
                string.Format("Time: {0} min", Math.Round(Levels.Sum(l => l.Time), 1)),
                string.Format("Gases : {0}", string.Join(", ", Levels.Select(l => l.Gas.Name))),
                string.Join(", ", decoGasStr),
                string.Format("Water: {0}",  Levels.First().DepthFactor.WaterDensity <= DivingMath.FreshWaterDensity ? "fresh" : "salt"),
                string.Format("Interval: {0} min", Math.Round(IntervalTime)),
                string.Format("RMV: {0}/{1} lt/min", Math.Round(DiveConfig.BottomRmv), Math.Round(DiveConfig.DecoRmv)),
                //string.Format("Algo: ZHL-16{0}", DiveConfig.AlgoSubType),
                string.Format("GF: {0}/{1}", Math.Round(DiveConfig.GradFactorLow * 100.0), Math.Round(DiveConfig.GradFactorHigh * 100.0))
            };

            infoBlocks.RemoveAll(b => string.IsNullOrEmpty(b));
            return infoBlocks;
        }
    }
}
