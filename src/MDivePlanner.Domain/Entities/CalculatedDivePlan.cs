using MDivePlanner.Domain.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDivePlanner.Domain.Entities
{
    public class CalculatedDivePlan
    {
        public DiveParameters DiveParameters { get; set; }

        public double MaxDepth { get; set; }

        public DepthTime MaxNoDecoDepthTime { get; set; }

        public DepthTime DynamicNoDecoDepthTime { get; set; }

        public double BottomTime { get; set; }

        public double TotalTime { get; set; }

        public double FullDesaturationTime { get; set; }

        public double IntervalTime { get; set; }

        public double MaxPpO { get; set; }

        public double MaxPpN { get; set; }

        public double MaxEND { get; set; }

        public IEnumerable<ConsumedGas> ConsumedBottomGases { get; set; }

        public IEnumerable<ConsumedGas> ConsumedDecoGases { get; set; }

        public double OxygenCns { get; set; }

        public IEnumerable<DivePlanPoint> PlanPoints { get; set; }

        public IEnumerable<DepthTime> CeilingDepthPoints { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public int DiveIndex { get; set; }

        public object TissuesSaturationData { get; set; }

        public IEnumerable<GasSwitch> GasSwitches { get; set; }

        public IEnumerable<LevelInfo> LevelsInfo { get; set; }

        public bool IsValid
        {
            get { return (Errors == null || Errors.Count() == 0) && this.MaxDepth > 0 && this.BottomTime > 0; }
        }

        public string Description
        {
            get
            {
                if (this.MaxDepth < double.Epsilon && this.BottomTime < double.Epsilon)
                    return "<New Dive>";
                else
                    return $"{DiveIndex}: {Math.Round(this.MaxDepth, 1)}m - {Math.Round(this.BottomTime)}mins";
            }
        }

        public IEnumerable<DiveResultBlock> GetDiveInfo()
        {
            const double maxPpO = 1.4;
            const double maxCns = 100.0;
            const double maxEnd = 60.0;


            var bottomGases = string.Join("/", ConsumedBottomGases.Select(g => $"{Math.Ceiling(g.Amount)}"));
            var decoGases = string.Join("/", ConsumedDecoGases?.Select(g => $"{Math.Ceiling(g.Amount)}") ?? new List<string>());

            var gasStr = string.Format("Consumed Gas: {0} ltr", bottomGases);
            if (ConsumedDecoGases?.Count() > 0)
            {
                gasStr += $", {decoGases} ltr";
            }

            var result = new List<DiveResultBlock>();
            result.Add(new DiveResultBlock(string.Format("Dive Info: {0} metters - {1}/{2} (bottom/total) mins", 
                    Math.Round(MaxDepth), Math.Round(BottomTime), Math.Round(TotalTime)), important: true, type: DiveResultBlockType.DepthTime));

            var hasDeco = PlanPoints.Any(p => (p.Type & DivePlanPointType.Deco) == DivePlanPointType.Deco);
            result.Add(new DiveResultBlock(hasDeco ? "With Deco" : "No Deco", warning: hasDeco));

            result.Add(new DiveResultBlock(string.Format("Max ppO: {0} ata", Utils.DoubleToString(MaxPpO)), MaxPpO > maxPpO, type: DiveResultBlockType.MaxPpO));
            result.Add(new DiveResultBlock(gasStr, type: DiveResultBlockType.ConsumedGas));
            result.Add(new DiveResultBlock(string.Format("CNS: {0}%", Utils.DoubleToString(OxygenCns)), OxygenCns >= maxCns, type: DiveResultBlockType.CNS));
            result.Add(new DiveResultBlock(string.Format("END: {0} m", Utils.DoubleToString(MaxEND)), warning: MaxEND > maxEnd, type: DiveResultBlockType.END));
            result.Add(new DiveResultBlock(string.Format("Max No Deco Time: {0} mins", Math.Round(MaxNoDecoDepthTime.Time)), type: DiveResultBlockType.NoDecoTime));
            result.Add(new DiveResultBlock(string.Format("Acsent Time: {0} mins", Math.Round(TotalTime - BottomTime)), type: DiveResultBlockType.AscentTime));
            result.Add(new DiveResultBlock(string.Format("Full Desaturation: {0} hours", Math.Round(FullDesaturationTime / 60)), type: DiveResultBlockType.FullDesaturation));

            return result;
        }
    }
}
