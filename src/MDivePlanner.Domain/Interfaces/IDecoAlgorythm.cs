using MDivePlanner.Domain.Entities;
using System;
using System.Collections.Generic;

namespace MDivePlanner.Domain.Interfaces
{
    public interface IDecoAlgorythm
    {
        CalculatedDiveResult ProcessDive(DiveParameters diveParameters, object tissuePreasures = null);
    }
}
