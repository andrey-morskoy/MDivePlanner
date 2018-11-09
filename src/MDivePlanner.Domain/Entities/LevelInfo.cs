using System;
using System.Collections.Generic;

namespace MDivePlanner.Domain.Entities
{
    public class LevelInfo
    {
        public double Depth { get; set; }

        public double PpO { get; set; }

        public double END { get; set; }

        public double TimeReached { get; set; }

        public BreathGas Gas { get; set; }
    }
}
