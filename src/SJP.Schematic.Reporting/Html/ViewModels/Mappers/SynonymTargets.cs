using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SynonymTargets
{
    public SynonymTargets(
        IEnumerable<Identifier> tableNames,
        IEnumerable<Identifier> viewNames,
        IEnumerable<Identifier> sequenceNames,
        IEnumerable<Identifier> synonymNames,
        IEnumerable<Identifier> routineNames,
        IEnumerable<Identifier> userDefinedTypeNames
    )
    {
        ArgumentNullException.ThrowIfNull(tableNames);
        ArgumentNullException.ThrowIfNull(viewNames);
        ArgumentNullException.ThrowIfNull(sequenceNames);
        ArgumentNullException.ThrowIfNull(synonymNames);
        ArgumentNullException.ThrowIfNull(routineNames);
        ArgumentNullException.ThrowIfNull(userDefinedTypeNames);

        TableNames = new ObjectNames(tableNames);
        ViewNames = new ObjectNames(viewNames);
        SequenceNames = new ObjectNames(sequenceNames);
        SynonymNames = new ObjectNames(synonymNames);
        RoutineNames = new ObjectNames(routineNames);
        UserDefinedTypeNames = new ObjectNames(userDefinedTypeNames);
    }

    private ObjectNames TableNames { get; }

    private ObjectNames ViewNames { get; }

    private ObjectNames SequenceNames { get; }

    private ObjectNames SynonymNames { get; }

    private ObjectNames RoutineNames { get; }

    private ObjectNames UserDefinedTypeNames { get; }

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

        if (TableNames.TryResolve(targetName, out var objectName))
            return new Uri(UrlRouter.GetTableUrl(objectName), UriKind.Relative);

        if (ViewNames.TryResolve(targetName, out objectName))
            return new Uri(UrlRouter.GetViewUrl(objectName), UriKind.Relative);

        if (SequenceNames.TryResolve(targetName, out objectName))
            return new Uri(UrlRouter.GetSequenceUrl(objectName), UriKind.Relative);

        if (SynonymNames.TryResolve(targetName, out objectName))
            return new Uri(UrlRouter.GetSynonymUrl(objectName), UriKind.Relative);

        if (RoutineNames.TryResolve(targetName, out objectName))
            return new Uri(UrlRouter.GetRoutineUrl(objectName), UriKind.Relative);

        if (UserDefinedTypeNames.TryResolve(targetName, out objectName))
            return new Uri(UrlRouter.GetUserDefinedTypeUrl(objectName), UriKind.Relative);

        return LanguageExt.Option<Uri>.None;
    }

    // Target names are matched case-insensitively, so a synonym whose target differs only in
    // casing still resolves. The route is built from the object's own name rather than the
    // target's spelling, because route keys are case-sensitive. An exact match wins, so objects
    // whose names differ only in casing each keep their own route.
    private sealed class ObjectNames
    {
        public ObjectNames(IEnumerable<Identifier> names)
        {
            foreach (var name in names)
            {
                ExactNames.Add(name);
                NamesIgnoringCase.TryAdd(name, name);
            }
        }

        private HashSet<Identifier> ExactNames { get; } = [];

        private Dictionary<Identifier, Identifier> NamesIgnoringCase { get; } = new(IdentifierComparer.OrdinalIgnoreCase);

        public bool TryResolve(Identifier targetName, [NotNullWhen(true)] out Identifier? objectName)
        {
            if (ExactNames.Contains(targetName))
            {
                objectName = targetName;
                return true;
            }

            return NamesIgnoringCase.TryGetValue(targetName, out objectName);
        }
    }
}
