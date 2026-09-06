using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-type detail payload (<c>data/userDefinedTypes/&lt;safeKey&gt;.json</c>): what the type
/// is built from — its base type, permitted values, attributes and constraints.
/// </summary>
public sealed class UserDefinedType
{
    public UserDefinedType(
        Identifier typeName,
        UserDefinedTypeKind kind,
        Option<IDbType> baseType,
        bool isNullable,
        Option<string> defaultValue,
        Option<string> definition,
        IEnumerable<string> enumValues,
        IEnumerable<Attribute> attributes,
        IEnumerable<Check> checks
    )
    {
        ArgumentNullException.ThrowIfNull(typeName);

        Name = typeName.ToVisibleName();
        TypeUrl = UrlRouter.GetUserDefinedTypeUrl(typeName);

        Kind = UserDefinedTypeKindNames.GetName(kind);
        BaseType = baseType.Match(static t => t.Definition, static () => string.Empty);
        IsNullable = isNullable;
        DefaultValue = defaultValue.Match(static d => d ?? string.Empty, static () => string.Empty);
        Definition = definition.Match(static d => d ?? string.Empty, static () => string.Empty);

        EnumValues = enumValues ?? throw new ArgumentNullException(nameof(enumValues));
        EnumValuesCount = enumValues.UCount();

        Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        AttributesCount = attributes.UCount();

        Checks = checks ?? throw new ArgumentNullException(nameof(checks));
        ChecksCount = checks.UCount();
    }

    public string Name { get; }

    public string TypeUrl { get; }

    /// <summary>Display name of the kind of type, e.g. <c>Domain</c>. Empty when unknown.</summary>
    public string Kind { get; }

    /// <summary>
    /// The type this type is defined in terms of. Empty when the type is not defined in terms of
    /// another one, or the database does not report it.
    /// </summary>
    public string BaseType { get; }

    public bool IsNullable { get; }

    /// <summary>The default for a column of this type. Empty when the type declares none.</summary>
    public string DefaultValue { get; }

    /// <summary>The textual definition of the type. Empty when the database reports none.</summary>
    public string Definition { get; }

    public IEnumerable<string> EnumValues { get; }

    public uint EnumValuesCount { get; }

    public IEnumerable<Attribute> Attributes { get; }

    public uint AttributesCount { get; }

    public IEnumerable<Check> Checks { get; }

    public uint ChecksCount { get; }

    /// <summary>
    /// A named attribute of a composite or table type. Named distinctly from <see cref="Table.Column"/>
    /// so the JSON source generator emits non-colliding metadata.
    /// </summary>
    public sealed class Attribute
    {
        public Attribute(
            string attributeName,
            int ordinal,
            bool isNullable,
            string typeDefinition,
            Option<string> defaultValue
        )
        {
            AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
            Ordinal = ordinal;
            IsNullable = isNullable;
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);
        }

        public int Ordinal { get; }

        public string AttributeName { get; }

        public bool IsNullable { get; }

        public string Type { get; }

        public string DefaultValue { get; }
    }

    /// <summary>
    /// A check constraint a value of the type must satisfy. Named distinctly from
    /// <see cref="Table.CheckConstraint"/> so the JSON source generator emits non-colliding metadata.
    /// </summary>
    public sealed class Check
    {
        public Check(Option<Identifier> constraintName, string definition)
        {
            ConstraintName = constraintName.Match(static name => name.LocalName, static () => string.Empty);
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>The constraint's name. Empty when the constraint is unnamed.</summary>
        public string ConstraintName { get; }

        public string Definition { get; }
    }
}
