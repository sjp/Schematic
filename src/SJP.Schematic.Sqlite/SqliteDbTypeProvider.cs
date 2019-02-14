﻿using System;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDbTypeProvider : IDbTypeProvider
    {
        public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            var typeName = GetDefaultTypeName(typeMetadata.DataType);
            var affinity = GetAffinity(typeName);
            var collation = typeMetadata.Collation.Match(
                c => Enum.TryParse(c.LocalName, out SqliteCollation sc) ? sc : SqliteCollation.None,
                () => SqliteCollation.None
            );

            return collation == SqliteCollation.None
                ? new SqliteColumnType(affinity)
                : new SqliteColumnType(affinity, collation);
        }

        public IDbType GetComparableColumnType(IDbType otherType)
        {
            if (otherType == null)
                throw new ArgumentNullException(nameof(otherType));

            // only interested in these two bits of information
            var typeMetadata = new ColumnTypeMetadata
            {
                Collation = otherType.Collation,
                DataType = otherType.DataType
            };

            return CreateColumnType(typeMetadata);
        }

        protected static string GetDefaultTypeName(DataType dataType)
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));

            switch (dataType)
            {
                case DataType.Binary:
                case DataType.LargeBinary:
                    return "BLOB";
                case DataType.SmallInteger:
                case DataType.BigInteger:
                case DataType.Boolean:
                case DataType.Integer:
                    return "INTEGER";
                case DataType.Float:
                    return "REAL";
                case DataType.Date:
                case DataType.DateTime:
                case DataType.Interval:
                case DataType.Time:
                case DataType.Numeric:
                    return "NUMERIC";
                case DataType.String:
                case DataType.Text:
                case DataType.Unicode:
                case DataType.UnicodeText:
                    return "TEXT";
                default:
                    return "NUMERIC";
            }
        }

        protected static SqliteTypeAffinity GetAffinity(string typeName) => AffinityParser.ParseTypeName(typeName);

        private static readonly SqliteTypeAffinityParser AffinityParser = new SqliteTypeAffinityParser();
    }
}
