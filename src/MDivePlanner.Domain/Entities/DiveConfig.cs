﻿using MDivePlanner.Domain.Logic;
using System;

namespace MDivePlanner.Domain.Entities
{
    public class DiveConfig
    {
        public enum MinDecoTimeStep
        {
            HalfMin = 0,
            FullMin
        }

        public double MaxAscentSpeed { get; set; }

        public double MaxDescentSpeed { get; set; }

        public double SafeStopDepth { get; set; }

        public double SafeStopTime { get; set; }

        public double GradFactorLow { get; set; }

        public double GradFactorHigh { get; set; }

        public double BottomRmv { get; set; }

        public double DecoRmv { get; set; }

        public MinDecoTimeStep MinDecoTime { get; set; }

        public Zhl16Algorithm.SubType AlgoSubType { get; set; }
    }
}
