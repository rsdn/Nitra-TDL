using System;

namespace Tdl.Transformator.Models
{
    [Flags]
    public enum PrintParameters
    {
        Default = 0,
        NoStartTab = 1 << 0,
        AddQuotes = 1 << 1,
        NoComments = 1 << 2,

        NameOnly = NoComments | NoStartTab | AddQuotes
    }
}
