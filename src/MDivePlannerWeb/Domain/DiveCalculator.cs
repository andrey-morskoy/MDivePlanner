using System;
using System.Collections.Generic;
using System.Linq;

namespace MDivePlannerWeb.Domain
{
    public class DiveCalculator
    {
        private const double MinDepth = 1;
        private const double MinTime = 2;
    
        private readonly IEnumerable<DivePlanPointType> _planSequence;

        private DiveParameters _diveParameters;
        private IDecoAlgorythm _diveAlgorythm;

        public DiveCalculator(DiveParameters diveParameters, IDecoAlgorythm divingAlgo)
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
            _diveParameters = diveParameters;
        }

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (_diveParameters.DepthFactor.Depth < MinDepth)
                errors.Add($"operting depth is too shallow (min {MinDepth} metter)");

            if (_diveParameters.Time < MinTime)
                errors.Add($"operting time is too small (min {MinTime} mins)");

            return errors.Count == 0;
        }

        public CalculatedDivePlan Calculate(CalculatedDivePlan prevDive)
        {
            var points = new List<DivePlanPoint>();
            double totalTime = 0;

            var plan = new CalculatedDivePlan
            {
                Depth = _diveParameters.DepthFactor.Depth,
                BottomTime = _diveParameters.Time,
                PlanPoints = points,
                MaxPpO = BreathGas.GetGasPartialPreasureForDepth(_diveParameters.DepthFactor, _diveParameters.Gas.PpO),
                MaxPpN = BreathGas.GetGasPartialPreasureForDepth(_diveParameters.DepthFactor, _diveParameters.Gas.PpN),
                DiveParameters = _diveParameters
            };

            var diveResult = _diveAlgorythm.ProcessDive(_diveParameters, prevDive.TissuesSaturationData);

            if (diveResult.Errors?.Any() == true)
                return new CalculatedDivePlan { Errors = diveResult.Errors };
           
            // dive start
            points.Add(new DivePlanPoint { Depth = 0, AbsoluteTime = 0, Duration = 0, Type = DivePlanPointType.StartDive, Gas = _diveParameters.Gas });

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

            // ascend & deco
            if (diveResult.DecoStops?.Any() == true)
            {
                double lastDepth = _diveParameters.DepthFactor.Depth;

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
                        Type = DivePlanPointType.Deco | DivePlanPointType.Ascent
                    });

                    lastDepth = deco.Depth;
                }

                var ascendTime = lastDepth / _diveParameters.DiveConfig.MaxAscentSpeed;
                totalTime += ascendTime;
            }
            else
            {
                // ascend
                var ascendTime = (_diveParameters.DepthFactor.Depth - _diveParameters.DiveConfig.SafeStopDepth) / _diveParameters.DiveConfig.MaxAscentSpeed;
                totalTime += ascendTime;

                var avrAscDepth = (_diveParameters.DepthFactor.Depth + _diveParameters.DiveConfig.SafeStopDepth) / 2;

                if (_diveParameters.DepthFactor.Depth <= _diveParameters.DiveConfig.SafeStopDepth)
                {
                    // no safety stop
                    totalTime += _diveParameters.DepthFactor.Depth / _diveParameters.DiveConfig.MaxAscentSpeed;
                }
                else
                {
                    // safety stop
                    points.Add(new DivePlanPoint
                    {
                        Depth = _diveParameters.DiveConfig.SafeStopDepth,
                        AbsoluteTime = totalTime,
                        Duration = _diveParameters.DiveConfig.SafeStopTime,
                        Gas = _diveParameters.Gas,
                        Type = DivePlanPointType.SafeStop
                    });

                    totalTime += _diveParameters.DiveConfig.SafeStopTime;

                    points.Add(new DivePlanPoint
                    {
                        Depth = _diveParameters.DiveConfig.SafeStopDepth,
                        AbsoluteTime = totalTime,
                        Duration = _diveParameters.DiveConfig.SafeStopTime,
                        Gas = _diveParameters.Gas,
                        Type = DivePlanPointType.SafeStop | DivePlanPointType.Ascent
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
            plan.NoDecoTime = diveResult.NoDecoDepthTime.Value;
            plan.IntervalTime = _diveParameters.IntervalTime;
            plan.ConsumedGas = consumedGas.Key;
            plan.ConsumedDecoGases = consumedGas.Value;
            plan.MValues = diveResult.MValues;
            plan.OxygenCns = OxygenToxicityCalculator.CalulateOxygenCns(diveResult.DivePoints);
            plan.END = DivingMath.CalculateEND(_diveParameters.DepthFactor, _diveParameters.Gas);
            plan.DecoGasSwitches = gasSwitches;

            foreach (var gas in _diveParameters.DecoGases)
            {
                var decoGasSwitchPoint = diveResult.DivePoints.FirstOrDefault(d => d.CurrentGas.CompareTo(gas));
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

            return plan;
        }

        private KeyValuePair<double, IEnumerable<double>> CalculateConsumedGas(CalculatedDiveResult diveResult)
        {
            double consumedGas = 0;
            var consumedDecoGases = new double[_diveParameters.DecoGases.Count()];
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

                        if (_diveParameters.DecoGases.Count() > 0)
                        {
                            int ind = 0;
                            foreach (var gas in _diveParameters.DecoGases)
                            {
                                if (point.CurrentGas.CompareTo(gas))
                                    consumedDecoGases[ind] += rmv * preasure / DivingMath.SeaLevelPreasureBars * timeSpan;
                                ++ind;
                            }
                        }
                    }
                }

                prevPoint = point;
            }

            return new KeyValuePair<double, IEnumerable<double>>(consumedGas, consumedDecoGases);
        }
    }
}
