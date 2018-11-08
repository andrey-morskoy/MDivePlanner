using System;

namespace MDivePlanner.Domain.Entities
{
    public class DiveLevel
    {
        public double Time { get; set; }

        public DepthFactor DepthFactor { get; set; }

        public BreathGas Gas { get; set; }

        public double Depth
        {
            get { return DepthFactor.Depth; }
        }
    }
}
