using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-routine detail payload (<c>data/routines/&lt;safeKey&gt;.json</c>): the routine's
/// name, kind, signature and definition, and links to the objects the definition references.
/// </summary>
public sealed class Routine
{
    public Routine(
        Identifier routine,
        string definition,
        RoutineType routineType,
        Option<string> language,
        IEnumerable<Parameter> parameters,
        Option<string> returnType,
        Option<Uri> returnTypeUrl,
        IEnumerable<Overload> overloads,
        IEnumerable<ReferencedObject> referencedObjects
    )
    {
        ArgumentNullException.ThrowIfNull(routine);
        ArgumentNullException.ThrowIfNull(referencedObjects);
        if (!routineType.IsValid())
            throw new ArgumentException($"The {nameof(RoutineType)} provided must be a valid enum.", nameof(routineType));

        Name = routine.ToVisibleName();
        RoutineUrl = UrlRouter.GetRoutineUrl(routine);
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        RoutineType = routineType.ToString();
        Language = language.MatchUnsafe(static l => l, static () => (string?)null);

        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ParametersCount = parameters.UCount();

        ReturnType = returnType.MatchUnsafe(static t => t, static () => (string?)null);
        ReturnTypeUrl = returnTypeUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);

        Overloads = overloads ?? throw new ArgumentNullException(nameof(overloads));
        OverloadsCount = overloads.UCount();

        ReferencedObjects = referencedObjects;
        ReferencedObjectsCount = referencedObjects.UCount();
    }

    public string Name { get; }

    public string RoutineUrl { get; }

    public string Definition { get; }

    public string RoutineType { get; }

    public string? Language { get; }

    public IEnumerable<Parameter> Parameters { get; }

    public uint ParametersCount { get; }

    public string? ReturnType { get; }

    /// <summary>
    /// The hash route of the user-defined type the routine returns. Omitted from the JSON when the
    /// routine returns nothing, or returns a type that is not one of the report's user-defined types.
    /// </summary>
    public string? ReturnTypeUrl { get; }

    public IEnumerable<Overload> Overloads { get; }

    public uint OverloadsCount { get; }

    public IEnumerable<ReferencedObject> ReferencedObjects { get; }

    public uint ReferencedObjectsCount { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Parameter
    {
        public Parameter(Option<Identifier> parameterName, string typeDefinition, Option<Uri> typeUrl, RoutineParameterDirection direction, Option<string> defaultValue, int ordinal)
        {
            if (!direction.IsValid())
                throw new ArgumentException($"The {nameof(RoutineParameterDirection)} provided must be a valid enum.", nameof(direction));

            ParameterName = parameterName.MatchUnsafe(static name => name.LocalName, static () => (string?)null);
            Type = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            TypeUrl = typeUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
            Direction = direction.ToString();
            DefaultValue = defaultValue.MatchUnsafe(static value => value, static () => (string?)null);
            Ordinal = ordinal;
        }

        public string? ParameterName { get; }

        public string Type { get; }

        /// <summary>
        /// The hash route of the user-defined type the parameter is declared with. Omitted from the
        /// JSON when the parameter's type is not one of the report's user-defined types.
        /// </summary>
        public string? TypeUrl { get; }

        public string Direction { get; }

        public string? DefaultValue { get; }

        public int Ordinal { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Overload
    {
        public Overload(string definition, IEnumerable<Parameter> parameters, Option<string> returnType, Option<Uri> returnTypeUrl)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnType = returnType.MatchUnsafe(static t => t, static () => (string?)null);
            ReturnTypeUrl = returnTypeUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
        }

        public string Definition { get; }

        public IEnumerable<Parameter> Parameters { get; }

        public string? ReturnType { get; }

        /// <summary>
        /// The hash route of the user-defined type this signature returns. Omitted from the JSON
        /// when it returns nothing, or a type that is not one of the report's user-defined types.
        /// </summary>
        public string? ReturnTypeUrl { get; }
    }
}
