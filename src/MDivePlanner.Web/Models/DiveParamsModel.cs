using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MDivePlannerWeb.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using MDivePlanner.Domain.Logic;
using static MDivePlanner.Domain.Entities.DiveConfig;
using MDivePlanner.Domain.Entities;

namespace MDivePlannerWeb.Models
{
    public class DiveParamsModel : IValidatableObject
    {
        public List<SelectListItem> Algorythms { private set; get; }

        public List<SelectListItem> MinDecoStops { private set; get; }

        public DiveLevelModel[] DiveLevels { get; set; }

        public DiveLevelModel[] DecoLevels { get; set; }

        public double? Interval { get; set; }

        [Required]
        public double WaterDensity { get; set; }

        [Required]
        public string Algorythm { get; set; }

        [Required]
        public string MinDecoStopTime { get; set; }

        [Required]
        public double AscentSpeed { get; set; }

        [Required]
        public double DescentSpeed { get; set; }

        [Required]
        public double GradFactorLow { get; set; }

        [Required]
        public double GradFactorHigh { get; set; }

        [Required]
        public double SafetyStopDepth { get; set; }

        [Required]
        public double SafetyStopTime { get; set; }

        [Required]
        public double RmvBottom { get; set; }

        [Required]
        public double RmvDeco { get; set; }

        public bool IsModelValid { get; set; }

        public string LevelsValidationMessage { get; set; }

        public string DecoLevelsValidationMessage { get; set; }

        public DiveParamsModel()
        {
            const int maxLevels = 3;
            DecoLevels = new DiveLevelModel[maxLevels];
            DiveLevels = new DiveLevelModel[maxLevels];

            for (int i = 0; i < maxLevels; i++)
            {
                DiveLevels[i] = new DiveLevelModel();
                DecoLevels[i] = new DiveLevelModel();
            }

            Algorythms = new List<SelectListItem>
            {
                new SelectListItem { Value = Zhl16Algorithm.SubType.A.ToString(), Text = "ZHL-16A" },
                new SelectListItem { Value = Zhl16Algorithm.SubType.B.ToString(), Text = "ZHL-16B" },
                new SelectListItem { Value = Zhl16Algorithm.SubType.C.ToString(), Text = "ZHL-16C" },
            };

            MinDecoStops = new List<SelectListItem>
            {
                new SelectListItem { Value = MinDecoTimeStep.HalfMin.ToString(), Text = "30 seconds" },
                new SelectListItem { Value = MinDecoTimeStep.FullMin.ToString(), Text = "1 min" }
            };
        }

        public void FillDefault()
        {
            DiveLevels[0].UseLevel = true;
            DiveLevels[0].Gas.PpO2 = 21;
            DiveLevels[0].Gas.PpHe = 0;
            DiveLevels[0].Depth = 40;
            DiveLevels[0].Time = 30;
            DiveLevels[0].IsValid = true;

            DecoLevels[1].UseLevel = true;
            DecoLevels[1].Gas.PpO2 = 32;
            DecoLevels[1].Gas.PpHe = 0;
            DecoLevels[1].Depth = 10;
            DecoLevels[1].IsValid = true;

            DecoLevels[0].UseLevel = true;
            DecoLevels[0].Gas.PpO2 = 21;
            DecoLevels[0].Gas.PpHe = 0;
            DecoLevels[0].Depth = 20;
            DecoLevels[0].IsValid = true;

            MinDecoStopTime = MinDecoStops[0].Value;
            WaterDensity = 1000;
            GradFactorLow = 40;
            GradFactorHigh = 80;
            AscentSpeed = 10;
            DescentSpeed = 20;
            SafetyStopDepth = 5;
            SafetyStopTime = 3;
            RmvBottom = 20; 
            RmvDeco = 18;
            Algorythm = Zhl16Algorithm.SubType.C.ToString();
        }

