using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Tests.Fakes
{
    internal sealed class FakeDatabaseDialect : IDatabaseDialect
    {
        public IDbTypeProvider TypeProvider => null;

        public bool IsReservedKeyword(string text) => ReservedKeywords.Contains(text);

        public IEnumerable<string> ReservedKeywords { get; set; } = new List<string>();

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

        public Task<IRelationalDatabase> GetRelationalDatabaseAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IRelationalDatabase>(null);

        public Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IRelationalDatabaseCommentProvider>(new EmptyRelationalDatabaseCommentProvider());
    }
}
