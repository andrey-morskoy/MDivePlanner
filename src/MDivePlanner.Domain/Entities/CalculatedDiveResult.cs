using System;
using System.Collections.Generic;

namespace MDivePlanner.Domain.Entities
{
    public struct AlgoResult
    {
        public double N2Presure;
        public double HePresure;
    }


    public class CalculatedDiveResult
    {
        public int SaturationIndex { get; set; }

        public DepthTime MaxNoDecoDepthTime { get; set; }

        public DepthTime DynamicNoDecoDepthTime { get; set; }

        public IEnumerable<DiveLevel> DecoStops { get; set; }

        public double MaxAscendSpeed { get; set; }

        public double DiveTotalTime { get; set; }

        public double FullDesaturationTime { get; set; }

        public IEnumerable<DepthTime> CeilingDepthPoints { get; set; }

        public IEnumerable<DivePoint> DivePoints { get; set; }

        public IEnumerable<AlgoResult> TissuesSaturationData { get; set; }

        public IEnumerable<string> Errors { get; set; }
    }
}
