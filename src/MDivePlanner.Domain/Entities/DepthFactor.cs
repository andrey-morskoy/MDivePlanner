using System;

namespace MDivePlanner.Domain.Entities
{
    public struct DepthFactor
    {
        public double Depth { get; set; }

        public double WaterDensity { get; set; }

        public DepthFactor(double depth, double waterDensity)
        {
            Depth = depth;
            WaterDensity = waterDensity;
        }
    }
}
