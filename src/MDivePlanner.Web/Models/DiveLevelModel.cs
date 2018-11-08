using MDivePlannerWeb.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MDivePlannerWeb.Models
{
    public class DiveLevelModel
    {
        public double? Depth { get; set; }

        public double? Time { get; set; }

        public bool UseLevel { get; set; }

        public bool IsValid { get; set; }

        public GasModel Gas { get; set; } = new GasModel();
    }
}
