using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    internal sealed class FakeDialect : IDatabaseDialect
    {
        public bool IsReservedKeyword(string text) => false;

        public Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IIdentifierDefaults>(null);

        public Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default) => Task.FromResult<string>(null);

        public Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default) => Task.FromResult<Version>(null);

        public string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Server != null)
                pieces.Add(QuoteIdentifier(name.Server));
            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        public string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"\"{ identifier.Replace("\"", "\"\"") }\"";
        }

        public IDbTypeProvider TypeProvider => InnerTypeProvider;

        private static readonly IDbTypeProvider InnerTypeProvider = new DbTypeProvider();

        private sealed class DbTypeProvider : IDbTypeProvider
        {
            public IDbType CreateColumnType(ColumnTypeMetadata typeMetadata) => new ColumnDataType(
                "test_type",
                DataType.BigInteger,
                "test_type(10)",
                typeof(object),
                false,
                10,
                new NumericPrecision(10, 0),
                null
            );

            public IDbType GetComparableColumnType(IDbType otherType) => CreateColumnType(null);
        }

        public Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IRelationalDatabase>(null);

        public Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IRelationalDatabaseCommentProvider>(new EmptyRelationalDatabaseCommentProvider());
    }
}
