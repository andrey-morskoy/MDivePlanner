using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MDivePlannerWeb.Validation
{
    public class MinValueAttribute : ValidationAttribute//, IClientModelValidator
    {
        private readonly double _minValue;
        private readonly bool _required;
        private readonly bool _canBeEqual;

        public MinValueAttribute(double minValue, bool required = true, bool canBeEqual = false)
        {
            _required = required;
            _canBeEqual = canBeEqual;
            _minValue = minValue;
            ErrorMessage = canBeEqual ? String.Format("Value must be greater or equal {0}.", _minValue) :
                                        String.Format("Value must be greater than {0}.", _minValue);
        }

        public MinValueAttribute(int minValue, bool required = true, bool canBeEqual = false)
        {
            _required = required;
            _canBeEqual = canBeEqual;
            _minValue = minValue;
            ErrorMessage = canBeEqual ? String.Format("Value must be greater or equal {0}.", _minValue) :
                                        String.Format("Value must be greater than {0}.", _minValue);
        }

        public override bool IsValid(object value)
        {
            if (!_required && value == null)
                return true;

            return _canBeEqual ? Convert.ToDouble(value) >= _minValue : Convert.ToDouble(value) > _minValue;
        }

        /*
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule();
            rule.ErrorMessage = ErrorMessage;
            rule.ValidationParameters.Add("min", _minValue);
            rule.ValidationParameters.Add("max", Double.MaxValue);
            rule.ValidationType = "range";
            yield return rule;
        }*/
    }
}
