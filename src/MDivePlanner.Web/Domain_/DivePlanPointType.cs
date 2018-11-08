using System;

namespace MDivePlannerWeb.Domain
{
    public enum DivePlanPointType
    {
        Default = 0,
        StartDive = 2,
        EndDive = 4,
        Ascent = 8,
        Descent = 16,
        Bottom = 32,
        Deco = 64,
        SafeStop = 128
    }
}
