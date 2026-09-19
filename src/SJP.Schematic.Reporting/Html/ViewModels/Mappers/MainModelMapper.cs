using System;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class MainModelMapper
{
    public Main.Table Map(IRelationalDatabaseTable table, Option<ITableStatistics> statistics)
    {
        ArgumentNullException.ThrowIfNull(table);

        var parentKeyCount = table.ParentKeys.UCount();
        var childKeyCount = table.ChildKeys.UCount();
        var columnCount = table.Columns.UCount();
        var rowCount = statistics.Bind(static stat => stat.RowCount);

        return new Main.Table(
            table.Name,
            parentKeyCount,
            childKeyCount,
            columnCount,
            table.Kind,
            rowCount
        );
    }

    public Main.View Map(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var columnCount = view.Columns.UCount();
        return new Main.View(view.Name, columnCount, view.IsMaterialized);
    }

    /// <summary>
    /// Maps a sequence to a row of the sequences list.
    /// </summary>
    /// <param name="sequence">The sequence to map.</param>
    /// <param name="userDefinedTypeTargets">Resolves the sequence's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sequence"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public Main.Sequence Map(IDatabaseSequence sequence, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        return new Main.Sequence(
            sequence.Name,
            sequence.Type.Definition,
            userDefinedTypeTargets.GetTypeUrl(sequence.Type),
            sequence.Start,
            sequence.Increment,
            sequence.MinValue,
            sequence.MaxValue,
            sequence.CacheMode,
            sequence.CacheSize,
            sequence.Cycle,
            sequence.IsOrdered
        );
    }

    public Main.Synonym Map(IDatabaseSynonym synonym, SynonymTargets targets)
    {
        ArgumentNullException.ThrowIfNull(synonym);
        ArgumentNullException.ThrowIfNull(targets);

        var targetUrl = targets.GetTargetUrl(synonym.Target);
        return new Main.Synonym(synonym.Name, synonym.Target, targetUrl);
    }

    public Main.Routine Map(IDatabaseRoutine routine)
    {
        ArgumentNullException.ThrowIfNull(routine);

        return new Main.Routine(routine.Name, routine.RoutineType);
    }

    /// <summary>
    /// Maps a user-defined type to a row of the types list.
    /// </summary>
    /// <param name="userDefinedType">The type to map.</param>
    /// <param name="userDefinedTypeTargets">Resolves the type's base type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="userDefinedType"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public Main.UserDefinedType Map(IDatabaseUserDefinedType userDefinedType, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(userDefinedType);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        return new Main.UserDefinedType(
            userDefinedType.Name,
            userDefinedType.Kind,
            userDefinedType.BaseType,
            userDefinedType.BaseType.Bind(baseType => userDefinedTypeTargets.GetTypeUrl(baseType)),
            userDefinedType.IsNullable,
            (uint)userDefinedType.Attributes.Count,
            (uint)userDefinedType.EnumValues.Count
        );
    }
}