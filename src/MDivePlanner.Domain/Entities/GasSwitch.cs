using System;

namespace MDivePlanner.Domain.Entities
{
    public class GasSwitch
    {
        public double Depth { get; set; }

        public double AbsoluteTime { get; set; }

        public double PpO { get; set; }

        public BreathGas Gas { get; set; }

        public bool IsDeco { get; set; }
    }
}
