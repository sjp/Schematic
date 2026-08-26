using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Tests;

/// <summary>
/// Resolves identifiers verbatim while recording what it was asked to resolve,
/// so that a test can tell which resolver a mapped provider was built with.
/// </summary>
internal sealed class RecordingIdentifierResolutionStrategy : IIdentifierResolutionStrategy
{
    public IReadOnlyCollection<Identifier> ResolvedNames => _resolvedNames;

    private readonly List<Identifier> _resolvedNames = [];

    public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
    {
        _resolvedNames.Add(identifier);
        yield return identifier;
    }
}