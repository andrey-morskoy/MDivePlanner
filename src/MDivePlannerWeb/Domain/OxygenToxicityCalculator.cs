using System;
using System.Linq;
using System.Collections.Generic;

namespace MDivePlannerWeb.Domain
{
    public static class OxygenToxicityCalculator
    {
        private struct Point
        {
            public double PpO;
            public double Exposure;
        }

        private static readonly Point[] _cnsTable;

        static OxygenToxicityCalculator()
        {
            _cnsTable = new Point[]
            {
                new Point { PpO = 2.5,  Exposure = 1.00 },
                new Point { PpO = 2.0,  Exposure = 1.85 },
                new Point { PpO = 1.9,  Exposure = 1.86 },
                new Point { PpO = 1.8,  Exposure = 1.86 },
                new Point { PpO = 1.7,  Exposure = 8.25 },
                new Point { PpO = 1.65, Exposure = 19.8 },
                new Point { PpO = 1.6,  Exposure = 45.0 },
                new Point { PpO = 1.55, Exposure = 82.0 },
                new Point { PpO = 1.5,  Exposure = 120.0 },
                new Point { PpO = 1.45, Exposure = 135.0 },
                new Point { PpO = 1.4,  Exposure = 150.0 },
                new Point { PpO = 1.35, Exposure = 165.0 },
                new Point { PpO = 1.3,  Exposure = 180.0 },
                new Point { PpO = 1.25, Exposure = 195.0 },
                new Point { PpO = 1.2,  Exposure = 210.0 },
                new Point { PpO = 1.1,  Exposure = 240.0 },
                new Point { PpO = 1.0,  Exposure = 300.0 },
                new Point { PpO = 0.9,  Exposure = 360.0 },
                new Point { PpO = 0.8,  Exposure = 450.0 },
                new Point { PpO = 0.7,  Exposure = 570.0 },
                new Point { PpO = 0.6,  Exposure = 720.0 },
                new Point { PpO = 0.5,  Exposure = 1100.0 }
            };
        }

        public static double CalulateOxygenCns(IEnumerable<DivePoint> divePoints)
        {
            double cns = 0;
            DivePoint? prevPoint = null;

            foreach (var point in divePoints)
            {
                if (prevPoint.HasValue)
                {
                    var timeExposure = point.CurrentDiveTime - prevPoint.Value.CurrentDiveTime;
                    var avgDepth = (prevPoint.Value.DepthFactor.Depth + point.DepthFactor.Depth) * 0.5;
                    var ambPreasure = BreathGas.GetGasPartialPreasureForDepth(new DepthFactor(avgDepth, point.DepthFactor.WaterDensity), 1.0);
                    var ppO2 = point.CurrentGas.PpO * (ambPreasure - DivingMath.WaterVapourPreasureBars);

                    if (ppO2 >= _cnsTable.Last().PpO)
                    {
                        int ind = -1;
                        for (int i = 1; i < _cnsTable.Length; i++)
                        {
                            if (ppO2 > _cnsTable[i].PpO)
                            {
                                ind = i - 1;
                                break;
                            }
                        }

                        if (ind < 0)
                            ind = _cnsTable.Length - 1;

                        var cnsRecord = _cnsTable[ind];
                        var cnsValue = cnsRecord.Exposure;

                        if (ppO2 > _cnsTable.First().PpO)
                        {
                            cnsValue = _cnsTable.First().Exposure;
                        }
                        else if (ind < (_cnsTable.Length - 1))
                        {
                            var prevRecord = _cnsTable[ind + 1];
                            var deltaPpO2 = ppO2 - prevRecord.PpO;
                            if (deltaPpO2 > 0)
                            {
                                // interpolate
                                var recordsDelta = Math.Abs(cnsRecord.PpO - prevRecord.PpO);
                                var offset = deltaPpO2 / recordsDelta;

                                cnsValue = offset * cnsRecord.Exposure + (1.0 - offset) * prevRecord.Exposure;
                            }
                        }

                        cns += timeExposure / cnsValue * 100.0;
                    }
                }

                prevPoint = point;
            }


            return Math.Round(cns, 1);
        }
    }
}
