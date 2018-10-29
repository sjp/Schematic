using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
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

            GeneratedByConstraint = (properties & _constraintGeneratedProps) == _constraintGeneratedProps;
        }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; } = Array.Empty<IDatabaseColumn>();

        public bool IsEnabled { get; } = true;

        public bool GeneratedByConstraint { get; }

        private const OracleIndexProperties _constraintGeneratedProps = OracleIndexProperties.Unique | OracleIndexProperties.CreatedByConstraint;
    }
}
