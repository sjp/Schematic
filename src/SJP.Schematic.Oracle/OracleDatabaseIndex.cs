using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class OracleDatabaseIndex : IOracleDatabaseIndex
    {
        public OracleDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, OracleIndexProperties properties)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!properties.IsValid())
                throw new ArgumentException($"The { nameof(OracleIndexProperties) } provided must be a valid enum.", nameof(properties));

            Name = name.LocalName;
            IsUnique = isUnique;
            Columns = columns;

            GeneratedByConstraint = (properties & ConstraintGeneratedProps) == ConstraintGeneratedProps;
        }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; } = Array.Empty<IDatabaseColumn>();

        public bool IsEnabled { get; } = true;

        public bool GeneratedByConstraint { get; }

        private const OracleIndexProperties ConstraintGeneratedProps = OracleIndexProperties.Unique | OracleIndexProperties.CreatedByConstraint;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Index: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append('.');

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
