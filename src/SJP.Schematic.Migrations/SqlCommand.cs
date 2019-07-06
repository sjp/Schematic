using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Migrations
{
    public class SqlCommand : ISqlCommand
    {
        public SqlCommand(string query)
        {
            if (query.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(query));

            Parameters = EmptyParameters;
        }

        public SqlCommand(string query, IReadOnlyDictionary<string, object> parameters)
        {
            if (query.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(query));

            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Query { get; }

        public IReadOnlyDictionary<string, object> Parameters { get; }

        private readonly static IReadOnlyDictionary<string, object> EmptyParameters = new Dictionary<string, object>();
    }
}
