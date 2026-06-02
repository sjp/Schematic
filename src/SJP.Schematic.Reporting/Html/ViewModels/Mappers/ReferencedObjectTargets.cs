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
        TableNames = tableNames ?? throw new ArgumentNullException(nameof(tableNames));
        ViewNames = viewNames ?? throw new ArgumentNullException(nameof(viewNames));
        SequenceNames = sequenceNames ?? throw new ArgumentNullException(nameof(sequenceNames));
        SynonymNames = synonymNames ?? throw new ArgumentNullException(nameof(synonymNames));
        RoutineNames = routineNames ?? throw new ArgumentNullException(nameof(routineNames));
    }

    private IDependencyProvider DependencyProvider { get; }

    private IEnumerable<Identifier> TableNames { get; }

    private IEnumerable<Identifier> ViewNames { get; }

    private IEnumerable<Identifier> SequenceNames { get; }

    private IEnumerable<Identifier> SynonymNames { get; }

    private IEnumerable<Identifier> RoutineNames { get; }

    /// <summary>
    /// Resolves the objects referenced by <paramref name="expression"/> to structured links
    /// (name + absolute hash route) for the JSON payload.
    /// </summary>
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
            var qualifiedName = QualifyReferenceName(objectName, name);
            var targetLinks = GetReferenceTargetLinks(objectName, qualifiedName);
            foreach (var link in targetLinks)
            {
                if (seenUrls.Add(link.Url))
                    result.Add(link);
            }
        }

        return result;
    }

    private IReadOnlyCollection<View.ReferencedObject> GetReferenceTargetLinks(Identifier objectName, Identifier referenceName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentNullException.ThrowIfNull(referenceName);

        var qualifiedReference = QualifyReferenceName(objectName, referenceName);
        var isSelfReference = string.Equals(objectName.Schema, qualifiedReference.Schema, StringComparison.OrdinalIgnoreCase)
            && string.Equals(objectName.LocalName, qualifiedReference.LocalName, StringComparison.OrdinalIgnoreCase);
        if (isSelfReference)
            return [];

        var result = new List<View.ReferencedObject>();

        result.AddRange(GetMatchingObjects(TableNames, qualifiedReference)
            .Select(static name => new View.ReferencedObject(name.ToVisibleName(), UrlRouter.GetTableUrl(name))));
        result.AddRange(GetMatchingObjects(ViewNames, qualifiedReference)
            .Select(static name => new View.ReferencedObject(name.ToVisibleName(), UrlRouter.GetViewUrl(name))));
        result.AddRange(GetMatchingObjects(SequenceNames, qualifiedReference)
            .Select(static name => new View.ReferencedObject(name.ToVisibleName(), UrlRouter.GetSequenceUrl(name))));
        result.AddRange(GetMatchingObjects(SynonymNames, qualifiedReference)
            .Select(static name => new View.ReferencedObject(name.ToVisibleName(), UrlRouter.GetSynonymUrl(name))));
        result.AddRange(GetMatchingObjects(RoutineNames, qualifiedReference)
            .Select(static name => new View.ReferencedObject(name.ToVisibleName(), UrlRouter.GetRoutineUrl(name))));

        return result;
    }

    private static IReadOnlyCollection<Identifier> GetMatchingObjects(IEnumerable<Identifier> objectNames, Identifier referenceName)
    {
        return objectNames
            .Where(name => string.Equals(name.Schema, referenceName.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(name.LocalName, referenceName.LocalName, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();
    }

    private static Identifier QualifyReferenceName(Identifier objectName, Identifier referenceName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentNullException.ThrowIfNull(referenceName);

        return Identifier.CreateQualifiedIdentifier(
            referenceName.Server ?? objectName.Server,
            referenceName.Database ?? objectName.Database,
            referenceName.Schema ?? objectName.Schema,
            referenceName.LocalName ?? objectName.LocalName
        );
    }
}
