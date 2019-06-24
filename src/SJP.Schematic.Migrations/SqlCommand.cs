using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Migrations
{
    public class SqlCommand : ISqlCommand
    {
        public SqlCommand(string sql)
        {
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            Parameters = EmptyParameters;
        }

        public SqlCommand(string sql, IReadOnlyDictionary<string, object> parameters)
        {
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Sql { get; }

        public IReadOnlyDictionary<string, object> Parameters { get; }

        private readonly static IReadOnlyDictionary<string, object> EmptyParameters = new Dictionary<string, object>();
    }
}
