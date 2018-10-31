﻿using System;

namespace MDivePlannerWeb.Domain
{
    public struct DivePoint
    {
        public DepthFactor DepthFactor { get; set; }

        public double CurrentDiveTime { get; set; }

        public BreathGas CurrentGas { get; set; }

        public bool IsEmpty()
        {
            return CurrentGas == null && CurrentDiveTime <= double.Epsilon && DepthFactor.WaterDensity == 0;
        }
    }
}
