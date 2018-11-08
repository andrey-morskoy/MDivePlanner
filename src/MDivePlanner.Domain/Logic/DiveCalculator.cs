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
            var points = new List<DivePlanPoint>();
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

                lastPoint = points.Last();
            }

            lastPoint.Type = DivePlanPointType.Ascent | DivePlanPointType.Bottom | DivePlanPointType.FinalAscent;

            /*
            var decsentTime = _diveParameters.DepthFactor.Depth / _diveParameters.DiveConfig.MaxDescentSpeed;
            totalTime += decsentTime;

            // descent
            points.Add(new DivePlanPoint
            {
                Depth = _diveParameters.DepthFactor.Depth,
                AbsoluteTime = totalTime,
                Duration = decsentTime,
                Gas = _diveParameters.Gas,
                Type = DivePlanPointType.Descent | DivePlanPointType.Bottom
            });

            // bottom
            points.Add(new DivePlanPoint
            {
                Depth = _diveParameters.DepthFactor.Depth,
                AbsoluteTime = _diveParameters.Time,
                Duration = _diveParameters.Time - totalTime,
                Gas = _diveParameters.Gas,
                Type = DivePlanPointType.Ascent | DivePlanPointType.Bottom
            });

            totalTime = _diveParameters.Time;
            */

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

            var consumedGas = CalculateConsumedGas(diveResult);
            var gasSwitches = new List<GasSwitch>();

            plan.FullDesaturationTime = diveResult.FullDesaturationTime;
            plan.TissuesSaturationData = diveResult.TissuesSaturationData;
            plan.TotalTime = diveResult.DiveTotalTime;
            plan.MaxNoDecoDepthTime = diveResult.MaxNoDecoDepthTime;
            plan.DynamicNoDecoDepthTime = diveResult.DynamicNoDecoDepthTime;
            plan.IntervalTime = _diveParameters.IntervalTime;
            //plan.ConsumedBottomGases = consumedGas.Key;
            plan.ConsumedDecoGases = consumedGas.Value;
            plan.OxygenCns = OxygenToxicityCalculator.CalulateOxygenCns(diveResult.DivePoints);
            plan.MaxEND = _diveParameters.Levels.Max(l => DivingMath.CalculateEND(l.DepthFactor, l.Gas));
            plan.DecoGasSwitches = gasSwitches;

            foreach (var decoLevel in _diveParameters.DecoLevels ?? new List<DiveLevel>())
            {
                var decoGasSwitchPoint = diveResult.DivePoints.FirstOrDefault(d => d.CurrentGas.IsEqual(decoLevel.Gas));
                if (!decoGasSwitchPoint.IsEmpty())
                {
                    gasSwitches.Add(new GasSwitch
                    {
                        Depth = decoGasSwitchPoint.DepthFactor.Depth,
                        AbsoluteTime = decoGasSwitchPoint.CurrentDiveTime,
                        Gas = decoGasSwitchPoint.CurrentGas
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

            return plan;
        }

        private KeyValuePair<double, IEnumerable<double>> CalculateConsumedGas(CalculatedDiveResult diveResult)
        {
            /*
            double consumedGas = 0;
            var consumedDecoGases = new double[_diveParameters.DecoLevels.Count()];
            DivePoint? prevPoint = null;
            var bottomReached = false;

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

                    if (DivingMath.CompareDouble(point.DepthFactor.Depth, _diveParameters.DepthFactor.Depth))
                        bottomReached = true;

                    var timeSpan = point.CurrentDiveTime - prevPoint.Value.CurrentDiveTime;
                    if (timeSpan > 0)
                    {
                        var preasure = DivingMath.DepthToPreasureBars(new DepthFactor(avgDepth, point.DepthFactor.WaterDensity));
                        var rmv = (avgDepth <= firstStopDepth && bottomReached) ? _diveParameters.DiveConfig.DecoRmv : _diveParameters.DiveConfig.BottomRmv;

                        if (point.CurrentGas.CompareTo(_diveParameters.Gas))
                            consumedGas += rmv * preasure / DivingMath.SeaLevelPreasureBars * timeSpan;

                        if (_diveParameters.DecoLevels.Count() > 0)
                        {
                            int ind = 0;
                            foreach (var decoLevel in _diveParameters.DecoLevels)
                            {
                                if (point.CurrentGas.CompareTo(decoLevel.Gas))
                                    consumedDecoGases[ind] += rmv * preasure / DivingMath.SeaLevelPreasureBars * timeSpan;
                                ++ind;
                            }
                        }
                    }
                }

                prevPoint = point;
            }

            return new KeyValuePair<double, IEnumerable<double>>(consumedGas, consumedDecoGases);
            */

            return new KeyValuePair<double, IEnumerable<double>>(0, new List<double>());
        }
    }
}
