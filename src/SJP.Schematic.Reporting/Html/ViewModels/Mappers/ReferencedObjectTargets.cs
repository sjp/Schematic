using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ReferencedObjectTargets
{
    public ReferencedObjectTargets(IDependencyProvider dependencyProvider, IEnumerable<DatabaseObject> databaseObjects)
    {
        DependencyProvider = dependencyProvider ?? throw new ArgumentNullException(nameof(dependencyProvider));
        ArgumentNullException.ThrowIfNull(databaseObjects);

        var objects = databaseObjects.ToList();
        if (objects.Exists(static o => o.Value is null))
            throw new ArgumentNullException(nameof(databaseObjects), "A database object provided holds no object.");

        // Every view and routine resolves each of its dependencies against every object name, so the
        // links are built once up front. Adding the objects in order of precedence keeps each name's
        // links ordered by kind (table, view, sequence, synonym, routine, user-defined type), then by
        // position in the source list, as the sort is stable.
        var targets = new Dictionary<(string? Schema, string LocalName), List<ReferencedObject>>(SchemaLocalNameComparer.OrdinalIgnoreCase);
        var seenObjects = new HashSet<(int Precedence, Identifier Name)>();
        var objectsByPrecedence = objects
            .Select(static o => (Precedence: DatabaseObjectPrecedence.Of(o), Object: o))
            .OrderBy(static o => o.Precedence);
        foreach (var (precedence, databaseObject) in objectsByPrecedence)
        {
            var objectName = databaseObject.Name;
            if (!seenObjects.Add((precedence, objectName)))
                continue;

            var key = (objectName.Schema, objectName.LocalName);
            if (!targets.TryGetValue(key, out var links))
            {
                links = [];
                targets[key] = links;
            }

            links.Add(new ReferencedObject(objectName.ToVisibleName(), UrlRouter.GetUrl(databaseObject)));
        }

        Targets = targets;
    }

    private IDependencyProvider DependencyProvider { get; }

    private IReadOnlyDictionary<(string? Schema, string LocalName), List<ReferencedObject>> Targets { get; }

    /// <summary>
    /// Resolves the objects referenced by <paramref name="expression"/> to structured links
    /// (name + absolute hash route) for the JSON payload. An expression that cannot be read as SQL
    /// resolves to no links at all.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <see langword="null" />.</exception>
    public IReadOnlyCollection<ReferencedObject> GetReferencedObjects(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        if (expression.IsNullOrWhiteSpace())
            return [];

        IReadOnlyCollection<Identifier> referencedNames;
        try
        {
            referencedNames = DependencyProvider.GetDependencies(objectName, expression);
        }
        catch (ArgumentException)
        {
            // Not every definition is SQL the dialect can tokenize: a routine may be written in a
            // procedural language the database merely stores, or be stored obfuscated. Links to the
            // objects it mentions decorate the page, so an unreadable definition costs the page its
            // links rather than failing the report.
            return [];
        }

        if (referencedNames.Count == 0)
            return [];

        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ReferencedObject>();

        var orderedNames = referencedNames
            .OrderBy(static name => name.Schema ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static name => name.LocalName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var name in orderedNames)
        {
            // Targets are matched on schema and local name only, so an unqualified reference
            // only needs the referencing object's schema filled in.
            var schema = name.Schema ?? objectName.Schema;
            var isSelfReference = string.Equals(objectName.Schema, schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(objectName.LocalName, name.LocalName, StringComparison.OrdinalIgnoreCase);
            if (isSelfReference || !Targets.TryGetValue((schema, name.LocalName), out var targetLinks))
                continue;

            foreach (var link in targetLinks)
            {
                if (seenUrls.Add(link.Url))
                    result.Add(link);
            }
        }

        return result;
    }
}
