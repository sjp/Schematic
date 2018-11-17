using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class RelationalDatabaseView : IRelationalDatabaseView
    {
        public RelationalDatabaseView(
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns,
            IReadOnlyCollection<IDatabaseIndex> indexes)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            IsIndexed = Indexes.Count > 0;
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Definition);

        public bool IsIndexed { get; }

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Indexes);

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Columns);
    }
}
