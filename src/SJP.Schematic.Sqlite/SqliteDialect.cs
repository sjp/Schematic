using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDialect : DatabaseDialect<SqliteDialect>
    {
        public override IDbConnection CreateConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public override async Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public override IDbType CreateColumnType(ColumnTypeMetadata typeMetadata)
        {
            if (typeMetadata == null)
                throw new ArgumentNullException(nameof(typeMetadata));

            var typeName = GetDefaultTypeName(typeMetadata.DataType);
            var affinity = GetAffinity(typeName);

            if (!Enum.TryParse(typeMetadata.Collation?.LocalName, out SqliteCollation collation))
                collation = SqliteCollation.None;

            return collation == SqliteCollation.None
                ? new SqliteColumnType(affinity)
                : new SqliteColumnType(affinity, collation);
        }

        public override IDbType GetComparableColumnType(IDbType otherType)
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

        protected string GetDefaultTypeName(DataType dataType)
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

        protected static SqliteTypeAffinity GetAffinity(string typeName) => _affinityParser.ParseTypeName(typeName);

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();

        public override bool IsValidColumnName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidConstraintName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidObjectName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteName(Identifier name)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));

            if (name.Schema != null)
            {
                return QuoteIdentifier(name.Schema)
                    + "."
                    + QuoteIdentifier(name.LocalName);
            }
            else
            {
                return QuoteIdentifier(name.LocalName);
            }
        }
    }
}
