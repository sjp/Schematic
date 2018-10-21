using System;

namespace SJP.Schematic.Core
{
    public class DatabaseSequence : IDatabaseSequence
    {
        public DatabaseSequence(
            IRelationalDatabase database,
            Identifier sequenceName,
            decimal start,
            decimal increment,
            decimal? minValue,
            decimal? maxValue,
            bool cycle,
            int cacheSize
        )
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var serverName = sequenceName.Server ?? database.ServerName;
            var databaseName = sequenceName.Database ?? database.DatabaseName;
            var schemaName = sequenceName.Schema ?? database.DefaultSchema;

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, sequenceName.LocalName);

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
    }
}
