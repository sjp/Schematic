using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
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
        /// <exception cref="ArgumentException">
        /// <paramref name="deleteAction"/> will throw this exception if given an invalid enum value.
        /// Alternatively if the child key is not a foreign key this will also be thrown.
        /// Furthermore, if the parent key is not a unique or primary key, this will also be thrown.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="parentTableName"/> or <paramref name="childTableName"/> or <paramref name="parentKey"/> or <paramref name="childKey"/> is <c>null</c></exception>
        public OracleRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, ReferentialAction deleteAction)
        {
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(deleteAction));

            ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

            if (ChildKey.KeyType != DatabaseKeyType.Foreign)
                throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{ childKey.KeyType }'.", nameof(childKey));
            if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{ parentKey.KeyType }'.", nameof(parentKey));

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
        /// The action to perform if the parent key's value is updated. Always <see cref="ReferentialAction.NoAction"/>.
        /// </summary>
        /// <value>Always <see cref="ReferentialAction.NoAction"/>.</value>
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
}
