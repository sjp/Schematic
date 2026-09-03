using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes a foreign key relationship.
/// </summary>
/// <seealso cref="IDatabaseRelationalKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseRelationalKey : IDatabaseRelationalKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRelationalKey"/> class.
    /// </summary>
    /// <param name="childTableName">The child table name.</param>
    /// <param name="childKey">The child key.</param>
    /// <param name="parentTableName">The parent table name.</param>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="deleteAction">The delete action.</param>
    /// <param name="updateAction">The update action.</param>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/>, <paramref name="childKey"/>, <paramref name="parentTableName"/> or <paramref name="parentKey"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="deleteAction"/> or <paramref name="updateAction"/> is not a valid enum, <paramref name="childKey"/> is not a foreign key, <paramref name="parentKey"/> is not a primary or unique key, or <paramref name="childKey"/> and <paramref name="parentKey"/> do not have the same number of columns.</exception>
    public DatabaseRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, ReferentialAction deleteAction, ReferentialAction updateAction)
        : this(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction, ForeignKeyMatchType.Simple, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRelationalKey"/> class.
    /// </summary>
    /// <param name="childTableName">The child table name.</param>
    /// <param name="childKey">The child key.</param>
    /// <param name="parentTableName">The parent table name.</param>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="deleteAction">The delete action.</param>
    /// <param name="updateAction">The update action.</param>
    /// <param name="matchType">How partially <c>null</c> child rows are matched against the parent key.</param>
    /// <param name="setNullColumns">The child key columns set to <c>null</c> when <paramref name="deleteAction"/> is <see cref="ReferentialAction.SetNull"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/>, <paramref name="childKey"/>, <paramref name="parentTableName"/>, <paramref name="parentKey"/> or <paramref name="setNullColumns"/> is <see langword="null" />, or <paramref name="setNullColumns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="deleteAction"/>, <paramref name="updateAction"/> or <paramref name="matchType"/> is not a valid enum, <paramref name="childKey"/> is not a foreign key, <paramref name="parentKey"/> is not a primary or unique key, or <paramref name="childKey"/> and <paramref name="parentKey"/> do not have the same number of columns.</exception>
    public DatabaseRelationalKey(
        Identifier childTableName,
        IDatabaseKey childKey,
        Identifier parentTableName,
        IDatabaseKey parentKey,
        ReferentialAction deleteAction,
        ReferentialAction updateAction,
        ForeignKeyMatchType matchType,
        IReadOnlyCollection<IDatabaseColumn> setNullColumns
    )
    {
        if (!deleteAction.IsValid())
            throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));
        if (!updateAction.IsValid())
            throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(updateAction));
        if (!matchType.IsValid())
            throw new ArgumentException($"The {nameof(ForeignKeyMatchType)} provided must be a valid enum.", nameof(matchType));

        ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
        ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
        ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
        ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

        if (ChildKey.KeyType != DatabaseKeyType.Foreign)
            throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{childKey.KeyType}'.", nameof(childKey));
        if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
            throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{parentKey.KeyType}'.", nameof(parentKey));

        if (ChildKey.Columns.Count != ParentKey.Columns.Count)
        {
            var childKeyName = ChildKey.Name.Match(static name => " '" + name.LocalName + "'", static () => string.Empty);
            var parentKeyName = ParentKey.Name.Match(static name => " '" + name.LocalName + "'", static () => string.Empty);

            throw new ArgumentException($"The child and parent key column counts must match. The child key{childKeyName} has {ChildKey.Columns.Count} column(s), while the parent key{parentKeyName} has {ParentKey.Columns.Count} column(s).", nameof(childKey));
        }

        DeleteAction = deleteAction;
        UpdateAction = updateAction;
        MatchType = matchType;
        SetNullColumns = setNullColumns.ToDefensiveCopy(nameof(setNullColumns));
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
    public IDatabaseKey ChildKey { get; }

    /// <summary>
    /// The parent table name.
    /// </summary>
    /// <value>A table name.</value>
    public Identifier ParentTable { get; }

    /// <summary>
    /// The primary or unique key being referred to in the relationship.
    /// </summary>
    /// <value>The parent primary or unique key.</value>
    public IDatabaseKey ParentKey { get; }

    /// <summary>
    /// The action to perform if the parent key's value is deleted.
    /// </summary>
    /// <value>The delete action.</value>
    public ReferentialAction DeleteAction { get; }

    /// <summary>
    /// The action to perform if the parent key's value is updated.
    /// </summary>
    /// <value>The update action.</value>
    public ReferentialAction UpdateAction { get; }

    /// <summary>
    /// Describes how the relationship treats child rows whose key columns are only partially <c>null</c>.
    /// </summary>
    /// <value>A foreign key match type.</value>
    public ForeignKeyMatchType MatchType { get; }

    /// <summary>
    /// The child key columns set to <c>null</c> when <see cref="DeleteAction"/> is <see cref="ReferentialAction.SetNull"/>.
    /// </summary>
    /// <value>A collection of database columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> SetNullColumns { get; }

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