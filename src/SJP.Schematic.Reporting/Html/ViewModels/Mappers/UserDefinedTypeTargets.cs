using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// Resolves the type of a column, parameter, sequence or attribute to the detail page of the
/// user-defined type it names, so that a type is a link wherever it is used and not only where it
/// is listed.
/// </summary>
internal sealed class UserDefinedTypeTargets
{
    // A type used by a column is named by the dialect with its schema and local name, while the
    // type is listed under whatever name its provider gives it, which may carry a database and
    // server too. Matching on schema and local name is what lets the two meet. It also keeps
    // built-in types out: they are named in the database's own schema (pg_catalog, sys), which no
    // user-defined type is declared in, so a user type sharing a built-in's local name never
    // steals its links and vice versa.
    private readonly Dictionary<(string? Schema, string LocalName), Uri> _exactNames = new(SchemaLocalNameComparer.Ordinal);
    private readonly Dictionary<(string? Schema, string LocalName), Uri> _namesIgnoringCase = new(SchemaLocalNameComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Builds the lookup from the types the report covers, so that a type is only ever linked when
    /// there is a page to link to.
    /// </summary>
    /// <param name="userDefinedTypeNames">The names of the user-defined types in the report.</param>
    /// <exception cref="ArgumentNullException"><paramref name="userDefinedTypeNames"/> is <see langword="null" />, or contains a <see langword="null" /> value.</exception>
    public UserDefinedTypeTargets(IEnumerable<Identifier> userDefinedTypeNames)
    {
        ArgumentNullException.ThrowIfNull(userDefinedTypeNames);

        foreach (var typeName in userDefinedTypeNames)
        {
            ArgumentNullException.ThrowIfNull(typeName);

            // The route is built from the type's own name rather than the spelling a column used,
            // because route keys are case-sensitive.
            var key = (typeName.Schema, typeName.LocalName);
            var url = new Uri(UrlRouter.GetUserDefinedTypeUrl(typeName), UriKind.Relative);
            _exactNames.TryAdd(key, url);
            _namesIgnoringCase.TryAdd(key, url);
        }
    }

    /// <summary>
    /// Resolves the type of a column, parameter, sequence or attribute to the route of the type's
    /// own page.
    /// </summary>
    /// <param name="type">The declared type.</param>
    /// <returns>The type's route, or none when the type is not one of the report's user-defined
    /// types. A collection type resolves to its element type's page when the collection type
    /// itself has no page, so that an array of an enum links to the enum.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null" />.</exception>
    public Option<Uri> GetTypeUrl(IDbType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Element types nest no more than a couple of levels deep in practice; the bound is what
        // stops a model whose type claims itself as its own element type from hanging the report.
        const int maxElementTypeDepth = 8;

        var current = type;
        for (var depth = 0; depth < maxElementTypeDepth; depth++)
        {
            if (TryResolve(current.TypeName, out var url))
                return url;

            var elementType = current.ElementType.MatchUnsafe(static t => t, static () => (IDbType?)null);
            if (elementType == null)
                break;

            current = elementType;
        }

        return Option<Uri>.None;
    }

    /// <summary>
    /// Resolves a type name to the route of the type's own page.
    /// </summary>
    /// <param name="typeName">The name of a type.</param>
    /// <returns>The type's route, or none when the name is not one of the report's user-defined
    /// types. A name that carries no schema only matches a type that carries none either, so a
    /// dialect that does not report the schema a type is declared in yields no link rather than a
    /// link to whichever type happens to share the name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public Option<Uri> GetTypeUrl(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return TryResolve(typeName, out var url) ? url : Option<Uri>.None;
    }

    // An exact match wins, so types whose names differ only in casing each keep their own page.
    private bool TryResolve(Identifier typeName, [NotNullWhen(true)] out Uri? url)
    {
        var key = (typeName.Schema, typeName.LocalName);
        return _exactNames.TryGetValue(key, out url)
            || _namesIgnoringCase.TryGetValue(key, out url);
    }
}
