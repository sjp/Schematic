using System;
using System.Collections.Generic;
using System.Globalization;
using SJP.Schematic.Core;
using StringHashSet = System.Collections.Generic.HashSet<string>;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Decorates a name translator so that each database object is given a class name that is unique within its namespace.
/// </summary>
/// <remarks>
/// <para>
/// Name translation is lossy — punctuation is removed and casing is normalised — so distinct database objects can translate
/// to the same class name. Tables and views share a namespace as well, so they are drawn from a single pool of names.
/// Without deduplication the generated project would declare the same class twice, and one generated file would silently
/// overwrite the other.
/// </para>
/// <para>
/// A name is only assigned to an object once, so repeated translations of the same object, such as the references made by
/// a foreign key, always resolve to the same class name.
/// </para>
/// </remarks>
/// <seealso cref="INameTranslator" />
internal sealed class UniqueNameTranslator : INameTranslator
{
    private readonly INameTranslator _translator;

    // Tables and views are tracked separately because a table and a view are able to share a name.
    private readonly Dictionary<Identifier, string> _tableClassNames = [];
    private readonly Dictionary<Identifier, string> _viewClassNames = [];
    private readonly Dictionary<string, StringHashSet> _classNamesByNamespace = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueNameTranslator"/> class.
    /// </summary>
    /// <param name="translator">The name translator to draw candidate names from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="translator"/> is <see langword="null" />.</exception>
    public UniqueNameTranslator(INameTranslator translator)
    {
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    /// <summary>
    /// Assigns a class name to each of the given objects, in the order that they are provided.
    /// </summary>
    /// <param name="tableNames">The names of the tables in the database.</param>
    /// <param name="viewNames">The names of the views in the database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tableNames"/> or <paramref name="viewNames"/> is <see langword="null" />.</exception>
    /// <remarks>
    /// Deduplication favours whichever object is translated first, so reserving names up front keeps the unmodified names
    /// with the objects that the database lists first, rather than with whichever object generation happens to reach first.
    /// </remarks>
    public void ReserveClassNames(IEnumerable<Identifier> tableNames, IEnumerable<Identifier> viewNames)
    {
        ArgumentNullException.ThrowIfNull(tableNames);
        ArgumentNullException.ThrowIfNull(viewNames);

        foreach (var tableName in tableNames)
            TableToClassName(tableName);
        foreach (var viewName in viewNames)
            ViewToClassName(viewName);
    }

    /// <inheritdoc />
    public string? SchemaToNamespace(Identifier objectName) => _translator.SchemaToNamespace(objectName);

    /// <inheritdoc />
    public string ColumnToPropertyName(string className, string columnName) => _translator.ColumnToPropertyName(className, columnName);

    /// <inheritdoc />
    public string TableToClassName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetClassName(_tableClassNames, tableName, _translator.TableToClassName);
    }

    /// <inheritdoc />
    public string ViewToClassName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetClassName(_viewClassNames, viewName, _translator.ViewToClassName);
    }

    private string GetClassName(Dictionary<Identifier, string> assignedNames, Identifier objectName, Func<Identifier, string> translate)
    {
        // Only the schema and the local name determine the generated name, so an object referred to with a different
        // level of qualification, e.g. by a foreign key, must still resolve to the name already assigned to it.
        var key = Identifier.CreateQualifiedIdentifier(objectName.Schema, objectName.LocalName);
        if (assignedNames.TryGetValue(key, out var assignedName))
            return assignedName;

        var className = MakeUniqueWithinNamespace(objectName, translate(objectName));
        assignedNames.Add(key, className);

        return className;
    }

    private string MakeUniqueWithinNamespace(Identifier objectName, string candidateName)
    {
        var objectNamespace = _translator.SchemaToNamespace(objectName) ?? string.Empty;
        if (!_classNamesByNamespace.TryGetValue(objectNamespace, out var usedNames))
        {
            // Names are compared without case sensitivity because the file systems they are written to may be, too.
            usedNames = new StringHashSet(StringComparer.OrdinalIgnoreCase);
            _classNamesByNamespace.Add(objectNamespace, usedNames);
        }

        // Terminates because each iteration tries a name that has not been tried before, and only finitely many are in use.
        var uniqueName = candidateName;
        for (var suffix = 1; !usedNames.Add(uniqueName); suffix++)
            uniqueName = candidateName + "_" + suffix.ToString(CultureInfo.InvariantCulture);

        return uniqueName;
    }
}
