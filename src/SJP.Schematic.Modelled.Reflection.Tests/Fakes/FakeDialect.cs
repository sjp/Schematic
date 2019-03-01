using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    internal sealed class FakeDialect : IDatabaseDialect
    {
        public bool IsReservedKeyword(string text) => false;

        public Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IIdentifierDefaults>(null);

        public Task<string> GetDatabaseDisplayVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<string>(null);

        public Task<Version> GetDatabaseVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<Version>(null);

        public string QuoteIdentifier(string identifier) => null;

        public string QuoteName(Identifier name) => null;

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

        public Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult<IRelationalDatabase>(null);

        public Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult<IRelationalDatabaseCommentProvider>(new EmptyRelationalDatabaseCommentProvider());
    }
}
