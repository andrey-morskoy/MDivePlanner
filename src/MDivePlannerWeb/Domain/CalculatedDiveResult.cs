using System;
using System.Collections.Generic;

namespace MDivePlannerWeb.Domain
{
    public class DecoStopResult
    {
        public double Depth { get; set; }

        public double Time { get; set; }

        public BreathGas Gas { get; set; }
    }


    public class CalculatedDiveResult
    {
        public int SaturationIndex { get; set; }

        public KeyValuePair<double, double> NoDecoDepthTime { get; set; }

        public IEnumerable<DecoStopResult> DecoStops { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public double MaxAscendSpeed { get; set; }

        public double DiveTotalTime { get; set; }

        public double FullDesaturationTime { get; set; }

        public IEnumerable<KeyValuePair<double, double>> MValues { get; set; }

        public IEnumerable<DivePoint> DivePoints { get; set; }

        public object TissuesSaturationData { get; set; }
    }
}
