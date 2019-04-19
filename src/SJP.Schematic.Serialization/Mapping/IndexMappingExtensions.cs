using System;
using System.Linq;
using SJP.Schematic.Core;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class IndexMappingExtensions
    {
        public static Dto.DatabaseIndex ToDto(this IDatabaseIndex index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            var indexName = index.Name?.ToDto();
            var columns = index.Columns.Select(c => c.ToDto()).ToList();
            var includedColumns = index.IncludedColumns.Select(c => c.ToDto()).ToList();

            return new Dto.DatabaseIndex
            {
                Name = indexName,
                Columns = columns,
                IncludedColumns = includedColumns,
                IsUnique = index.IsUnique,
                IsEnabled = index.IsEnabled
            };
        }

        public static IDatabaseIndex FromDto(this Dto.DatabaseIndex dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var name = dto.Name.FromDto();
            var columns = dto.Columns.Select(c => c.FromDto()).ToList();
            var includedColumns = dto.IncludedColumns.Select(c => c.FromDto()).ToList();

            return new DatabaseIndex(
                (Identifier)name,
                dto.IsUnique,
                columns,
                includedColumns,
                dto.IsEnabled
            );
        }
    }
}
