using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class IndexesModelMapper :
        IDatabaseModelMapper<IDatabaseTableIndex, Indexes.Index>
    {
        public IndexesModelMapper(IDatabaseDialect dialect)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDatabaseDialect Dialect { get; }

        public Indexes.Index Map(IDatabaseTableIndex dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return new Indexes.Index(
                dbObject.Name?.LocalName,
                dbObject.Table.Name,
                dbObject.IsUnique,
                dbObject.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                dbObject.Columns.Select(c => c.Order).ToList(),
                dbObject.IncludedColumns.Select(c => c.Name.LocalName).ToList()
            );
        }
    }
}
