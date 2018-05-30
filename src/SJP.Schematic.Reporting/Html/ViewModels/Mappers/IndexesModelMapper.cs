using System;
using System.Data;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal class IndexesModelMapper :
        IDatabaseModelMapper<IDatabaseTableIndex, Indexes.Index>
    {
        public IndexesModelMapper(IDatabaseDialect dialect)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDatabaseDialect Dialect { get; }

        public Indexes.Index Map(IDatabaseTableIndex dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return new Indexes.Index(dbObject.Table.Name)
            {
                Name = dbObject.Name?.LocalName ?? string.Empty,
                Unique = dbObject.IsUnique,
                Columns = dbObject.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                IncludedColumns = dbObject.IncludedColumns.Select(c => c.Name.LocalName).ToList(),
                ColumnSorts = dbObject.Columns.Select(c => c.Order).ToList()
            };
        }
    }
}
