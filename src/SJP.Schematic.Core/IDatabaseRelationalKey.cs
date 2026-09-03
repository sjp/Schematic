using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a foreign key relationship.
/// </summary>
public interface IDatabaseRelationalKey
{
    /// <summary>
    /// The parent table name.
    /// </summary>
    /// <value>A table name.</value>
    Identifier ParentTable { get; }

    /// <summary>
    /// The primary or unique key being referred to in the relationship.
    /// </summary>
    /// <value>The parent primary or unique key.</value>
    IDatabaseKey ParentKey { get; }

    /// <summary>
    /// The child table name.
    /// </summary>
    /// <value>A table name.</value>
    Identifier ChildTable { get; }

    /// <summary>
    /// The foreign key defined in the child table.
    /// </summary>
    /// <value>The child foreign key.</value>
    IDatabaseKey ChildKey { get; }

    /// <summary>
    /// <para>The action to perform if the parent key's value is updated.</para>
    /// <para>Dialects without an <c>ON UPDATE</c> clause report <see cref="ReferentialAction.NoAction"/>.</para>
    /// </summary>
    /// <value>The update action.</value>
    ReferentialAction UpdateAction { get; }

    /// <summary>
    /// <para>The action to perform if the parent key's value is deleted.</para>
    /// <para>Dialects without an <c>ON DELETE</c> clause report <see cref="ReferentialAction.NoAction"/>.</para>
    /// </summary>
    /// <value>The delete action.</value>
    ReferentialAction DeleteAction { get; }

    /// <summary>
    /// Describes how the relationship treats child rows whose key columns are only partially
    /// <c>null</c>. Dialects that implement a single behaviour report <see cref="ForeignKeyMatchType.Simple"/>.
    /// </summary>
    /// <value>A foreign key match type.</value>
    ForeignKeyMatchType MatchType { get; }

    /// <summary>
    /// <para>The child key columns set to <c>null</c> when <see cref="DeleteAction"/> is <see cref="ReferentialAction.SetNull"/>.</para>
    /// <para>
    /// Empty when the delete action is not <see cref="ReferentialAction.SetNull"/>, when every child
    /// key column is set, or when the dialect cannot restrict the action to a subset of columns.
    /// </para>
    /// </summary>
    /// <value>A collection of database columns.</value>
    IReadOnlyCollection<IDatabaseColumn> SetNullColumns { get; }
}