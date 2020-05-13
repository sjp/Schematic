using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class SqliteDatabaseKey : IDatabaseKey
    {
        public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Name = name.Map(n => Identifier.CreateQualifiedIdentifier(n.LocalName));
            KeyType = keyType;
            Columns = columns.ToList();
        }

        public Option<Identifier> Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; } = true;

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

                builder.Append(KeyType.ToString())
                    .Append(" Key");

                Name.IfSome(name =>
                {
                    builder.Append(": ")
                        .Append(name.LocalName);
                });

                return builder.GetStringAndRelease();
            }
        }
    }
}
