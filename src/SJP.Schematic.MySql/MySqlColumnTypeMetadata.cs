using System;
using System.Collections.Generic;
using System.Text;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql;

/// <summary>
/// Builds column type metadata out of the type information the catalog reports for a column.
/// </summary>
/// <remarks>
/// <c>information_schema.columns.data_type</c> names only the type, so the members of an enum or a
/// set, whether an integer is unsigned, and the display width that distinguishes a boolean from a
/// one-byte integer are all read from <c>column_type</c>, which reports the type as declared.
/// </remarks>
internal static class MySqlColumnTypeMetadata
{
    private const string EnumTypeName = "enum";
    private const string SetTypeName = "set";
    private const string TinyIntTypeName = "tinyint";

    // MySQL has no boolean type: BOOL and BOOLEAN are synonyms for TINYINT(1), and the display
    // width is the only thing that distinguishes one from an ordinary one-byte integer
    private const string BooleanColumnTypePrefix = "tinyint(1)";

    private const string UnsignedSuffix = "unsigned";

    /// <summary>
    /// Describes a column's type, reading the members, sign and display width that only the declared type reports.
    /// </summary>
    /// <param name="dataTypeName">The name of the column's type, from <c>data_type</c>.</param>
    /// <param name="columnType">The column's type as declared, from <c>column_type</c>.</param>
    /// <param name="collation">The column's collation, if any.</param>
    /// <param name="maxLength">The column's maximum length.</param>
    /// <param name="numericPrecision">The column's numeric precision, if any.</param>
    /// <param name="fractionalSecondsPrecision">The column's fractional seconds precision, for a temporal type; otherwise none.</param>
    /// <returns>Column type metadata.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataTypeName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="dataTypeName"/> is empty or whitespace.</exception>
    public static ColumnTypeMetadata Create(
        string dataTypeName,
        string? columnType,
        Option<Identifier> collation,
        int maxLength,
        Option<INumericPrecision> numericPrecision,
        Option<int> fractionalSecondsPrecision
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataTypeName);

        var metadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(dataTypeName),
            Collation = collation,
            MaxLength = maxLength,
            NumericPrecision = numericPrecision,
            FractionalSecondsPrecision = fractionalSecondsPrecision,
            IsUnsigned = columnType?.Contains(UnsignedSuffix, StringComparison.OrdinalIgnoreCase) == true,
        };

        if (string.Equals(dataTypeName, EnumTypeName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(dataTypeName, SetTypeName, StringComparison.OrdinalIgnoreCase))
        {
            metadata.EnumValues = ParseMemberValues(columnType);
        }
        else if (string.Equals(dataTypeName, TinyIntTypeName, StringComparison.OrdinalIgnoreCase)
            && columnType?.StartsWith(BooleanColumnTypePrefix, StringComparison.OrdinalIgnoreCase) == true)
        {
            metadata.DataType = DataType.Boolean;
            metadata.ClrType = typeof(bool);
        }

        return metadata;
    }

    /// <summary>
    /// Reads the permitted values out of a declared enum or set type.
    /// </summary>
    /// <param name="columnType">A declared type, e.g. <c>enum('a','b')</c>.</param>
    /// <returns>The permitted values, unquoted and unescaped.</returns>
    /// <remarks>
    /// The members are printed as single-quoted string literals separated by commas, with an
    /// embedded quote doubled. A member may itself contain a comma or a parenthesis, so the values
    /// are read a character at a time rather than split apart.
    /// </remarks>
    public static IReadOnlyList<string> ParseMemberValues(string? columnType)
    {
        if (columnType.IsNullOrWhiteSpace())
            return [];

        var openingParen = columnType.IndexOf('(', StringComparison.Ordinal);
        var closingParen = columnType.LastIndexOf(')');
        if (openingParen < 0 || closingParen < openingParen)
            return [];

        var members = columnType.AsSpan(openingParen + 1, closingParen - openingParen - 1);
        var result = new List<string>();
        var current = new StringBuilder();
        var inValue = false;

        for (var i = 0; i < members.Length; i++)
        {
            var c = members[i];
            if (!inValue)
            {
                if (c == '\'')
                    inValue = true;
                continue;
            }

            if (c == '\'')
            {
                // a doubled quote is an escaped quote rather than the end of the value
                if (i + 1 < members.Length && members[i + 1] == '\'')
                {
                    current.Append('\'');
                    i++;
                    continue;
                }

                result.Add(current.ToString());
                current.Clear();
                inValue = false;
                continue;
            }

            // a backslash escape, which MySQL also prints, covers the character that follows it
            if (c == '\\' && i + 1 < members.Length)
            {
                current.Append(Unescape(members[i + 1]));
                i++;
                continue;
            }

            current.Append(c);
        }

        return result;
    }

    private static char Unescape(char escaped)
    {
        return escaped switch
        {
            '0' => '\0',
            'b' => '\b',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            'Z' => '\u001A',
            _ => escaped,
        };
    }
}
