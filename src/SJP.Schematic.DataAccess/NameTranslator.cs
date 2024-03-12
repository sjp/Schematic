﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// A set of rules for determining the class and property names for a database mapping object.
/// </summary>
public abstract class NameTranslator : INameTranslator
{
    /// <summary>
    /// Return a namespace name for a schema qualified object name.
    /// </summary>
    /// <param name="objectName">An optionally qualified object name.</param>
    /// <returns><c>null</c> if <paramref name="objectName"/> does not contain a schema name or should not be used.</returns>
    public abstract string? SchemaToNamespace(Identifier objectName);

    /// <summary>
    /// Return a name for a table.
    /// </summary>
    /// <param name="tableName">An optionally qualified table name.</param>
    /// <returns>A class name.</returns>
    public abstract string TableToClassName(Identifier tableName);

    /// <summary>
    /// Return a name for a view.
    /// </summary>
    /// <param name="viewName">An optionally qualified view name.</param>
    /// <returns>A class name.</returns>
    public abstract string ViewToClassName(Identifier viewName);

    /// <summary>
    /// Return a property name for a column.
    /// </summary>
    /// <param name="className">The name of the class the column is a member of.</param>
    /// <param name="columnName">A column name.</param>
    /// <returns>A property name.</returns>
    public abstract string ColumnToPropertyName(string className, string columnName);

    /// <summary>
    /// Determines whether a given string is a valid identifier.
    /// </summary>
    /// <param name="identifier">A string to test.</param>
    /// <returns><c>true</c> if <paramref name="identifier"/> is a valid identifier; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>, empty or whitespace</exception>
    protected static bool IsValidIdentifier(string identifier)
    {
        if (identifier.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(identifier));

        if (Keywords.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            return false;

        var firstChar = identifier[0];
        var isValidFirstChar = firstChar == '_' || firstChar.IsLetter();
        if (!isValidFirstChar)
            return false;

        var restChars = identifier.Skip(1).ToList();
        if (restChars.Empty())
            return true;

        return restChars
            .Select(static c => c.GetUnicodeCategory())
            .All(ValidPartCategories.Contains);
    }

    /// <summary>
    /// Creates a valid identifier from a given object name.
    /// </summary>
    /// <param name="objectName">An object name.</param>
    /// <returns>A valid identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <c>null</c>, empty or whitespace</exception>
    protected static string CreateValidIdentifier(string objectName)
    {
        if (objectName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(objectName));

        var firstChar = objectName[0];
        var isValidFirstChar = firstChar == '_' || firstChar.IsLetter();
        if (!isValidFirstChar)
            objectName = firstChar.ToString() + objectName;

        var chars = objectName
            .Select(static c => new { NameChar = c, CharCategory = c.GetUnicodeCategory() })
            .Where(static cc => ValidPartCategories.Contains(cc.CharCategory))
            .Select(static cc => cc.NameChar);

        return new string(chars.ToArray());
    }

    /// <summary>
    /// Creates a valid identifier from a given class name and column name.
    /// </summary>
    /// <param name="className">The name of the containing class.</param>
    /// <param name="columnName">The name of the column the property maps to.</param>
    /// <returns>A valid identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="className"/> or <paramref name="columnName"/> are <c>null</c>, empty or whitespace.
    /// </exception>
    protected static string CreateValidIdentifier(string className, string columnName)
    {
        if (className.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(className));
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        if (string.Equals(columnName, className, StringComparison.Ordinal))
            return columnName + "_";

        var firstChar = columnName[0];
        var isValidFirstChar = firstChar == '_' || firstChar.IsLetter();
        if (!isValidFirstChar)
            columnName = firstChar.ToString() + columnName;

        var chars = columnName
            .Select(c => new { NameChar = c, CharCategory = c.GetUnicodeCategory() })
            .Where(cc => ValidPartCategories.Contains(cc.CharCategory))
            .Select(cc => cc.NameChar);

        return new string(chars.ToArray());
    }

    private static readonly IEnumerable<UnicodeCategory> ValidPartCategories =
    [
        // letter character
        UnicodeCategory.UppercaseLetter,
        UnicodeCategory.LowercaseLetter,
        UnicodeCategory.TitlecaseLetter,
        UnicodeCategory.ModifierLetter,
        UnicodeCategory.OtherLetter,
        UnicodeCategory.LetterNumber,

        // combining character
        UnicodeCategory.NonSpacingMark,
        UnicodeCategory.SpacingCombiningMark,

        // decimal digit character
        UnicodeCategory.DecimalDigitNumber,

        // connecting character
        UnicodeCategory.ConnectorPunctuation,

        // formatting character
        UnicodeCategory.Format
    ];

    private static readonly IEnumerable<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "static",
        "virtual",
        "void",
        "volatile",
        "while"
    };
}