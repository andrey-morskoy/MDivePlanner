using FluentValidation;
using MDivePlanner.Domain.Entities;
using MDivePlannerWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MDivePlannerWeb.Validation
{
    public class DiveParamsBasicValidator : AbstractValidator<DiveParamsModel>
    {
        public DiveParamsBasicValidator()
        {
            RuleFor(m => m.WaterDensity).NotEmpty();
            RuleFor(m => m.Algorythm).NotEmpty();
            RuleFor(m => m.MinDecoStopTime).NotEmpty();
            RuleFor(m => m.AscentSpeed).NotEmpty();
            RuleFor(m => m.DescentSpeed).NotEmpty();
            RuleFor(m => m.GradFactorHigh).NotEmpty();
            RuleFor(m => m.GradFactorLow).NotEmpty();
            RuleFor(m => m.SafetyStopDepth).NotEmpty();
            RuleFor(m => m.SafetyStopTime).NotEmpty();
            RuleFor(m => m.RmvBottom).NotEmpty();
            RuleFor(m => m.RmvDeco).NotEmpty();
        }

        public static IEnumerable<ValidationResult> ValidateModel(DiveParamsModel model)
        {
            const double minDepth = 3;
            const double minTime = 1;
            const double maxAscentSpeed = 20;
            const double maxDescentSpeed = 30;
            const double minAscentDescentSpeed = 5;
            const int minPpO = 5;
            const int maxPpO = 100;
            const int minWaterDensity = 1000;
            const int maxWaterDensity = 1100;
            const int minSafetyStopTime = 1;
            const int maxSafetyStopTime = 5;
            const double minSafetyStopDepth = 3;
            const double maxSafetyStopDepth = 6;
            const int minGradFactor = 10;
            const int maxGradFactor = 100;
            const int minRmv = 10;
            const int maxRmv = 100;
            const int minInterval = 5;

            var errors = new List<ValidationResult>();

            model.LevelsValidationMessage = string.Empty;
            model.DecoLevelsValidationMessage = string.Empty;

            Action<string, string> addError = (error, field) =>
            {
                errors.Add(new ValidationResult(error, new List<string> { field }));
            };

            Func<DiveLevelModel, string> validateGas = level =>
            {
                var ppO2 = (level.Gas.PpO2 ?? 0) * 0.01;
                var ppHe = (level.Gas.PpHe ?? 0) * 0.01;
                if ((level.Gas.PpO2 ?? 0) < minPpO || (level.Gas.PpO2 ?? 0) > maxPpO)
                    return $"Gas ppO is out of range [{minPpO} - {maxPpO}]%";
                if (!(level.IsValid = new BreathGas(ppO2, 1.0 - ppO2 - ppHe, ppHe).ValidateGas()))
                    return "Invalid Gas values/proportions";
                return null;
            };

            foreach (var level in model.DiveLevels.Where(l => l.UseLevel))
            {
                if ((level.Depth ?? 0) < minDepth || (level.Time ?? 0) < minTime)
                {
                    model.LevelsValidationMessage = $"Depth & Time must be greater than {minDepth}m / {minTime} mins";
                    break;
                }

                model.LevelsValidationMessage = validateGas(level);
                if (!string.IsNullOrEmpty(model.LevelsValidationMessage))
                    break;
            }

            foreach (var level in model.DecoLevels.Where(l => l.UseLevel))
            {
                if (level.Depth.HasValue && level.Depth < minDepth)
                {
                    model.DecoLevelsValidationMessage = $"Depth must be greater than {minDepth}m";
                    break;
                }

                model.DecoLevelsValidationMessage = validateGas(level);
                if (!string.IsNullOrEmpty(model.DecoLevelsValidationMessage))
                    break;
            }

            if (model.DecoLevels.Any(d => d.Depth > double.Epsilon))
            {
                if (!model.DecoLevels.Where(d => d.UseLevel).All(d => d.Depth > double.Epsilon))
                {
                    model.DecoLevelsValidationMessage = "Explicit depth must be present for all deco gases";
                }
            }

            if (!model.DiveLevels.Any(l => l.UseLevel))
                model.LevelsValidationMessage = "Please add at least one level";

            if (model.DescentSpeed < minAscentDescentSpeed || model.DescentSpeed > maxDescentSpeed)
                addError($"Descent speed is out of range [{minAscentDescentSpeed} - {maxDescentSpeed}]", nameof(model.DescentSpeed));
            if (model.AscentSpeed < minAscentDescentSpeed || model.AscentSpeed > maxAscentSpeed)
                addError($"Ascent speed speed is out of range [{minAscentDescentSpeed} - {maxAscentSpeed}]", nameof(model.AscentSpeed));
            if (model.WaterDensity < minWaterDensity || model.WaterDensity > maxWaterDensity)
                addError($"Water density is out of range [{minWaterDensity} - {maxWaterDensity}] kg/m3", nameof(model.WaterDensity));
            if (model.SafetyStopDepth < minSafetyStopDepth || model.SafetyStopDepth > maxSafetyStopDepth)
                addError($"Safety stop depth is out of range [{minSafetyStopDepth} - {maxSafetyStopDepth}] m", nameof(model.SafetyStopDepth));
            if (model.SafetyStopTime < minSafetyStopTime || model.SafetyStopTime > maxSafetyStopTime)
                addError($"Safety stop time is out of range [{minSafetyStopTime} - {maxSafetyStopTime}] mins", nameof(model.SafetyStopTime));
            if (model.GradFactorHigh < minGradFactor || model.GradFactorHigh > maxGradFactor)
                addError($"Gradient factor high is out of range [{minGradFactor} - {maxGradFactor}] %", nameof(model.GradFactorHigh));
            if (model.GradFactorLow < minGradFactor || model.GradFactorLow > maxGradFactor)
                addError($"Gradient factor low is out of range [{minGradFactor} - {maxGradFactor}] %", nameof(model.GradFactorLow));
            if (model.GradFactorLow > model.GradFactorHigh)
                addError("Gradient factor 'low' must be <= 'high' value", nameof(model.GradFactorLow));
            if (model.RmvBottom < minRmv || model.RmvBottom > maxRmv)
                addError($"Bottom RMV is out of range [{minRmv} - {maxRmv}] ltrs/min", nameof(model.RmvBottom));
            if (model.RmvDeco < minRmv || model.RmvDeco > maxRmv)
                addError($"Deco RMV is out of range [{minRmv} - {maxRmv}] ltrs/min", nameof(model.RmvDeco));
            if (model.Interval.HasValue && model.Interval < minInterval)
                addError($"Interval must be greater than {minInterval} mins", nameof(model.Interval));

            model.IsModelValid = !errors.Any() && string.IsNullOrEmpty(model.LevelsValidationMessage) && string.IsNullOrEmpty(model.DecoLevelsValidationMessage);

            if (!model.IsModelValid)
                addError($"Please correct the errors above", string.Empty);

            return errors;

        }
    }
}
