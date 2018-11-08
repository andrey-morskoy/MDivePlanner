using System;
using System.Collections.Generic;
using System.Text;

namespace MDivePlanner.Domain.Entities
{
    public struct DepthTime
    {
        public double Depth { get; set; }

        public double Time { get; set; }

        public DepthTime(double depth , double time)
        {
            Depth = depth;
            Time = time;
        }

        public bool IsEmpty()
        {
            return Depth < double.Epsilon && Time < double.Epsilon;
        }
    }
}
