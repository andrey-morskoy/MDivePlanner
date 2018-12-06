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
            var hasDecoLevels = DecoLevels?.Count() > 0;

            var infoBlocks = new List<string>
            {
                string.Format("Depth:  {0} m", Math.Round(Levels.Max(l => l.Depth), 1)),
                string.Format("Time:  {0} min", Math.Round(Levels.Sum(l => l.Time), 1)),
                string.Format("Interval:  {0}", IntervalTime < double.Epsilon ? "first dive" : Math.Round(IntervalTime) + " mins"),
                string.Format("Gases:  level {0}", string.Join(", ", Levels.Select(l => l.Gas.Name)) + 
                    (hasDecoLevels ? ";  deco " + string.Join(", ", DecoLevels.Select(l => l.Gas.Name)) : string.Empty)),
                string.Format("Water:  {0}",  Levels.First().DepthFactor.WaterDensity <= DivingMath.FreshWaterDensity ? "fresh" : "salt"),
                string.Format("Interval:  {0} min", Math.Round(IntervalTime)),
                string.Format("RMV:  {0}/{1} lt/min", Math.Round(DiveConfig.BottomRmv), Math.Round(DiveConfig.DecoRmv)),
                string.Format("Algo:  ZHL-16{0}", DiveConfig.AlgoSubType),
                string.Format("GF:  {0}/{1}", Math.Round(DiveConfig.GradFactorLow * 100.0), Math.Round(DiveConfig.GradFactorHigh * 100.0))
            };

            infoBlocks.RemoveAll(b => string.IsNullOrEmpty(b));
            return infoBlocks;
        }
    }
}
