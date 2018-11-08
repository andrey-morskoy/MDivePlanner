using System;

namespace MDivePlannerWeb.Domain
{
    public class DivePlanPoint
    {
        public double Depth { get; set; }

        public double AbsoluteTime { get; set; }

        public double Duration { get; set; }

        public BreathGas Gas { get; set; }

        // add ppO, end

        public DivePlanPointType Type { get; set; }
    }
}
