using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Tests.Fakes;

internal sealed class FakeDatabaseDialect : IDatabaseDialect
{
    public IDbTypeProvider TypeProvider => null;

    public IDatabaseDialectCapabilities Capabilities { get; set; } = new DatabaseDialectCapabilities
    {
        SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.NoAction },
        MaxIdentifierLength = int.MaxValue,
    };

    public IDependencyProvider GetDependencyProvider() => new EmptyDependencyProvider();

    public bool IsReservedKeyword(string text) => ReservedKeywords.Contains(text, StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> ReservedKeywords { get; set; } = [];

    public string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}