using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class RoutineModelMapper
{
    public Routine Map(IDatabaseRoutine routine)
    {
        ArgumentNullException.ThrowIfNull(routine);

        return new Routine(
            routine.Name,
            routine.Definition,
            routine.RoutineType,
            routine.Language,
            MapParameters(routine.Parameters),
            MapReturnType(routine.ReturnType),
            routine.Overloads
                .Select(static overload => new Routine.Overload(
                    overload.Definition,
                    MapParameters(overload.Parameters),
                    MapReturnType(overload.ReturnType)
                ))
                .ToList()
        );
    }

    private static IEnumerable<Routine.Parameter> MapParameters(IEnumerable<IDatabaseRoutineParameter> parameters)
    {
        return parameters
            .Select(static parameter => new Routine.Parameter(
                parameter.Name,
                parameter.Type.Definition,
                parameter.Direction,
                parameter.DefaultValue,
                parameter.Ordinal
            ))
            .ToList();
    }

    private static Option<string> MapReturnType(Option<IDbType> returnType) => returnType.Map(static type => type.Definition);
}
