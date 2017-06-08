using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseSequence : IDatabaseEntity
    {
        int Cache { get; }

        bool Cycle { get; }

        decimal Increment { get; }

        decimal? MaxValue { get; }

        decimal? MinValue { get; }

        decimal Start { get; }
    }
}
