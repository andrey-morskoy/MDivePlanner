﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace MDivePlannerWeb.Domain
{
    public class Zhl16Algorithm : IDecoAlgorythm
    {
        private const double TimeStep = 0.016666667; // 1 sec
        private const double DecoStopsStep = 3.0;

        private struct Compartment
        {
            public int Number { get; set; }
            public double N2HalfTime { get; set; }
            public double N2ValueA { get; set; }
            public double N2ValueB { get; set; }
            public double HeHalfTime { get; set; }
            public double HeValueA { get; set; }
            public double HeValueB { get; set; }
        }

        public class CompartmentCalculations
        {
            public double N2Presure { get; set; }
            public double HePresure { get; set; }

            public void Reset(double startN2Presure, double startHePresure)
            {
                N2Presure = startN2Presure;
                HePresure = startHePresure;
            }
        }

        public enum SubType
        {
            A,
            B,
            C
        }

        private readonly List<Compartment> _compartments;
        private readonly CompartmentCalculations[] _compartmentData;
        private readonly CompartmentCalculations[] _compartmentAltData;

        private DiveParameters _diveParameters;
        private double _firstDecoStopDepth;
        private List<KeyValuePair<double, double>> _mValues;
        private List<DivePoint> _divePoints;
        private BreathGas _airGas;
        private double _trackedDivePointTime;

        public Zhl16Algorithm(SubType subType)
        {
            switch (subType)
            {
                case SubType.A:
                    _compartments = new List<Compartment>
                    {
                        new Compartment { Number = 1, N2HalfTime = 4, N2ValueA = 1.2599, N2ValueB = 0.5050 },
                        new Compartment { Number = 2, N2HalfTime = 8, N2ValueA = 1.0000, N2ValueB = 0.6514 },
                        new Compartment { Number = 3, N2HalfTime = 12.5, N2ValueA = 0.8618, N2ValueB = 0.7222 },
                        new Compartment { Number = 4, N2HalfTime = 18.5, N2ValueA = 0.7562, N2ValueB = 0.7725 },
                        new Compartment { Number = 5, N2HalfTime = 27,   N2ValueA = 0.6667, N2ValueB = 0.8125 },
                        new Compartment { Number = 6, N2HalfTime = 38.3, N2ValueA = 0.5933, N2ValueB = 0.8434 },
                        new Compartment { Number = 7, N2HalfTime = 54.3, N2ValueA = 0.5282, N2ValueB = 0.8693 },
                        new Compartment { Number = 8, N2HalfTime = 77,   N2ValueA = 0.4701, N2ValueB = 0.8910 },
                        new Compartment { Number = 9, N2HalfTime = 109,  N2ValueA = 0.4187, N2ValueB = 0.9092 },
                        new Compartment { Number = 10, N2HalfTime = 146, N2ValueA = 0.3798, N2ValueB = 0.9222 },
                        new Compartment { Number = 11, N2HalfTime = 187, N2ValueA = 0.3497, N2ValueB = 0.9319 },
                        new Compartment { Number = 12, N2HalfTime = 239, N2ValueA = 0.3223, N2ValueB = 0.9403 },
                        new Compartment { Number = 13, N2HalfTime = 305, N2ValueA = 0.2971, N2ValueB = 0.9477 },
                        new Compartment { Number = 14, N2HalfTime = 390, N2ValueA = 0.2737, N2ValueB = 0.9544 },
                        new Compartment { Number = 15, N2HalfTime = 498, N2ValueA = 0.2523, N2ValueB = 0.9602 },
                        new Compartment { Number = 16, N2HalfTime = 635, N2ValueA = 0.2327, N2ValueB = 0.9653 }
                    };
                    break;
                case SubType.B:
                    _compartments = new List<Compartment>
                    {
                        new Compartment { Number = 1, N2HalfTime = 4, N2ValueA = 1.2599, N2ValueB = 0.5050 },
                        //new Compartment { Number = 1, N2HalfTime = 5, N2ValueA = 1.1696, N2ValueB = 0.5578 },
                        new Compartment { Number = 2, N2HalfTime = 8, N2ValueA = 1.0000, N2ValueB = 0.6514 },
                        new Compartment { Number = 3, N2HalfTime = 12.5, N2ValueA = 0.8618, N2ValueB = 0.7222 },
                        new Compartment { Number = 4, N2HalfTime = 18.5, N2ValueA = 0.7562, N2ValueB = 0.7825 },
                        new Compartment { Number = 5, N2HalfTime = 27,   N2ValueA = 0.6667, N2ValueB = 0.8126 },
                        new Compartment { Number = 6, N2HalfTime = 38.3, N2ValueA = 0.5600, N2ValueB = 0.8434 },
                        new Compartment { Number = 7, N2HalfTime = 54.3, N2ValueA = 0.4947, N2ValueB = 0.8693 },
                        new Compartment { Number = 8, N2HalfTime = 77,   N2ValueA = 0.4500, N2ValueB = 0.8910 },
                        new Compartment { Number = 9, N2HalfTime = 109,  N2ValueA = 0.4187, N2ValueB = 0.9092 },
                        new Compartment { Number = 10, N2HalfTime = 146, N2ValueA = 0.3798, N2ValueB = 0.9222 },
                        new Compartment { Number = 11, N2HalfTime = 187, N2ValueA = 0.3497, N2ValueB = 0.9319 },
                        new Compartment { Number = 12, N2HalfTime = 239, N2ValueA = 0.3223, N2ValueB = 0.9403 },
                        new Compartment { Number = 13, N2HalfTime = 305, N2ValueA = 0.2850, N2ValueB = 0.9477 },
                        new Compartment { Number = 14, N2HalfTime = 390, N2ValueA = 0.2737, N2ValueB = 0.9544 },
                        new Compartment { Number = 15, N2HalfTime = 498, N2ValueA = 0.2523, N2ValueB = 0.9602 },
                        new Compartment { Number = 16, N2HalfTime = 635, N2ValueA = 0.2327, N2ValueB = 0.9653 }
                    };
                    break;
                case SubType.C:
                    _compartments = new List<Compartment>
                    {
                        new Compartment { Number = 1, N2HalfTime = 4, N2ValueA = 1.2599, N2ValueB = 0.5050, HeHalfTime = 1.51, HeValueA = 1.7424, HeValueB = 0.4245 },
                        new Compartment { Number = 2, N2HalfTime = 8, N2ValueA = 1.0000, N2ValueB = 0.6514, HeHalfTime = 3.02, HeValueA = 1.3830, HeValueB = 0.5747 },
                        new Compartment { Number = 3, N2HalfTime = 12.5, N2ValueA = 0.8618, N2ValueB = 0.7222, HeHalfTime = 4.72, HeValueA = 1.1919, HeValueB = 0.6527 },
                        new Compartment { Number = 4, N2HalfTime = 18.5, N2ValueA = 0.7562, N2ValueB = 0.7725, HeHalfTime = 6.99, HeValueA = 1.0458, HeValueB = 0.7223 },
                        new Compartment { Number = 5, N2HalfTime = 27,   N2ValueA = 0.6200, N2ValueB = 0.8125, HeHalfTime = 10.21, HeValueA = 0.9220, HeValueB = 0.7582 },
                        new Compartment { Number = 6, N2HalfTime = 38.3, N2ValueA = 0.5043, N2ValueB = 0.8434, HeHalfTime = 14.48, HeValueA = 0.8205, HeValueB = 0.7957 },
                        new Compartment { Number = 7, N2HalfTime = 54.3, N2ValueA = 0.4410, N2ValueB = 0.8693, HeHalfTime = 20.53, HeValueA = 0.7305, HeValueB = 0.8279 },
                        new Compartment { Number = 8, N2HalfTime = 77,   N2ValueA = 0.4000, N2ValueB = 0.8910, HeHalfTime = 29.11, HeValueA = 0.6502, HeValueB = 0.8553 },
                        new Compartment { Number = 9, N2HalfTime = 109,  N2ValueA = 0.3750, N2ValueB = 0.9092, HeHalfTime = 41.20, HeValueA = 0.5950, HeValueB = 0.8757 },
                        new Compartment { Number = 10, N2HalfTime = 146, N2ValueA = 0.3500, N2ValueB = 0.9222, HeHalfTime = 55.19, HeValueA = 0.5545, HeValueB = 0.8903 },
                        new Compartment { Number = 11, N2HalfTime = 187, N2ValueA = 0.3295, N2ValueB = 0.9319, HeHalfTime = 70.69, HeValueA = 0.5333, HeValueB = 0.8997 },
                        new Compartment { Number = 12, N2HalfTime = 239, N2ValueA = 0.3065, N2ValueB = 0.9403, HeHalfTime = 90.34, HeValueA = 0.5189, HeValueB = 0.9073 },
                        new Compartment { Number = 13, N2HalfTime = 305, N2ValueA = 0.2835, N2ValueB = 0.9477, HeHalfTime = 115.29, HeValueA = 0.5181, HeValueB = 0.9122 },
                        new Compartment { Number = 14, N2HalfTime = 390, N2ValueA = 0.2610, N2ValueB = 0.9544, HeHalfTime = 147.42, HeValueA = 0.5176, HeValueB = 0.9171 },
                        new Compartment { Number = 15, N2HalfTime = 498, N2ValueA = 0.2480, N2ValueB = 0.9602, HeHalfTime = 188.24, HeValueA = 0.5172, HeValueB = 0.9217 },
                        new Compartment { Number = 16, N2HalfTime = 635, N2ValueA = 0.2327, N2ValueB = 0.9653, HeHalfTime = 240.03, HeValueA = 0.5119, HeValueB = 0.9267 }
                    };
                    break;
            }

            _airGas = new BreathGas();
            _compartmentData = new CompartmentCalculations[_compartments.Count];
            _compartmentAltData = new CompartmentCalculations[_compartments.Count];

            for (int i = 0; i < _compartmentData.Length; i++)
            {
                _compartmentAltData[i] = new CompartmentCalculations();
                _compartmentData[i] = new CompartmentCalculations();
            }
        }

        public CalculatedDiveResult ProcessDive(DiveParameters diveParameters, object tissuePreasures = null)
        {
            Reset(diveParameters, tissuePreasures);

            double totalTime = 0;
            double currentDepth = 0;
            double maxDepth = diveParameters.DepthFactor.Depth;

            AddDivePointInfo(totalTime, 0, 0, _diveParameters.Gas, true);

            // dive
            while (true)
            {
                currentDepth += TimeStep * diveParameters.DiveConfig.MaxDescentSpeed;
                if (currentDepth > maxDepth)
                    currentDepth = maxDepth;

                CalculateCompartmentPresures(TimeStep, currentDepth, diveParameters.Gas);
                var ceilingDepth = GetCurrentCeilingDepth(currentDepth);

                totalTime += TimeStep;
                AddDivePointInfo(totalTime, ceilingDepth, currentDepth, _diveParameters.Gas);

                if (totalTime >= diveParameters.Time)
                    break;
            }

            var maxNoDecoTime = CalculateMaxNoDecoTime(maxDepth);
            IEnumerable<DecoStopResult> decoStops = null;

            // ascent
            if (currentDepth <= diveParameters.DiveConfig.SafeStopDepth)
            {
                //no need even safety stop
                while (true)
                {
                    currentDepth -= TimeStep * _diveParameters.DiveConfig.MaxAscentSpeed;
                    if (currentDepth < double.Epsilon)
                        break;

                    CalculateCompartmentPresures(TimeStep, currentDepth, diveParameters.Gas);
                    totalTime += TimeStep;
                }

                AddDivePointInfo(totalTime, 0, 0, _diveParameters.Gas, true);
            }
            else
            {
                var decoStopDepth = FindNearestDecoDepth(GetCurrentCeilingDepth(currentDepth));
                if (decoStopDepth > 0 && GetCurrentCeilingDepth(currentDepth, _diveParameters.DiveConfig.GradFactorHigh) > 0)
                {
                    var ascentResult = AscentWithDeco(currentDepth, totalTime);
                    totalTime = ascentResult.Key;
                    decoStops = ascentResult.Value;
                }
                else
                {
                    totalTime = AscentWithoutDeco(currentDepth, totalTime);
                }
            }

            var tissueData = _compartmentData.Select(c => new CompartmentCalculations { N2Presure = c.N2Presure, HePresure = c.HePresure }).ToArray();

            var result = new CalculatedDiveResult
            {
                MaxAscendSpeed = diveParameters.DiveConfig.MaxAscentSpeed,
                NoDecoDepthTime = new KeyValuePair<double, double>(maxDepth, maxNoDecoTime),
                SaturationIndex = 0,
                DecoStops = decoStops,
                MValues = _mValues,
                FullDesaturationTime = CalculateFullDesaturationTime(),
                DivePoints = _divePoints,
                DiveTotalTime = totalTime,
                TissuesSaturationData = tissueData
            };

            return result;
        }

        private void Reset(DiveParameters diveParams, object tissuePreasures)
        {
            _diveParameters = diveParams;
            _firstDecoStopDepth = 0;
            _trackedDivePointTime = 0;

            if (tissuePreasures != null && tissuePreasures is CompartmentCalculations[])
            {
                var tissuePreasureValues = (CompartmentCalculations[])tissuePreasures;
                if (tissuePreasureValues.Length != _compartmentData.Length)
                    throw new Exception("Dives may use different deco algo.");
                if (diveParams.IntervalTime < double.Epsilon)
                    throw new Exception("Interval time cannot be 0 for the next dive.");

                for (int i = 0; i < tissuePreasureValues.Length; i++)
                {
                    var currTissueValues = tissuePreasureValues[i];
                    _compartmentData[i].Reset(currTissueValues.N2Presure, currTissueValues.HePresure);
                    _compartmentAltData[i].Reset(currTissueValues.N2Presure, currTissueValues.HePresure);
                }

                const double intervalTimeStep = 1.0;
                double time = 0;

                while(true)
                {
                    CalculateCompartmentPresures(intervalTimeStep, 0, _airGas);
                    CalculateCompartmentPresures(intervalTimeStep, 0, _airGas, _compartmentAltData);

                    time += intervalTimeStep;
                    if (time >= diveParams.IntervalTime)
                        break;
                }
            }
            else
            {
                var startAmbientPreasure = _airGas.PpN * (DivingMath.SeaLevelPreasureBars - DivingMath.WaterVapourPreasureBars);

                Array.ForEach(_compartmentData, i => i.Reset(startAmbientPreasure, 0));
                Array.ForEach(_compartmentAltData, i => i.Reset(startAmbientPreasure, 0));
            }

            _mValues = new List<KeyValuePair<double, double>>((int)diveParams.Time * 2);
            _divePoints = new List<DivePoint>((int)diveParams.Time * 2);
        }

        private double CalculateMaxNoDecoTime(double depth)
        {
            double maxNoDecoTime = 0;
            double currentNoDecoDepth = 0;
            double noDecoTimeStep = 0.5; 

            while (true)
            {
                currentNoDecoDepth += noDecoTimeStep * _diveParameters.DiveConfig.MaxDescentSpeed;
                if (currentNoDecoDepth > depth)
                    currentNoDecoDepth = depth;

                CalculateCompartmentPresures(noDecoTimeStep, currentNoDecoDepth, _diveParameters.Gas, _compartmentAltData);
                var ceilingNoDecoDepth = GetCurrentCeilingDepth(currentNoDecoDepth, _diveParameters.DiveConfig.GradFactorHigh, _compartmentAltData);

                maxNoDecoTime += noDecoTimeStep;

                if (maxNoDecoTime > 300)
                    noDecoTimeStep = 1;

                if (ceilingNoDecoDepth > double.Epsilon || maxNoDecoTime > 999.0)
                    break;
            }

            return Math.Floor(maxNoDecoTime);
        }

        private KeyValuePair<double, IEnumerable<DecoStopResult>> AscentWithDeco(double currentDepth, double totalTime)
        {
            var minDecoStopTime = _diveParameters.DiveConfig.MinDecoTime == MinDecoTimeStep.HalfMin ? 0.5 : 1.0;
            var isFirstStop = true;
            var isSafetyStop = false;
            var decoStopDepth = FindNearestDecoDepth(GetCurrentCeilingDepth(currentDepth));
            var decoStops = new List<DecoStopResult>();
            var currGas = _diveParameters.Gas;

            while (true)
            {
                currentDepth -= TimeStep * _diveParameters.DiveConfig.MaxAscentSpeed;
                if (currentDepth < 0)
                    currentDepth = 0;

                currGas = SelectGas(new DepthFactor(currentDepth, _diveParameters.DepthFactor.WaterDensity), decoStopDepth); 

                if (currentDepth <= decoStopDepth)
                {
                    currentDepth = decoStopDepth;
                    if (currentDepth < double.Epsilon)
                        break;

                    if (isFirstStop)
                        _firstDecoStopDepth = decoStopDepth;

                    // deco stop
                    double decoStopTime = 0;
                    while (true)
                    {
                        CalculateCompartmentPresures(TimeStep, currentDepth, currGas);
                        var ceilingDecoDepth = GetCurrentCeilingDepth(currentDepth);
                        var nextDecoStopDepth = FindNearestDecoDepth(ceilingDecoDepth);

                        var perTime = decoStopTime;

                        totalTime += TimeStep;
                        decoStopTime += TimeStep;
                        AddDivePointInfo(totalTime, ceilingDecoDepth, currentDepth, currGas);

                        if (isSafetyStop)
                        {
                            if (decoStopTime >= _diveParameters.DiveConfig.SafeStopTime)
                            {
                                decoStopDepth = 0;
                                break;
                            }
                        }
                        else
                        {
                            if (nextDecoStopDepth < decoStopDepth &&
                                decoStopTime >= (isFirstStop ? 0.1 : GetNearestTime(minDecoStopTime)) && decoStopTime >= GetNearestTime(perTime))
                            {
                                decoStops.Add(new DecoStopResult { Depth = decoStopDepth, Time = decoStopTime, Gas = currGas });
                                decoStopDepth = (decoStopDepth - nextDecoStopDepth) > DecoStopsStep ? decoStopDepth - DecoStopsStep : nextDecoStopDepth;
                                break;
                            }
                        }
                    }

                    isFirstStop = false;
                    continue;
                }

                CalculateCompartmentPresures(TimeStep, currentDepth, currGas);
                var ceilingDepth = GetCurrentCeilingDepth(currentDepth);
                decoStopDepth = FindNearestDecoDepth(ceilingDepth);

                // It may happen that during ascent tissues have desaturated and we do not require any deco stops.
                // In this case we need to make just a safety stop.
                if (decoStopDepth == 0 && decoStops.Count == 0 && currentDepth > (_diveParameters.DiveConfig.SafeStopDepth + double.Epsilon))
                {
                    isSafetyStop = true;
                    decoStopDepth = _diveParameters.DiveConfig.SafeStopDepth;
                }

                if (decoStops.Count > 0)
                {
                    var nextStop = decoStops[0].Depth - decoStops.Count * DecoStopsStep;
                    if (decoStopDepth < nextStop)
                        decoStopDepth = nextStop;
                }

                totalTime += TimeStep;
                AddDivePointInfo(totalTime, ceilingDepth, currentDepth, currGas);
            }

            totalTime += TimeStep;
            AddDivePointInfo(totalTime, 0, 0, currGas, true);

            return new KeyValuePair<double, IEnumerable<DecoStopResult>>(totalTime, decoStops);
        }

        private double AscentWithoutDeco(double currentDepth, double totalTime)
        {
            double stopDepth = _diveParameters.DiveConfig.SafeStopDepth;

            //ascent without deco
            while (true)
            {
                currentDepth -= TimeStep * _diveParameters.DiveConfig.MaxAscentSpeed;
                if (currentDepth < double.Epsilon)
                    currentDepth = 0;

                if (currentDepth <= stopDepth && stopDepth > double.Epsilon)
                {
                    currentDepth = stopDepth;
                    double stopTime = 0;

                    while (true)
                    {
                        CalculateCompartmentPresures(TimeStep, currentDepth, _diveParameters.Gas);

                        totalTime += TimeStep;
                        stopTime += TimeStep;
                        AddDivePointInfo(totalTime, GetCurrentCeilingDepth(currentDepth, _diveParameters.DiveConfig.GradFactorHigh), currentDepth, _diveParameters.Gas);

                        if (stopTime >= _diveParameters.DiveConfig.SafeStopTime)
                            break;
                    }

                    stopDepth = 0;
                }

                CalculateCompartmentPresures(TimeStep, currentDepth, _diveParameters.Gas);
                var ceilingDepth = GetCurrentCeilingDepth(currentDepth, _diveParameters.DiveConfig.GradFactorHigh);

                totalTime += TimeStep;
                AddDivePointInfo(totalTime, ceilingDepth, currentDepth, _diveParameters.Gas);

                if (currentDepth <= 0)
                {
                    break;
                }
            }

            AddDivePointInfo(totalTime, 0, 0, _diveParameters.Gas, true);
            return totalTime;
        }

        private BreathGas SelectGas(DepthFactor currDepth, double decoStopDepth)
        {
            const double MaxDecoPpO = 1.6;
            const double OptimalDecoPpO = 1.5;

            if (_diveParameters.DecoGases.Count() == 0)
                return _diveParameters.Gas;

            var decoPpO = MaxDecoPpO;
            if ((currDepth.Depth - decoStopDepth) > (DecoStopsStep + double.Epsilon))
                decoPpO = OptimalDecoPpO;

            var gasesCanUse = new List<KeyValuePair<double, BreathGas>>(_diveParameters.DecoGases.Count());

            foreach (var decoGas in _diveParameters.DecoGases)
            {
                var currDecoPpO = BreathGas.GetGasPartialPreasureForDepth(currDepth, decoGas.PpO);
                if (currDecoPpO <= decoPpO)
                    gasesCanUse.Add(new KeyValuePair<double, BreathGas>(currDecoPpO, decoGas));
            }

            if (gasesCanUse.Count == 0)
                return _diveParameters.Gas;

            return gasesCanUse.First(g => DivingMath.CompareDouble(g.Key, gasesCanUse.Max(k => k.Key))).Value;
        }

        private void AddDivePointInfo(double time, double celingDepth, double currDepth, BreathGas gas, bool forceAdd = false)
        {
            if (TimeSpan.FromMinutes(time - _trackedDivePointTime).TotalSeconds >= 5 || time <= double.Epsilon || forceAdd)
            {
                _mValues.Add(new KeyValuePair<double, double>(time, celingDepth));
                _divePoints.Add(new DivePoint
                {
                    DepthFactor = new DepthFactor(currDepth, _diveParameters.DepthFactor.WaterDensity),
                    CurrentDiveTime = time,
                    CurrentGas = gas
                });

                _trackedDivePointTime = time;
            }
        }

        private double CalculateFullDesaturationTime()
        {
            const double desatTimeStep = 10.0;
            double desatTime = 0;
            var startAmbientPreasure = _airGas.PpN * (DivingMath.SeaLevelPreasureBars - DivingMath.WaterVapourPreasureBars) + double.Epsilon;

            while (true)
            {
                CalculateCompartmentPresures(desatTimeStep, 0, _airGas);
                desatTime += desatTimeStep;

                int desutCpts = 0;
                foreach (var cpt in _compartmentData)
                {
                    if (cpt.N2Presure <= (startAmbientPreasure + 0.0115) && cpt.HePresure <= 0.01)
                        ++desutCpts;
                }

                if (desutCpts == _compartmentData.Length)
                    break;
            }

            return desatTime;
        }

        private void CalculateCompartmentPresures(double time, double depth, BreathGas gas, CompartmentCalculations[] tissues = null)
        {
            var depthFact = new DepthFactor(depth, _diveParameters.DepthFactor.WaterDensity);
            double ppNitro = gas.PpN * (BreathGas.GetGasPartialPreasureForDepth(depthFact, 1.0) - DivingMath.WaterVapourPreasureBars);
            double ppHe = gas.PpHe * (BreathGas.GetGasPartialPreasureForDepth(depthFact, 1.0) - DivingMath.WaterVapourPreasureBars);

            int index = 0;

            foreach (var cpt in _compartments)
            {
                var data = (tissues ?? _compartmentData)[index];

                var n2Koef = 1.0 - Math.Pow(2.0, -time / cpt.N2HalfTime);
                var heKoef = 1.0 - Math.Pow(2.0, -time / cpt.HeHalfTime);

                data.N2Presure = data.N2Presure + ((ppNitro - data.N2Presure) * n2Koef);
                data.HePresure = data.HePresure + ((ppHe - data.HePresure) * heKoef);

                ++index;
            }
        }

        private double GetCurrentCeilingDepth(double currentDepth, double? customGF = null, CompartmentCalculations[] tissues = null)
        {
            int index = 0;
            double maxDepth = 0;
            double gradFactor = customGF ?? 0;

            if (gradFactor <= double.Epsilon)
            {
                if (_firstDecoStopDepth < double.Epsilon)
                    gradFactor = _diveParameters.DiveConfig.GradFactorLow;
                else
                {
                    currentDepth -= DecoStopsStep;
                    if (currentDepth < 0)
                        currentDepth = 0;

                    gradFactor = _diveParameters.DiveConfig.GradFactorHigh - 
                        (_diveParameters.DiveConfig.GradFactorHigh - _diveParameters.DiveConfig.GradFactorLow) / _firstDecoStopDepth * currentDepth;
                }
            }

            foreach (var cpt in _compartments)
            {
                var data = (tissues ?? _compartmentData)[index];
                var pTotal = data.HePresure + data.N2Presure;

                var bulmanA = ((cpt.N2ValueA * data.N2Presure) + (cpt.HeValueA * data.HePresure)) / pTotal;
                var bulmanB = ((cpt.N2ValueB * data.N2Presure) + (cpt.HeValueB * data.HePresure)) / pTotal;

                var tolerantPreasure = (pTotal - (bulmanA * gradFactor)) / ((gradFactor / bulmanB) + 1.0 - gradFactor);
                var tolerantDepth = DivingMath.PreasureBarsToDepth(tolerantPreasure, _diveParameters.DepthFactor.WaterDensity);

                if (tolerantDepth.Depth > maxDepth)
                    maxDepth = tolerantDepth.Depth;

                /*
                var minAmbientTolerantN2Presure = (data.N2Presure - gradFactor * cpt.N2ValueA) / (gradFactor / cpt.N2ValueB + 1.0 - gradFactor);
                var minAmbientTolerantHePresure = (data.HePresure - gradFactor * cpt.HeValueA) / (gradFactor / cpt.HeValueB + 1.0 - gradFactor);

                var tolerantN2Depth = DivingMath.PreasureBarsToDepth(minAmbientTolerantN2Presure, _diveParameters.DepthFactor.WaterDensity);
                var tolerantHeDepth = DivingMath.PreasureBarsToDepth(minAmbientTolerantHePresure, _diveParameters.DepthFactor.WaterDensity);

                if (tolerantN2Depth.Depth > maxDepth)
                    maxDepth = tolerantN2Depth.Depth;
                if (tolerantHeDepth.Depth > maxDepth)
                    maxDepth = tolerantHeDepth.Depth;
                 */

                ++index;
            }

            return maxDepth;
        }

        private static double GetNearestTime(double time)
        {
            const double nearestTimeStep = 0.5;

            var delta = time - Math.Floor(time);
            if (delta > 0)
                return Math.Floor(time) + (delta <= nearestTimeStep ? nearestTimeStep : 2.0 * nearestTimeStep);
           
            return time;
        }

        private static double FindNearestDecoDepth(double depth)
        {
            const double step = DecoStopsStep;

            var parts = (int)depth / (int)step;
            return parts * step + (depth / step > parts ? step : 0);
        }
    }
}