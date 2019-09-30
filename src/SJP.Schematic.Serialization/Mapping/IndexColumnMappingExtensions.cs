using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class IndexColumnMappingExtensions
    {
        public static Dto.DatabaseIndexColumn ToDto(this IDatabaseIndexColumn indexColumn)
        {
            if (indexColumn == null)
                throw new ArgumentNullException(nameof(indexColumn));

            var columns = indexColumn.DependentColumns.Select(c => c.ToDto()).ToList();

            return new Dto.DatabaseIndexColumn
            {
                Order = indexColumn.Order,
                DependentColumns = columns,
                Expression = indexColumn.Expression
            };
        }

        public static IDatabaseIndexColumn FromDto(this Dto.DatabaseIndexColumn dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Expression == null)
                throw new ArgumentException("The given index column expression is null.", nameof(dto));

            var columns = dto.DependentColumns.Select(c => c.FromDto()).ToList();

            return new DatabaseIndexColumn(
                dto.Expression,
                columns,
                dto.Order
            );
        }
    }
}
