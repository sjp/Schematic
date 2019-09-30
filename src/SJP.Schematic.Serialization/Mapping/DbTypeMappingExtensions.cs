using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DbTypeMappingExtensions
    {
        public static Dto.DbType ToDto(this IDbType columnType)
        {
            if (columnType == null)
                throw new ArgumentNullException(nameof(columnType));

            return new Dto.DbType
            {
                TypeName = columnType.TypeName.ToDto(),
                ClrTypeName = columnType.ClrType.ToString(),
                Collation = columnType.Collation.ToDto(),
                DataType = columnType.DataType,
                Definition = columnType.Definition,
                IsFixedLength = columnType.IsFixedLength,
                MaxLength = columnType.MaxLength,
                NumericPrecision = columnType.NumericPrecision.ToDto()
            };
        }

        public static IDbType FromDto(this Dto.DbType? dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var typeName = dto.TypeName.FromDto();
            var clrType = Type.GetType(dto.ClrTypeName);
            var numericPrecision = dto.NumericPrecision.FromDto();
            var collation = dto.Collation.FromDto();

            return new ColumnDataType(
                (Identifier)typeName,
                dto.DataType,
                dto.Definition!,
                clrType,
                dto.IsFixedLength,
                dto.MaxLength,
                numericPrecision,
                collation
            );
        }
    }
}