        public DiveParameters GetDiveParameters()
        {
            var diveParams = new DiveParameters();
            var levels = new List<DiveLevel>(DiveLevels.Length);
            var decoLevels = new List<DiveLevel>(DecoLevels.Length);

            diveParams.DiveConfig.AlgoSubType = Enum.Parse<Zhl16Algorithm.SubType>(Algorythm);
            diveParams.DiveConfig.MinDecoTime = Enum.Parse<MinDecoTimeStep>(MinDecoStopTime);
            diveParams.DiveConfig.BottomRmv = RmvBottom;
            diveParams.DiveConfig.DecoRmv = RmvDeco;
            diveParams.DiveConfig.GradFactorHigh = GradFactorHigh * 0.01;
            diveParams.DiveConfig.GradFactorLow = GradFactorLow * 0.01;
            diveParams.DiveConfig.MaxAscentSpeed = AscentSpeed;
            diveParams.DiveConfig.MaxDescentSpeed = DescentSpeed;
            diveParams.DiveConfig.SafeStopDepth = SafetyStopDepth;
            diveParams.DiveConfig.SafeStopTime = SafetyStopTime;

            foreach (var levelModel in DiveLevels.Where(l => l.IsValid && l.UseLevel))
            {
                levels.Add(new DiveLevel
                {
                    DepthFactor = new DepthFactor(levelModel.Depth.Value, WaterDensity),
                    Time = levelModel.Time.Value,
                    Gas = levelModel.Gas.GetGas(),
                });
            }

            foreach (var levelModel in DecoLevels.Where(l => l.IsValid && l.UseLevel))
            {
                decoLevels.Add(new DiveLevel
                {
                    DepthFactor = new DepthFactor(levelModel.Depth ?? 0, WaterDensity),
                    Gas = levelModel.Gas.GetGas(),
                });
            }

            diveParams.DecoLevels = decoLevels;
            diveParams.Levels = levels;
            diveParams.IntervalTime = Interval ?? 0;

            return diveParams;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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

            LevelsValidationMessage = string.Empty;
            DecoLevelsValidationMessage = string.Empty;

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

            foreach (var level in DiveLevels.Where(l => l.UseLevel))
            {
                if ((level.Depth ?? 0) < minDepth || (level.Time ?? 0) < minTime)
                {
                    LevelsValidationMessage = $"Depth & Time must be greater than {minDepth}m / {minTime} mins";
                    break;
                }

                LevelsValidationMessage = validateGas(level);
                if (!string.IsNullOrEmpty(LevelsValidationMessage))
                    break;
            }

            foreach (var level in DecoLevels.Where(l => l.UseLevel))
            {
                if (level.Depth.HasValue && level.Depth < minDepth)
                {
                    DecoLevelsValidationMessage = $"Depth must be greater than {minDepth}m";
                    break;
                }

                DecoLevelsValidationMessage = validateGas(level);
                if (!string.IsNullOrEmpty(DecoLevelsValidationMessage))
                    break;
            }

            if (DecoLevels.Any(d => d.Depth > double.Epsilon))
            {
                if (!DecoLevels.Where(d => d.UseLevel).All(d => d.Depth > double.Epsilon))
                {
                    DecoLevelsValidationMessage = "Explicit depth must be present for all deco gases";
                }
            }

            if (!DiveLevels.Any(l => l.UseLevel))
                LevelsValidationMessage = "Please add at least one level";

            if (DescentSpeed < minAscentDescentSpeed || DescentSpeed > maxDescentSpeed)
                addError($"Descent speed is out of range [{minAscentDescentSpeed} - {maxDescentSpeed}]", nameof(DescentSpeed));
            if (AscentSpeed < minAscentDescentSpeed || AscentSpeed > maxAscentSpeed)
                addError($"Ascent speed speed is out of range [{minAscentDescentSpeed} - {maxAscentSpeed}]", nameof(AscentSpeed));
            if (WaterDensity < minWaterDensity || WaterDensity > maxWaterDensity)
                addError($"Water density is out of range [{minWaterDensity} - {maxWaterDensity}] kg/m3", nameof(WaterDensity));
            if (SafetyStopDepth < minSafetyStopDepth || SafetyStopDepth > maxSafetyStopDepth)
                addError($"Safety stop depth is out of range [{minSafetyStopDepth} - {maxSafetyStopDepth}] m", nameof(SafetyStopDepth));
            if (SafetyStopTime < minSafetyStopTime || SafetyStopTime > maxSafetyStopTime)
                addError($"Safety stop time is out of range [{minSafetyStopTime} - {maxSafetyStopTime}] mins", nameof(SafetyStopTime));
            if (GradFactorHigh < minGradFactor || GradFactorHigh > maxGradFactor)
                addError($"Gradient factor high is out of range [{minGradFactor} - {maxGradFactor}] %", nameof(GradFactorHigh));
            if (GradFactorLow < minGradFactor || GradFactorLow > maxGradFactor)
                addError($"Gradient factor low is out of range [{minGradFactor} - {maxGradFactor}] %", nameof(GradFactorLow));
            if (GradFactorLow > GradFactorHigh)
                addError("Gradient factor 'low' must be <= 'high' value", nameof(GradFactorLow));
            if (RmvBottom < minRmv || RmvBottom > maxRmv)
                addError($"Bottom RMV is out of range [{minRmv} - {maxRmv}] ltrs/min", nameof(RmvBottom));
            if (RmvDeco < minRmv || RmvDeco > maxRmv)
                addError($"Deco RMV is out of range [{minRmv} - {maxRmv}] ltrs/min", nameof(RmvDeco));
            if (Interval.HasValue && Interval < minInterval)
                addError($"Interval must be greater than {minInterval} mins", nameof(Interval));

            IsModelValid = !errors.Any() && string.IsNullOrEmpty(LevelsValidationMessage) && string.IsNullOrEmpty(DecoLevelsValidationMessage);

            if (!IsModelValid)
                addError($"Please correct the errors above", string.Empty);

            return errors;
        }
    }
}
