using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class RoutineModelMapper
{
    /// <summary>
    /// Maps a routine to its detail payload.
    /// </summary>
    /// <param name="routine">The routine to map.</param>
    /// <param name="referencedObjectTargets">Resolves the objects the routine's definition references to their pages.</param>
    /// <param name="userDefinedTypeTargets">Resolves a parameter or return type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null" />.</exception>
    public Routine Map(IDatabaseRoutine routine, ReferencedObjectTargets referencedObjectTargets, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(routine);
        ArgumentNullException.ThrowIfNull(referencedObjectTargets);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        // An overloaded routine's definition holds the definitions of every overload, so this
        // covers each of them without resolving them one at a time.
        var referencedObjects = referencedObjectTargets.GetReferencedObjects(routine.Name, routine.Definition);

        return new Routine(
            routine.Name,
            routine.Definition,
            routine.RoutineType,
            routine.Language,
            MapParameters(routine.Parameters, userDefinedTypeTargets),
            MapReturnType(routine.ReturnType),
            MapReturnTypeUrl(routine.ReturnType, userDefinedTypeTargets),
            routine.Overloads
                .Select(overload => new Routine.Overload(
                    overload.Definition,
                    MapParameters(overload.Parameters, userDefinedTypeTargets),
                    MapReturnType(overload.ReturnType),
                    MapReturnTypeUrl(overload.ReturnType, userDefinedTypeTargets)
                ))
                .ToList(),
            referencedObjects
        );
    }

    private static IEnumerable<Routine.Parameter> MapParameters(IEnumerable<IDatabaseRoutineParameter> parameters, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        return parameters
            .Select(parameter => new Routine.Parameter(
                parameter.Name,
                parameter.Type.Definition,
                userDefinedTypeTargets.GetTypeUrl(parameter.Type),
                parameter.Direction,
                parameter.DefaultValue,
                parameter.Ordinal
            ))
            .ToList();
    }

    private static Option<string> MapReturnType(Option<IDbType> returnType) => returnType.Map(static type => type.Definition);

    private static Option<Uri> MapReturnTypeUrl(Option<IDbType> returnType, UserDefinedTypeTargets userDefinedTypeTargets)
        => returnType.Bind(type => userDefinedTypeTargets.GetTypeUrl(type));
}
