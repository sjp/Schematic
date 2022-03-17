using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A MySQL relational key definition.
/// </summary>
/// <seealso cref="IDatabaseRelationalKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class MySqlRelationalKey : IDatabaseRelationalKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlRelationalKey"/> class.
    /// </summary>
    /// <param name="childTableName">Name of the child table.</param>
    /// <param name="childKey">The child key.</param>
    /// <param name="parentTableName">Name of the parent table.</param>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="deleteAction">The delete action.</param>
    /// <param name="updateAction">The update action.</param>
    /// <exception cref="ArgumentException"><paramref name="deleteAction"/> or <paramref name="updateAction"/> are invalid enums or have invalid values.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="childTableName"/> or <paramref name="childKey"/> or <paramref name="parentTableName"/> or <paramref name="parentKey"/> are <c>null</c></exception>
    public MySqlRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, ReferentialAction deleteAction, ReferentialAction updateAction)
    {
        if (!deleteAction.IsValid())
            throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(deleteAction));
        if (!updateAction.IsValid())
            throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(updateAction));
        if (deleteAction == ReferentialAction.SetDefault)
            throw new ArgumentException("MySQL does not support a delete action of 'SET DEFAULT'.", nameof(deleteAction));
        if (updateAction == ReferentialAction.SetDefault)
            throw new ArgumentException("MySQL does not support an update action of 'SET DEFAULT'.", nameof(updateAction));

        ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
        ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
        ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
        ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

        if (ChildKey.KeyType != DatabaseKeyType.Foreign)
            throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{ childKey.KeyType }'.", nameof(childKey));
        if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
            throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{ parentKey.KeyType }'.", nameof(parentKey));

        DeleteAction = deleteAction;
        UpdateAction = updateAction;
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
