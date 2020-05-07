namespace SJP.Schematic.Core
{
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
        /// The action to perform if the parent key's value is updated.
        /// </summary>
        /// <value>The update action.</value>
        ReferentialAction UpdateAction { get; }

        /// <summary>
        /// The action to perform if the parent key's value is deleted.
        /// </summary>
        /// <value>The delete action.</value>
        ReferentialAction DeleteAction { get; }
    }
}
