using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class IndexesModelMapper
    {
        public Indexes.Index Map(Identifier parent, IDatabaseIndex index)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            return new Indexes.Index(
                index.Name?.LocalName,
                parent,
                index.IsUnique,
                index.Columns.Select(c => c.Expression).ToList(),
                index.Columns.Select(c => c.Order).ToList(),
                index.IncludedColumns.Select(c => c.Name.LocalName).ToList()
            );
        }
    }
}
