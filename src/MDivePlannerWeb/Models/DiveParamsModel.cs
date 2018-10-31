using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MDivePlannerWeb.Validation;

namespace MDivePlannerWeb.Models
{
    public class DiveParamsModel : IValidatableObject
    {
        [MinValue(1)]
        [Required]
        public double Depth { get; set; }

        [MinValue(1)]
        [Required]
        public double Time { get; set; }

        [MinValue(0, required: false, canBeEqual: true)]
        public double? Interval { get; set; }

        [MinValue(1000, canBeEqual: true)]
        public double WaterDensity { get; set; }


        public bool IsModelValid { get; set; }

        public void FillDefault()
        {
            WaterDensity = 1000;
            Depth = 20;
            Time = 50;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            //results.Add(new ValidationResult("Yo!"));

            IsModelValid = !results.Any();
            return results;
        }
    }
}
