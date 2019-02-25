using System;
using System.Collections.Generic;
using System.Linq;
using MDivePlanner.Domain.Interfaces;
using MDivePlanner.Domain.Entities;

namespace MDivePlanner.Domain.Logic
{
    public class DiveCalculator : IDiveCalculator
    {
        private readonly IEnumerable<DivePlanPointType> _planSequence;

        private DiveParameters _diveParameters;
        private IDecoAlgorythm _diveAlgorythm;

        public DiveCalculator(IDecoAlgorythm divingAlgo)
        {
            _planSequence = new DivePlanPointType[]
            {
                DivePlanPointType.StartDive,
                DivePlanPointType.Descent,
                DivePlanPointType.Bottom,
                DivePlanPointType.Ascent,
                DivePlanPointType.Deco,
                DivePlanPointType.EndDive
            };

            _diveAlgorythm = divingAlgo;
        }

        public CalculatedDivePlan Calculate(CalculatedDivePlan prevDive, DiveParameters diveParameters)
        {
            var points = new List<DivePlanPoint>(sizeof(double));
            var bottomLevels = new List<LevelInfo>(diveParameters.Levels.Count());
            double totalTime = 0;

            _diveParameters = diveParameters;

            var plan = new CalculatedDivePlan
            {
                MaxDepth = _diveParameters.Levels.Max(l => l.DepthFactor.Depth),
                BottomTime = _diveParameters.Levels.Sum(l => l.Time),
                PlanPoints = points,
                MaxPpO = _diveParameters.Levels.Max(l => BreathGas.GetGasPartialPreasureForDepth(l.DepthFactor, l.Gas.PpO)),
                MaxPpN = _diveParameters.Levels.Max(l => BreathGas.GetGasPartialPreasureForDepth(l.DepthFactor, l.Gas.PpN)),
                DiveParameters = _diveParameters
            };

            var diveResult = _diveAlgorythm.ProcessDive(_diveParameters, prevDive?.TissuesSaturationData);
            DivePlanPoint lastPoint = null;

            if (diveResult.Errors?.Any() == true)
                return new CalculatedDivePlan { Errors = diveResult.Errors };
           
            // dive start
            points.Add(new DivePlanPoint
            {
                Depth = 0,
                AbsoluteTime = 0,
                Duration = 0,
                Type = DivePlanPointType.StartDive,
                Gas = _diveParameters.Levels.First().Gas
            });

            lastPoint = points[0];

            foreach (var level in _diveParameters.Levels)
            {
                var depthDistance = Math.Abs(lastPoint.Depth - level.Depth);
                var descend = level.Depth > lastPoint.Depth;

                if (lastPoint.Type != DivePlanPointType.StartDive)
                    lastPoint.Type = DivePlanPointType.Bottom | (descend ? DivePlanPointType.Descent : DivePlanPointType.Ascent);

                var goToLevelTime = descend ? 
                    depthDistance / _diveParameters.DiveConfig.MaxDescentSpeed :
                    depthDistance / _diveParameters.DiveConfig.MaxAscentSpeed;

                totalTime += goToLevelTime;
                var reachedTime = totalTime;

                points.Add(new DivePlanPoint
                {
                    Depth = level.Depth,
                    AbsoluteTime = totalTime,
                    Duration = goToLevelTime,
                    Gas = level.Gas,
                    Type = (descend ? DivePlanPointType.Descent : DivePlanPointType.Ascent) | DivePlanPointType.Bottom
                });

                totalTime += level.Time;

                points.Add(new DivePlanPoint
                {
                    Depth = level.Depth,
                    AbsoluteTime = totalTime,
                    Duration = level.Time,
                    Gas = level.Gas,
                    Type = DivePlanPointType.Bottom
                });

                bottomLevels.Add(new LevelInfo
                {
                     Depth = level.Depth,
                     PpO = BreathGas.GetGasPartialPreasureForDepth(level.DepthFactor, level.Gas.PpO),
                     END = DivingMath.CalculateEND(level.DepthFactor, level.Gas),
                     Gas = level.Gas,
                     TimeReached = reachedTime
                });

                lastPoint = points.Last();
            }

            lastPoint.Type = DivePlanPointType.Ascent | DivePlanPointType.Bottom | DivePlanPointType.FinalAscent;

            // ascend & deco
            if (diveResult.DecoStops?.Any() == true)
            {
                double lastDepth = lastPoint.Depth;

                foreach (var deco in diveResult.DecoStops)
                {
                    var decoAscendTime = (lastDepth - deco.Depth) / _diveParameters.DiveConfig.MaxAscentSpeed;
                    totalTime += decoAscendTime;

                    points.Add(new DivePlanPoint
                    {
                        Depth = deco.Depth,
                        AbsoluteTime = totalTime,
                        Duration = deco.Time,
                        Gas = deco.Gas,
                        Type = DivePlanPointType.Deco
                    });

                    totalTime += deco.Time;

                    points.Add(new DivePlanPoint
                    {
                        Depth = deco.Depth,
                        AbsoluteTime = totalTime,
                        Duration = deco.Time,
                        Gas = deco.Gas,
                        Type = DivePlanPointType.Deco | DivePlanPointType.FinalAscent
                    });

                    lastDepth = deco.Depth;
                }

                var ascendTime = lastDepth / _diveParameters.DiveConfig.MaxAscentSpeed;
                totalTime += ascendTime;
            }
            else
            {
                // ascend
                var ascendTime = (lastPoint.Depth - _diveParameters.DiveConfig.SafeStopDepth) / _diveParameters.DiveConfig.MaxAscentSpeed;
                totalTime += ascendTime;

                var avrAscDepth = (lastPoint.Depth + _diveParameters.DiveConfig.SafeStopDepth) / 2;

                if (lastPoint.Depth <= _diveParameters.DiveConfig.SafeStopDepth)
                {
                    // no safety stop
                    totalTime += lastPoint.Depth / _diveParameters.DiveConfig.MaxAscentSpeed;
                }
                else
                {
                    // safety stop
                    points.Add(new DivePlanPoint
                    {
                        Depth = _diveParameters.DiveConfig.SafeStopDepth,
                        AbsoluteTime = totalTime,
                        Duration = _diveParameters.DiveConfig.SafeStopTime,
                        Gas = _diveParameters.Levels.First().Gas,
                        Type = DivePlanPointType.SafeStop
                    });

                    totalTime += _diveParameters.DiveConfig.SafeStopTime;

                    points.Add(new DivePlanPoint
                    {
                        Depth = _diveParameters.DiveConfig.SafeStopDepth,
                        AbsoluteTime = totalTime,
                        Duration = _diveParameters.DiveConfig.SafeStopTime,
                        Gas = _diveParameters.Levels.First().Gas,
                        Type = DivePlanPointType.SafeStop | DivePlanPointType.FinalAscent
                    });

                    ascendTime = _diveParameters.DiveConfig.SafeStopDepth / _diveParameters.DiveConfig.MaxAscentSpeed;
                    totalTime += ascendTime;
                }
            }

            // end dive
            points.Add(new DivePlanPoint { Depth = 0, AbsoluteTime = totalTime, Type = DivePlanPointType.EndDive, Gas = new BreathGas() });

            var gasSwitches = new List<GasSwitch>();

            plan.FullDesaturationTime = diveResult.FullDesaturationTime;
            plan.TissuesSaturationData = diveResult.TissuesSaturationData;
            plan.TotalTime = diveResult.DiveTotalTime;
            plan.MaxNoDecoDepthTime = diveResult.MaxNoDecoDepthTime;
            plan.DynamicNoDecoDepthTime = diveResult.DynamicNoDecoDepthTime;
            plan.IntervalTime = _diveParameters.IntervalTime;
            plan.OxygenCns = OxygenToxicityCalculator.CalulateOxygenCns(diveResult.DivePoints);
            plan.MaxEND = _diveParameters.Levels.Max(l => DivingMath.CalculateEND(l.DepthFactor, l.Gas));
            plan.GasSwitches = gasSwitches;
            plan.LevelsInfo = bottomLevels;

            foreach (var level in _diveParameters.Levels.Skip(1))
            {
                // use strict reference comparsion for presise gas check
                var gasSwitchPoint = diveResult.DivePoints.FirstOrDefault(d => d.CurrentGas == level.Gas);
                if (!gasSwitchPoint.IsEmpty())
                {
                    gasSwitches.Add(new GasSwitch
                    {
                        Depth = gasSwitchPoint.DepthFactor.Depth,
                        AbsoluteTime = gasSwitchPoint.CurrentDiveTime,
                        Gas = gasSwitchPoint.CurrentGas,
                        IsDeco = false,
                        PpO = BreathGas.GetGasPartialPreasureForDepth(gasSwitchPoint.DepthFactor, gasSwitchPoint.CurrentGas.PpO)
                    });
                }
            }

            foreach (var decoLevel in _diveParameters.DecoLevels ?? new List<DiveLevel>())
            {
                // use strict reference comparsion for presise gas check
                var decoGasSwitchPoint = diveResult.DivePoints.FirstOrDefault(d => d.CurrentGas == decoLevel.Gas);
                if (!decoGasSwitchPoint.IsEmpty())
                {
                    gasSwitches.Add(new GasSwitch
                    {
                        Depth = decoGasSwitchPoint.DepthFactor.Depth,
                        AbsoluteTime = decoGasSwitchPoint.CurrentDiveTime,
                        Gas = decoGasSwitchPoint.CurrentGas,
                        IsDeco = true,
                        PpO = BreathGas.GetGasPartialPreasureForDepth(decoGasSwitchPoint.DepthFactor, decoGasSwitchPoint.CurrentGas.PpO)
                    });
                }
            }

            // celing points reduce logic
            if (diveResult.CeilingDepthPoints?.Any() == true)
            {
                const int maxPoints = 100;
                var skip = 2 * diveResult.CeilingDepthPoints.Count() / 2 / maxPoints;
                if (skip == 0)
                    skip = 1;

                var ceilingDepthPoints = new List<DepthTime>(diveResult.CeilingDepthPoints.Count() / skip);

                var count = diveResult.CeilingDepthPoints.Count();
                for (int i = 0; i < count; i++)
                {
                    if (i % skip == 0)
                    {
                        var elem = diveResult.CeilingDepthPoints.ElementAt(i);
                        ceilingDepthPoints.Add(new DepthTime(elem.Depth, elem.Time));
                    }
                }

                ceilingDepthPoints.Add(new DepthTime(0, diveResult.CeilingDepthPoints.Last().Time));
                plan.CeilingDepthPoints = ceilingDepthPoints;
            }

            var consumedGas = CalculateConsumedGas(diveResult, gasSwitches);
            plan.ConsumedBottomGases = consumedGas.Key;
            plan.ConsumedDecoGases = consumedGas.Value;
            plan.DiveResultBlocks = plan.GetDiveInfo();

            return plan;
        }

