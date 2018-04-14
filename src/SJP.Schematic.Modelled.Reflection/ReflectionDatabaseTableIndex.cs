using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionDatabaseTableIndex : IDatabaseTableIndex
    {
        public ReflectionDatabaseTableIndex(IRelationalDatabaseTable table, IModelledIndex index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            Parent = table ?? throw new ArgumentNullException(nameof(table));
            IsUnique = index.IsUnique;

            var dialect = table.Database.Dialect;
            Name = dialect.GetAliasOrDefault(index.Property);

            var tableType = index.Property.DeclaringType.GetTypeInfo();
            var propertyLookup = tableType.GetProperties()
                .Select(p => new KeyValuePair<string, PropertyInfo>(p.Name, p))
                .ToDictionary();

            var columns = new List<IDatabaseIndexColumn>();
            var includedColumns = new List<IDatabaseTableColumn>();

            var isFunctionBasedIndex = false;

            foreach (var indexColumn in index.Columns)
            {
                if (indexColumn.Expression.IsIdentity)
                {
                    var expressionName = indexColumn.Expression.DependentNames.Single().LocalName;
                    var columnName = dialect.GetAliasOrDefault(propertyLookup[expressionName]);
                    var tableColumns = new List<IDatabaseColumn> { Parent.Column[columnName] };
                    var column = new ReflectionIndexColumn(indexColumn.Expression, tableColumns, indexColumn.Order);
                    columns.Add(column);
                }
                else
                {
                    isFunctionBasedIndex = true;
                    var tableColumns = indexColumn.Expression.DependentNames
                        .Select(name => propertyLookup.ContainsKey(name.LocalName) ? propertyLookup[name.LocalName] : null)
                        .Where(prop => prop != null)
                        .Select(prop => dialect.GetAliasOrDefault(prop))
                        .Select(name => table.Column[name] as IDatabaseColumn)
                        .ToList();
                    var column = new ReflectionIndexColumn(indexColumn.Expression, tableColumns, indexColumn.Order);
                    columns.Add(column);
                }
            }

            foreach (var includedColumn in index.IncludedColumns)
            {
                var includedColumnName = dialect.GetAliasOrDefault(includedColumn.Property);
                var column = table.Column[includedColumnName];
                includedColumns.Add(column);
            }

            IsFunctionBased = isFunctionBasedIndex;

            Columns = columns;
            IncludedColumns = includedColumns;
        }

        public IRelationalDatabaseTable Table => Parent;

        public IRelationalDatabaseTable Parent { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        public bool IsUnique { get; }

        public Identifier Name { get; }

        public bool IsFunctionBased { get; }

        // this should always be true
        // is there a situation where would not want it to be true?
        public bool IsEnabled { get; } = true;
    }
}
