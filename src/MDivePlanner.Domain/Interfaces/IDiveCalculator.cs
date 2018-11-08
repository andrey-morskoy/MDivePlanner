using MDivePlanner.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDivePlanner.Domain.Interfaces
{
    public interface IDiveCalculator
    {
        CalculatedDivePlan Calculate(CalculatedDivePlan prevDive, DiveParameters diveParameters);
    }
}
