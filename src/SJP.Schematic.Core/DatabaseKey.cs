using System;
using System.Collections.Generic;
using System.ComponentModel;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    public class DatabaseKey : IDatabaseKey
    {
        public DatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Name = name.Map(n => Identifier.CreateQualifiedIdentifier(n.LocalName)); // strip to localname only
            KeyType = keyType;
            Columns = columns;
            IsEnabled = isEnabled;
        }

        public Option<Identifier> Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        public bool IsEnabled { get; }

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
