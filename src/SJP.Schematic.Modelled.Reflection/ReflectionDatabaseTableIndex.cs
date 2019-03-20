using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionDatabaseTableIndex : IDatabaseIndex
    {
        public ReflectionDatabaseTableIndex(IDatabaseDialect dialect, IRelationalDatabaseTable table, IModelledIndex index)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            IsUnique = index.IsUnique;

            Name = dialect.GetAliasOrDefault(index.Property);

            var tableType = index.Property.ReflectedType;
            var propertyLookup = tableType.GetProperties()
                .Select(p => new KeyValuePair<string, PropertyInfo>(p.Name, p))
                .ToDictionary();

            var tableColumnLookup = table.GetColumnLookup();
            var columns = new List<IDatabaseIndexColumn>();
            var includedColumns = new List<IDatabaseColumn>();

            var isFunctionBasedIndex = false;

            foreach (var indexColumn in index.Columns)
            {
                if (indexColumn.Expression.IsIdentity)
                {
                    var expressionName = indexColumn.Expression.DependentNames.Single().LocalName;
                    var columnName = dialect.GetAliasOrDefault(propertyLookup[expressionName]);
                    var tableColumn = tableColumnLookup[columnName];
                    var textExpression = indexColumn.Expression.ToSql(dialect);
                    var column = new DatabaseIndexColumn(textExpression, tableColumn, indexColumn.Order);
                    columns.Add(column);
                }
                else
                {
                    isFunctionBasedIndex = true;
                    var tableColumns = indexColumn.Expression.DependentNames
                        .Select(name => propertyLookup.ContainsKey(name.LocalName) ? propertyLookup[name.LocalName] : null)
                        .Where(prop => prop != null)
                        .Select(prop => dialect.GetAliasOrDefault(prop))
                        .Select(name => tableColumnLookup[name])
                        .ToList();

                    var textExpression = indexColumn.Expression.ToSql(dialect);
                    var column = new DatabaseIndexColumn(textExpression, tableColumns, indexColumn.Order);
                    columns.Add(column);
                }
            }

            foreach (var includedColumn in index.IncludedColumns)
            {
                var includedColumnName = dialect.GetAliasOrDefault(includedColumn.Property);
                var column = tableColumnLookup[includedColumnName];
                includedColumns.Add(column);
            }

            IsFunctionBased = isFunctionBasedIndex;

            Columns = columns;
            IncludedColumns = includedColumns;
        }

        public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        public bool IsUnique { get; }

        public Identifier Name { get; }

        public bool IsFunctionBased { get; }

        // this should always be true
        // is there a situation where would not want it to be true?
        public bool IsEnabled { get; } = true;
    }
}
