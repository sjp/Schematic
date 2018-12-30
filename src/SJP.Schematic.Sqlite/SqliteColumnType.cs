using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public class SqliteColumnType : IDbType
    {
        public SqliteColumnType(SqliteTypeAffinity typeAffinity)
        {
            if (!typeAffinity.IsValid())
                throw new ArgumentException($"The { nameof(SqliteTypeAffinity) } provided must be a valid enum.", nameof(typeAffinity));

            var typeName = typeAffinity.ToString().ToUpperInvariant();
            TypeName = typeName;
            Definition = typeName;

            DataType = _affinityTypeMap[typeAffinity];
            ClrType = _affinityClrTypeMap[typeAffinity];
        }

        public SqliteColumnType(SqliteTypeAffinity typeAffinity, SqliteCollation collation)
            : this(typeAffinity)
        {
            if (!typeAffinity.IsValid())
                throw new ArgumentException($"The { nameof(SqliteTypeAffinity) } provided must be a valid enum.", nameof(typeAffinity));
            if (!collation.IsValid())
                throw new ArgumentException($"The { nameof(SqliteCollation) } provided must be a valid enum.", nameof(collation));
            if (typeAffinity != SqliteTypeAffinity.Text)
                throw new ArgumentException("The type affinity must be a text type when a collation has been provided.", nameof(typeAffinity));

            Collation = collation != SqliteCollation.None
                ? Option<Identifier>.Some(collation.ToString().ToUpperInvariant())
                : Option<Identifier>.None;
        }

        public Identifier TypeName { get; }

        public DataType DataType { get; }

        public string Definition { get; }

        public bool IsFixedLength { get; }

        public int MaxLength { get; } = -1;

        public Type ClrType { get; }

        public Option<NumericPrecision> NumericPrecision { get; } = Option<NumericPrecision>.None;

        public Option<Identifier> Collation { get; }

        private readonly IReadOnlyDictionary<SqliteTypeAffinity, DataType> _affinityTypeMap = new Dictionary<SqliteTypeAffinity, DataType>
        {
            [SqliteTypeAffinity.Blob] = DataType.LargeBinary,
            [SqliteTypeAffinity.Integer] = DataType.BigInteger,
            [SqliteTypeAffinity.Numeric] = DataType.Numeric,
            [SqliteTypeAffinity.Real] = DataType.Float,
            [SqliteTypeAffinity.Text] = DataType.UnicodeText
        };

        private readonly IReadOnlyDictionary<SqliteTypeAffinity, Type> _affinityClrTypeMap = new Dictionary<SqliteTypeAffinity, Type>
        {
            [SqliteTypeAffinity.Blob] = typeof(byte[]),
            [SqliteTypeAffinity.Integer] = typeof(long),
            [SqliteTypeAffinity.Numeric] = typeof(decimal),
            [SqliteTypeAffinity.Real] = typeof(double),
            [SqliteTypeAffinity.Text] = typeof(string)
        };
    }
}
