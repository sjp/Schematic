using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class ColumnMappingExtensions
    {
        public static Dto.DatabaseColumn ToDto(this IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var columnName = column.Name.ToDto();
            var columnType = column.Type.ToDto();
            var autoIncr = column.AutoIncrement.ToDto();
            var defaultValue = column.DefaultValue.IfNoneUnsafe((string)null);

            return new Dto.DatabaseColumn
            {
                Name = columnName,
                Type = columnType,
                AutoIncrement = autoIncr,
                DefaultValue = defaultValue,
                IsComputed = column.IsComputed,
                IsNullable = column.IsNullable
            };
        }

        public static IDatabaseColumn FromDto(this Dto.DatabaseColumn dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var name = dto.Name.FromDto();
            var dbType = dto.Type.FromDto();
            var autoIncr = dto.AutoIncrement.FromDto();

            return new DatabaseColumn(
                (Identifier)name,
                dbType,
                dto.IsNullable,
                dto.DefaultValue,
                autoIncr
            );
        }
    }
}
