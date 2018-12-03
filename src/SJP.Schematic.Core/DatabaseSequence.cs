using System;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DatabaseSequence : IDatabaseSequence
    {
        public DatabaseSequence(
            Identifier sequenceName,
            decimal start,
            decimal increment,
            decimal? minValue,
            decimal? maxValue,
            bool cycle,
            int cacheSize
        )
        {
            Name = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
            Start = start;

            if (increment == 0)
                throw new ArgumentException("A non-zero increment is required", nameof(increment));
            Increment = increment;

            if (increment > 0)
            {
                if (minValue.HasValue && minValue > start)
                    throw new ArgumentException("When a minimum value and positive increment is provided, the minimum value must not be larger than the starting value.", nameof(minValue));
                if (maxValue.HasValue && maxValue < start)
                    throw new ArgumentException("When a maximum value and positive increment is provided, the maximum value must not be less than the starting value.", nameof(maxValue));
            }
            else
            {
                if (minValue.HasValue && minValue < start)
                    throw new ArgumentException("When a minimum value and negative increment is provided, the minimum value must not be less than the starting value.", nameof(minValue));
                if (maxValue.HasValue && maxValue > start)
                    throw new ArgumentException("When a maximum value and negative increment is provided, the maximum value must not be larger than the starting value.", nameof(maxValue));
            }

            if (cacheSize < 0)
                cacheSize = UnknownCacheSize;

            Cache = cacheSize;
            Cycle = cycle;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public Identifier Name { get; }

        public int Cache { get; }

        public bool Cycle { get; }

        public decimal Increment { get; }

        public decimal? MaxValue { get; }

        public decimal? MinValue { get; }

        public decimal Start { get; }

        public static int UnknownCacheSize { get; } = -1;

        public override string ToString() => "Sequence: " + Name.ToString();

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Sequence: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
