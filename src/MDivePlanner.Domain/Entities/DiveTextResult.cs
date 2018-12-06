using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MDivePlanner.Domain.Logic;

namespace MDivePlanner.Domain.Entities
{
    public struct TextResultBlock
    {
        public string Text { get; set; }

        public int Level { get; set; }

        public TextResultBlock(string text, int level = 0)
        {
            Text = text + "\r\n";
            Level = level;
        }
    }


    public class DiveTextResult
    {
        private List<TextResultBlock> _blocks = new List<TextResultBlock>(2 * sizeof(double));

        public IEnumerable<TextResultBlock> Blocks => _blocks;

        public DiveTextResult(CalculatedDivePlan plan)
        {
            string newLine = string.Empty;

            _blocks.Add(new TextResultBlock("DIVE PLAN"));
            _blocks.Add(new TextResultBlock(newLine));

            foreach (var block in plan.DiveParameters.GetDiveParamsInfo())
                _blocks.Add(new TextResultBlock(block));

            _blocks.Add(new TextResultBlock(newLine));

            var prevPoint = plan.PlanPoints.FirstOrDefault();
            var firstDecoStop = plan.PlanPoints.FirstOrDefault(p => p.Type == DivePlanPointType.Deco || p.Type == DivePlanPointType.SafeStop);
            var density = plan.DiveParameters.Levels.First().DepthFactor.WaterDensity;

            foreach (var point in plan.PlanPoints)
            {
                var poindDepth = new DepthFactor(point.Depth, density);
                var end = DivingMath.CalculateEND(poindDepth, point.Gas);
                var ppO = BreathGas.GetGasPartialPreasureForDepth(poindDepth, point.Gas.PpO);

                switch (point.Type)
                {
                    case DivePlanPointType.Descent | DivePlanPointType.Bottom:
                        {
                            if (DivingMath.CompareDouble(prevPoint?.Depth ?? 0, point.Depth))
                            {
                                _blocks.Add(new TextResultBlock(string.Format("Level at\t{0}m\t {1}   ({2})   {3}\t ppO2: {4}   END: {5}m",
                                      Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                                      TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(ppO, 2), Math.Round(end, 1))));
                            }
                            else
                            {
                                _blocks.Add(new TextResultBlock(string.Format("Dec to\t{0}m\t {1}   ({2})   {3}\t {4}m/min descent",
                                     Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                                     TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(plan.DiveParameters.DiveConfig.MaxDescentSpeed, 1))));
                            }
                        }
                        break;
                    case DivePlanPointType.Ascent | DivePlanPointType.Bottom:
                    case DivePlanPointType.Ascent | DivePlanPointType.Bottom | DivePlanPointType.FinalAscent:
                        {
                            if (DivingMath.CompareDouble(prevPoint?.Depth ?? 0, point.Depth))
                            {
                                _blocks.Add(new TextResultBlock(string.Format("Level at\t{0}m\t {1}   ({2})   {3}\t ppO2: {4}   END: {5}m",
                                      Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                                      TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(ppO, 2), Math.Round(end, 1))));
                            }
                            else
                            {
                                _blocks.Add(new TextResultBlock(string.Format("Asc to\t{0}m\t {1}   ({2})   {3}\t {4}m/min ascent",
                                    Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                                    TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(plan.DiveParameters.DiveConfig.MaxAscentSpeed, 1))));
                            }
                        }
                        break;
                    case DivePlanPointType.Deco:
                    case DivePlanPointType.SafeStop:
                        if (DivingMath.CompareDouble(firstDecoStop.Depth, point.Depth))
                        {
                            _blocks.Add(new TextResultBlock(string.Format("Asc to\t{0}m\t {1}   ({2})   {3}\t {4}m/min ascent",
                                    Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                                    TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(plan.DiveParameters.DiveConfig.MaxAscentSpeed, 1))));
                        }
                        break;
                    case DivePlanPointType.SafeStop | DivePlanPointType.FinalAscent:
                    case DivePlanPointType.Deco | DivePlanPointType.FinalAscent:
                        _blocks.Add(new TextResultBlock(string.Format("Stop at\t{0}m\t {1}   ({2})   {3}\t ppO2: {4}   END: {5}m",
                            Math.Round(point.Depth, 1), TimeString(prevPoint.AbsoluteTime),
                           TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime), point.Gas.Name, Math.Round(ppO, 2), Math.Round(end, 1))));
                        break;
                    case DivePlanPointType.EndDive:
                        _blocks.Add(new TextResultBlock(string.Format("Surface\t\t {0}   ({1})", 
                            TimeString(point.AbsoluteTime), TimeString(point.AbsoluteTime - prevPoint.AbsoluteTime))));
                        break;
                }

                prevPoint = point;
            }

            _blocks.Add(new TextResultBlock(newLine));

            var diveInfo = plan.GetDiveInfo();
            var cns = diveInfo.FirstOrDefault(b => b.Type == DiveResultBlockType.CNS);
            var consumedGas = diveInfo.FirstOrDefault(b => b.Type == DiveResultBlockType.ConsumedGas);
            var noDecoTime = diveInfo.FirstOrDefault(b => b.Type == DiveResultBlockType.NoDecoTime);
            var ascentTime = diveInfo.FirstOrDefault(b => b.Type == DiveResultBlockType.AscentTime);
            var fullDesatTime = diveInfo.FirstOrDefault(b => b.Type == DiveResultBlockType.FullDesaturation);

            _blocks.Add(new TextResultBlock("CNS:  " + cns.Text));
            _blocks.Add(new TextResultBlock("No Deco Time:  " + noDecoTime.Text));
            _blocks.Add(new TextResultBlock("Acsent Time:  " + ascentTime.Text));
            _blocks.Add(new TextResultBlock("Full Desaturation:  " + fullDesatTime.Text));
            _blocks.Add(new TextResultBlock("Consumed Gases:  " + consumedGas.Text));
        }

        private static string TimeString(double absoluteTime)
        {
            var timeSpan = TimeSpan.FromMinutes(absoluteTime);
            return string.Format("{0}:{1}", ((int)timeSpan.TotalMinutes).ToString("D2"), timeSpan.Seconds.ToString("D2"));
        }
    }
}
