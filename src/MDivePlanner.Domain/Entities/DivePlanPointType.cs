using System;

namespace MDivePlanner.Domain.Entities
{
    public enum DivePlanPointType
    {
        Default = 0,
        StartDive = 2,
        EndDive = 4,
        Ascent = 8,
        Descent = 16,
        Bottom = 32,
        FinalAscent = 64,
        Deco = 128,
        SafeStop = 256,
    }
}
