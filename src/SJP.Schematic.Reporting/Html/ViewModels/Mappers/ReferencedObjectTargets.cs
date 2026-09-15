using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ReferencedObjectTargets
{
    public ReferencedObjectTargets(
        IDependencyProvider dependencyProvider,
        IEnumerable<Identifier> tableNames,
        IEnumerable<Identifier> viewNames,
        IEnumerable<Identifier> sequenceNames,
        IEnumerable<Identifier> synonymNames,
        IEnumerable<Identifier> routineNames
    )
    {
        DependencyProvider = dependencyProvider ?? throw new ArgumentNullException(nameof(dependencyProvider));
        ArgumentNullException.ThrowIfNull(tableNames);
        ArgumentNullException.ThrowIfNull(viewNames);
        ArgumentNullException.ThrowIfNull(sequenceNames);
        ArgumentNullException.ThrowIfNull(synonymNames);
        ArgumentNullException.ThrowIfNull(routineNames);

        // Every view resolves each of its dependencies against every object name, so the links are
        // built once up front. Adding the kinds in a fixed order keeps each name's links ordered by
        // kind (table, view, sequence, synonym, routine), then by position in the source list.
        var targets = new Dictionary<(string? Schema, string LocalName), List<View.ReferencedObject>>(SchemaLocalNameComparer.Instance);
        AddTargets(targets, tableNames, UrlRouter.GetTableUrl);
        AddTargets(targets, viewNames, UrlRouter.GetViewUrl);
        AddTargets(targets, sequenceNames, UrlRouter.GetSequenceUrl);
        AddTargets(targets, synonymNames, UrlRouter.GetSynonymUrl);
        AddTargets(targets, routineNames, UrlRouter.GetRoutineUrl);
        Targets = targets;
    }

    private IDependencyProvider DependencyProvider { get; }

    private IReadOnlyDictionary<(string? Schema, string LocalName), List<View.ReferencedObject>> Targets { get; }

    /// <summary>
    /// Resolves the objects referenced by <paramref name="expression"/> to structured links
    /// (name + absolute hash route) for the JSON payload.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <see langword="null" />.</exception>
    public IReadOnlyCollection<View.ReferencedObject> GetReferencedObjects(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        if (expression.IsNullOrWhiteSpace())
            return [];

        var referencedNames = DependencyProvider.GetDependencies(objectName, expression);
        if (referencedNames.Count == 0)
            return [];

        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<View.ReferencedObject>();

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

    private static void AddTargets(
        Dictionary<(string? Schema, string LocalName), List<View.ReferencedObject>> targets,
        IEnumerable<Identifier> objectNames,
        Func<Identifier, string> urlFactory)
    {
        var seenNames = new HashSet<Identifier>();
        foreach (var objectName in objectNames)
        {
            ArgumentNullException.ThrowIfNull(objectName);
            if (!seenNames.Add(objectName))
                continue;

            var key = (objectName.Schema, objectName.LocalName);
            if (!targets.TryGetValue(key, out var links))
            {
                links = [];
                targets[key] = links;
            }

            links.Add(new View.ReferencedObject(objectName.ToVisibleName(), urlFactory(objectName)));
        }
    }

    private sealed class SchemaLocalNameComparer : IEqualityComparer<(string? Schema, string LocalName)>
    {
        public static SchemaLocalNameComparer Instance { get; } = new();

        public bool Equals((string? Schema, string LocalName) x, (string? Schema, string LocalName) y)
        {
            return string.Equals(x.Schema, y.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.LocalName, y.LocalName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((string? Schema, string LocalName) obj)
        {
            var schemaHash = obj.Schema is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Schema);
            return HashCode.Combine(schemaHash, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.LocalName));
        }
    }
}
