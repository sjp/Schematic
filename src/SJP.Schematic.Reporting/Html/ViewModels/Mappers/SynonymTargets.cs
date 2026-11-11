using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SynonymTargets
{
    public SynonymTargets(IEnumerable<DatabaseObject> databaseObjects)
    {
        ArgumentNullException.ThrowIfNull(databaseObjects);

        var objects = databaseObjects.ToList();
        if (objects.Exists(static o => o.Value is null))
            throw new ArgumentNullException(nameof(databaseObjects), "A database object provided holds no object.");

        // One set of names per kind of object, searched in order of precedence, so a kind that ranks
        // higher wins even when a lower-ranked kind matches the target's casing exactly.
        NamesByKind = objects
            .GroupBy(DatabaseObjectPrecedence.Of)
            .OrderBy(static kind => kind.Key)
            .Select(static kind => new ObjectNames(kind))
            .ToList();
    }

    private IReadOnlyList<ObjectNames> NamesByKind { get; }

    /// <summary>
    /// Resolves a synonym's target to the hash route of the object it names.
    /// </summary>
    /// <param name="targetName">The name of the object a synonym aliases.</param>
    /// <returns>The target's route, or none when the target is not a known object. When the name
    /// is shared by several kinds of object, a table wins over a view, then a sequence, synonym,
    /// routine and finally a user-defined type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetName"/> is <see langword="null" />.</exception>
    public LanguageExt.Option<Uri> GetTargetUrl(Identifier targetName)
    {
        ArgumentNullException.ThrowIfNull(targetName);

        foreach (var names in NamesByKind)
        {
            if (names.TryResolve(targetName, out var databaseObject))
                return new Uri(UrlRouter.GetUrl(databaseObject), UriKind.Relative);
        }

        return LanguageExt.Option<Uri>.None;
    }

    // Target names are matched case-insensitively, so a synonym whose target differs only in
    // casing still resolves. The route is built from the object's own name rather than the
    // target's spelling, because route keys are case-sensitive. An exact match wins, so objects
    // whose names differ only in casing each keep their own route.
    private sealed class ObjectNames
    {
        public ObjectNames(IEnumerable<DatabaseObject> databaseObjects)
        {
            foreach (var databaseObject in databaseObjects)
            {
                var name = databaseObject.Name;
                ExactNames.TryAdd(name, databaseObject);
                NamesIgnoringCase.TryAdd(name, databaseObject);
            }
        }

        private Dictionary<Identifier, DatabaseObject> ExactNames { get; } = [];

        private Dictionary<Identifier, DatabaseObject> NamesIgnoringCase { get; } = new(IdentifierComparer.OrdinalIgnoreCase);

        public bool TryResolve(Identifier targetName, out DatabaseObject databaseObject)
        {
            return ExactNames.TryGetValue(targetName, out databaseObject)
                || NamesIgnoringCase.TryGetValue(targetName, out databaseObject);
        }
    }
}
