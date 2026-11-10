using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Describes a foreign key relationship in Oracle databases.
/// </summary>
/// <seealso cref="IDatabaseRelationalKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class OracleRelationalKey : IDatabaseRelationalKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleRelationalKey"/> class.
    /// </summary>
    /// <param name="childTableName">The child table name.</param>
    /// <param name="childKey">The child key.</param>
    /// <param name="parentTableName">The parent table name.</param>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="deleteAction">The delete action.</param>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/>, <paramref name="childKey"/> or <paramref name="parentTableName"/> is <see langword="null" />, or <paramref name="parentKey"/> holds no key.</exception>
    /// <exception cref="ArgumentException"><paramref name="deleteAction"/> is not a valid enum.</exception>
    public OracleRelationalKey(Identifier childTableName, DatabaseKey.Foreign childKey, Identifier parentTableName, CandidateKey parentKey, ReferentialAction deleteAction)
    {
        if (!deleteAction.IsValid())
            throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));

        ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
        ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
        ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
        ParentKey = parentKey.Value is not null ? parentKey : throw new ArgumentNullException(nameof(parentKey));

        DeleteAction = deleteAction;
    }

    /// <summary>
    /// The child table name.
    /// </summary>
    /// <value>A table name.</value>
    public Identifier ChildTable { get; }

    /// <summary>
    /// The foreign key defined in the child table.
    /// </summary>
    /// <value>The child foreign key.</value>
    public DatabaseKey.Foreign ChildKey { get; }

    /// <summary>
    /// The parent table name.
    /// </summary>
    /// <value>A table name.</value>
    public Identifier ParentTable { get; }

    /// <summary>
    /// The primary or unique key being referred to in the relationship.
    /// </summary>
    /// <value>The parent primary or unique key.</value>
    public CandidateKey ParentKey { get; }

    /// <summary>
    /// The action to perform if the parent key's value is deleted.
    /// </summary>
    /// <value>The delete action.</value>
    public ReferentialAction DeleteAction { get; }

    /// <summary>
    /// The action to perform if the parent key's value is updated. Always <see cref="ReferentialAction.NoAction"/>.
    /// </summary>
    /// <value>Always <see cref="ReferentialAction.NoAction"/>.</value>
    public ReferentialAction UpdateAction { get; }

    /// <summary>
    /// Describes how the relationship treats child rows whose key columns are only partially
    /// <c>null</c>. Always <see cref="ForeignKeyMatchType.Simple"/>; Oracle implements no other behaviour.
    /// </summary>
    /// <value>Always <see cref="ForeignKeyMatchType.Simple"/>.</value>
    public ForeignKeyMatchType MatchType { get; } = ForeignKeyMatchType.Simple;

    /// <summary>
    /// The child key columns set to <c>null</c> by an <c>ON DELETE SET NULL</c> action. Always empty;
    /// Oracle sets every child key column.
    /// </summary>
    /// <value>An empty collection.</value>
    public IReadOnlyCollection<IDatabaseColumn> SetNullColumns { get; } = [];

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Relational Key: ");

            if (!ChildTable.Schema.IsNullOrWhiteSpace())
                builder.Append(ChildTable.Schema).Append('.');

            builder.Append(ChildTable.LocalName);

            ChildKey.Name.IfSome(name =>
            {
                builder.Append(" (")
                    .Append(name.LocalName)
                    .Append(')');
            });

            builder.Append(" -> ");

            if (!ParentTable.Schema.IsNullOrWhiteSpace())
                builder.Append(ParentTable.Schema).Append('.');

            builder.Append(ParentTable.LocalName);

            ParentKey.Name.IfSome(name =>
            {
                builder.Append(" (")
                    .Append(name.LocalName)
                    .Append(')');
            });

            return builder.GetStringAndRelease();
        }
    }
}