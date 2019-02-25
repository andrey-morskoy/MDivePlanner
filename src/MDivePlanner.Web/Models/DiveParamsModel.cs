using System;
using System.Collections.Generic;
using System.Linq;
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

        public double WaterDensity { get; set; }

        public string Algorythm { get; set; }

        public string MinDecoStopTime { get; set; }

        public double AscentSpeed { get; set; }

        public double DescentSpeed { get; set; }

        public double GradFactorLow { get; set; }

        public double GradFactorHigh { get; set; }

        public double SafetyStopDepth { get; set; }

        public double SafetyStopTime { get; set; }

        public double RmvBottom { get; set; }

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

        public DiveParamsModel(DiveParameters diveParams) : this()
        {
            MinDecoStopTime = diveParams.DiveConfig.MinDecoTime.ToString();
            WaterDensity = diveParams.Levels.First().DepthFactor.WaterDensity;
            GradFactorLow = Math.Round(diveParams.DiveConfig.GradFactorLow * 100.0);
            GradFactorHigh = Math.Round(diveParams.DiveConfig.GradFactorHigh * 100.0);

            AscentSpeed = diveParams.DiveConfig.MaxAscentSpeed;
            DescentSpeed = diveParams.DiveConfig.MaxDescentSpeed;
            SafetyStopDepth = diveParams.DiveConfig.SafeStopDepth;
            SafetyStopTime = diveParams.DiveConfig.SafeStopTime;
            RmvBottom = diveParams.DiveConfig.BottomRmv;
            RmvDeco = diveParams.DiveConfig.DecoRmv;
            Algorythm = diveParams.DiveConfig.AlgoSubType.ToString();
            Interval = diveParams.IntervalTime > double.Epsilon ? diveParams.IntervalTime : (double?)null;

            int levelInd = 0;

            foreach (var level in diveParams.Levels)
            {
                DiveLevels[levelInd].Depth = level.Depth;
                DiveLevels[levelInd].Time = level.Time;
                DiveLevels[levelInd].UseLevel = true;
                DiveLevels[levelInd].Gas = new GasModel(level.Gas);
                ++levelInd;
            }

            levelInd = 0;
            foreach (var level in diveParams.DecoLevels)
            {
                DecoLevels[levelInd].Depth = level.Depth;
                DecoLevels[levelInd].UseLevel = true;
                DecoLevels[levelInd].Gas = new GasModel(level.Gas);
                ++levelInd;
            }
        }

        public DiveParamsModel FillDefault()
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

            return this;
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
            return DiveParamsBasicValidator.ValidateModel(this);
        }
    }
}
