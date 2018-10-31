using System;

namespace MDivePlannerWeb.Domain
{
    public class GasSwitch
    {
        public double Depth { get; set; }

        public double AbsoluteTime { get; set; }

        public BreathGas Gas { get; set; }
    }
}
