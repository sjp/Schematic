using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseTrigger : IDatabaseTrigger
    {
        public SqlServerDatabaseTrigger(IRelationalDatabaseTable table, Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events, bool isEnabled)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));
            if (events == TriggerEvent.None)
                throw new ArgumentException("Invalid trigger event flags given. Must include at least one event, e.g. INSERT, DELETE, UPDATE.", nameof(events));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = name.LocalName;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            QueryTiming = queryTiming;
            TriggerEvent = events;
            IsEnabled = isEnabled;
        }

        public string Definition { get; }

        public Identifier Name { get; }

        public TriggerQueryTiming QueryTiming { get; }

        public TriggerEvent TriggerEvent { get; }

        public IRelationalDatabaseTable Table { get; }

        public bool IsEnabled { get; }
    }
}
