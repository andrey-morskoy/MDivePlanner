using MDivePlanner.Domain.Entities;
using System;

namespace MDivePlannerWeb.Models
{
    public class GasModel
    {
        public double? PpO2 { get; set; }

        public double? PpHe { get; set; }

        public BreathGas GetGas()
        {
            var ppO2 = (PpO2 ?? 0) * 0.01;
            var ppHe = (PpHe ?? 0) * 0.01;
            return new BreathGas(ppO2, 1.0 - ppO2 - ppHe, ppHe);
        }
    }
}
