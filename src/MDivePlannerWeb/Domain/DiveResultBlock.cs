using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDivePlannerWeb.Domain
{
    public enum DiveResultBlockType
    {
        Default = 0,
        DepthTime,
        MainGas,
        DecoGases,
        CNS,
        END,
        ConsumedGas,
        MaxPpO,
        FullDesaturation,
        NoDecoTime,
        AscentTime
    }


    public struct DiveResultBlock
    {
        public DiveResultBlockType Type { get; set; }

        public string Text { get; set; }

        public bool Dangerous { get; set; }

        public bool Warning { get; set; }

        public bool Important { get; set; }

        public DiveResultBlock(string text, bool dangerous = false, bool warning = false, bool important = false, DiveResultBlockType type = DiveResultBlockType.Default)
        {
            Type = type;
            Text = text;
            Dangerous = dangerous;
            Warning = warning;
            Important = important;
        }
    }
}