        private KeyValuePair<IEnumerable<ConsumedGas>, IEnumerable<ConsumedGas>> CalculateConsumedGas(
            CalculatedDiveResult diveResult, IEnumerable<GasSwitch> gasSwitches)
        {
            DivePoint? prevPoint = null;
            var bottomGases = new Dictionary<BreathGas, ConsumedGas>(sizeof(double));
            var decoGases = new Dictionary<BreathGas, ConsumedGas>(sizeof(double));

            var firstStopDepth = diveResult.DecoStops?.Any() == true ? diveResult.DecoStops.Max(s => s.Depth) : 0;
            if (firstStopDepth < double.Epsilon)
                firstStopDepth = _diveParameters.DiveConfig.SafeStopDepth;

            foreach (var point in diveResult.DivePoints)
            {
                if (prevPoint != null)
                {
                    var avgDepth = (prevPoint.Value.DepthFactor.Depth + point.DepthFactor.Depth) * 0.5;
                    if (avgDepth <= double.Epsilon)
                        break;

                    var timeSpan = point.CurrentDiveTime - prevPoint.Value.CurrentDiveTime;
                    if (timeSpan > 0)
                    {
                        var preasure = DivingMath.DepthToPreasureBars(new DepthFactor(avgDepth, point.DepthFactor.WaterDensity));
                        var rmv = (avgDepth <= firstStopDepth) ? _diveParameters.DiveConfig.DecoRmv : _diveParameters.DiveConfig.BottomRmv;
                        
                        var decoSwitchTime = gasSwitches.FirstOrDefault(gs => gs.IsDeco)?.AbsoluteTime ?? 0.0;
                        var gases = (point.CurrentDiveTime >= decoSwitchTime && decoSwitchTime > double.Epsilon) ? decoGases : bottomGases;

                        if (!gases.ContainsKey(point.CurrentGas))
                            gases[point.CurrentGas] = new ConsumedGas() { Gas = point.CurrentGas };

                        gases[point.CurrentGas].Amount += rmv * preasure / DivingMath.SeaLevelPreasureBars * timeSpan;
                    }
                }

                prevPoint = point;
            }

            return new KeyValuePair<IEnumerable<ConsumedGas>, IEnumerable<ConsumedGas>>(bottomGases.Values, decoGases.Values);
        }
    }
}
