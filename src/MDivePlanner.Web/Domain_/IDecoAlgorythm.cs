using System;
using System.Collections.Generic;

namespace MDivePlannerWeb.Domain
{
    public interface IDecoAlgorythm
    {
        CalculatedDiveResult ProcessDive(DiveParameters diveParameters, object tissuePreasures = null);
    }
}
