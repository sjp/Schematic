using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels.Mappers
{
    internal class TableModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseTable, Table>
    {
        public TableModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public Table Map(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);

            var result = new Table
            {
                TableName = dbObject.Name,
                RowCount = rowCount
            };

            var tableColumns = dbObject.Columns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = dbObject.PrimaryKey;
            var uniqueKeys = dbObject.UniqueKeys.ToList();
            var parentKeys = dbObject.ParentKeys.ToList();
            var childKeys = dbObject.ChildKeys.ToList();
            var checks = dbObject.Checks.ToList();

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = dbObject.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == columnName);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == columnName));
                var isParentKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName));

                var column = new Table.Column(result.Name, columnName)
                {
                    Ordinal = tableColumn.Ordinal,
                    IsPrimaryKeyColumn = isPrimaryKey,
                    IsUniqueKeyColumn = isUniqueKey,
                    IsForeignKeyColumn = isParentKey,
                    DefaultValue = col.DefaultValue,
                    IsNullable = col.IsNullable,
                    Type = col.Type.Definition
                };

                var matchingParentKeys = parentKeys.Where(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName)).ToList();
                var columnParentKeys = new List<Table.ParentKey>();
                foreach (var parentKey in matchingParentKeys)
                {
                    var columnIndexes = parentKey.ChildKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var parentColumnNames = parentKey.ParentKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(parentKey.ParentKey.Table.Name, colName, qualifiedColumnName)
                        {
                            ConstraintName = parentKey.ChildKey.Name?.LocalName
                        }
                    )
                    .ToList();

                    columnParentKeys.AddRange(columnFks);
                }

                column.ParentKeys = columnParentKeys;

                var matchingChildKeys = childKeys.Where(ck => ck.ParentKey.Columns.Any(ckc => ckc.Name.LocalName == columnName)).ToList();
                var columnChildKeys = new List<Table.ChildKey>();
                foreach (var childKey in matchingChildKeys)
                {
                    var columnIndexes = childKey.ParentKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var childColumnNames = childKey.ChildKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(childKey.ChildKey.Table.Name, colName, qualifiedColumnName)
                        {
                            ConstraintName = childKey.ChildKey.Name?.LocalName
                        }
                    )
                    .ToList();

                    columnChildKeys.AddRange(columnFks);
                }

                column.ChildKeys = columnChildKeys;

                columns.Add(column);
            }

            var tableIndexes = dbObject.Indexes.ToList();
            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index
                {
                    Name = index.Name?.LocalName,
                    Unique = index.IsUnique,
                    Columns = index.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                    IncludedColumns = index.IncludedColumns.Select(c => c.Name.LocalName).ToList(),
                    ColumnSorts = index.Columns.Select(c => c.Order).ToList()
                }
            ).ToList();

            if (primaryKey != null)
            {
                var pkConstraint = new Table.PrimaryKeyConstraint
                {
                    Columns = primaryKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = primaryKey.Name?.LocalName
                };
                result.PrimaryKey = pkConstraint;
            }

            var renderUniqueKeys = new List<Table.UniqueKey>();
            foreach (var uniqueKey in uniqueKeys)
            {
                var uk = new Table.UniqueKey
                {
                    Columns = uniqueKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = uniqueKey.Name?.LocalName
                };
                renderUniqueKeys.Add(uk);
            }
            result.UniqueKeys = renderUniqueKeys;

            var renderParentKeys = new List<Table.ForeignKey>();
            foreach (var parentKey in parentKeys)
            {
                var fk = new Table.ForeignKey(parentKey.ParentKey.Table.Name)
                {
                    ChildColumns = parentKey.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = parentKey.ChildKey.Name?.LocalName,
                    ParentColumns = parentKey.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ParentConstraintName = parentKey.ParentKey.Name?.LocalName,
                    DeleteRule = parentKey.DeleteRule,
                    UpdateRule = parentKey.UpdateRule
                };
                renderParentKeys.Add(fk);
            }
            result.ForeignKeys = renderParentKeys;

            var renderChecks = new List<Table.CheckConstraint>();
            foreach (var check in checks)
            {
                var ck = new Table.CheckConstraint
                {
                    Definition = check.Definition,
                    ConstraintName = check.Name?.LocalName
                };
                renderChecks.Add(ck);
            }

            result.Columns = columns;
            result.Indexes = mappedIndexes;

            return result;
        }

        public async Task<Table> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);

            var result = new Table
            {
                TableName = dbObject.Name,
                RowCount = rowCount
            };

            var dbColumns = await dbObject.ColumnsAsync().ConfigureAwait(false);
            var tableColumns = dbColumns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = await dbObject.PrimaryKeyAsync().ConfigureAwait(false);
            var uniqueKeys = await dbObject.UniqueKeysAsync().ConfigureAwait(false);
            var parentKeys = await dbObject.ParentKeysAsync().ConfigureAwait(false);
            var childKeys = await dbObject.ChildKeysAsync().ConfigureAwait(false);
            var checks = await dbObject.ChecksAsync().ConfigureAwait(false);
            var tableIndexes = await dbObject.IndexesAsync().ConfigureAwait(false);

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = dbObject.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == columnName);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == columnName));
                var isParentKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName));

                var column = new Table.Column(result.Name, columnName)
                {
                    Ordinal = tableColumn.Ordinal,
                    IsPrimaryKeyColumn = isPrimaryKey,
                    IsUniqueKeyColumn = isUniqueKey,
                    IsForeignKeyColumn = isParentKey,
                    DefaultValue = col.DefaultValue,
                    IsNullable = col.IsNullable,
                    Type = col.Type.Definition
                };

                var matchingParentKeys = parentKeys.Where(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName)).ToList();
                var columnParentKeys = new List<Table.ParentKey>();
                foreach (var parentKey in matchingParentKeys)
                {
                    var columnIndexes = parentKey.ChildKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var parentColumnNames = parentKey.ParentKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(parentKey.ParentKey.Table.Name, colName, qualifiedColumnName)
                        {
                            ConstraintName = parentKey.ChildKey.Name?.LocalName
                        }
                    )
                    .ToList();

                    columnParentKeys.AddRange(columnFks);
                }

                column.ParentKeys = columnParentKeys;

                var matchingChildKeys = childKeys.Where(ck => ck.ParentKey.Columns.Any(ckc => ckc.Name.LocalName == columnName)).ToList();
                var columnChildKeys = new List<Table.ChildKey>();
                foreach (var childKey in matchingChildKeys)
                {
                    var columnIndexes = childKey.ParentKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var childColumnNames = childKey.ChildKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(childKey.ChildKey.Table.Name, colName, qualifiedColumnName)
                        {
                            ConstraintName = childKey.ChildKey.Name?.LocalName
                        }
                    )
                    .ToList();

                    columnChildKeys.AddRange(columnFks);
                }

                column.ChildKeys = columnChildKeys;

                columns.Add(column);
            }

            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index
                {
                    Name = index.Name?.LocalName,
                    Unique = index.IsUnique,
                    Columns = index.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                    IncludedColumns = index.IncludedColumns.Select(c => c.Name.LocalName).ToList(),
                    ColumnSorts = index.Columns.Select(c => c.Order).ToList()
                }
            ).ToList();

            if (primaryKey != null)
            {
                var pkConstraint = new Table.PrimaryKeyConstraint
                {
                    Columns = primaryKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = primaryKey.Name?.LocalName
                };
                result.PrimaryKey = pkConstraint;
            }

            var renderUniqueKeys = new List<Table.UniqueKey>();
            foreach (var uniqueKey in uniqueKeys)
            {
                var uk = new Table.UniqueKey
                {
                    Columns = uniqueKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = uniqueKey.Name?.LocalName
                };
                renderUniqueKeys.Add(uk);
            }
            result.UniqueKeys = renderUniqueKeys;

            var renderParentKeys = new List<Table.ForeignKey>();
            foreach (var parentKey in parentKeys)
            {
                var fk = new Table.ForeignKey(parentKey.ParentKey.Table.Name)
                {
                    ChildColumns = parentKey.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ConstraintName = parentKey.ChildKey.Name?.LocalName,
                    ParentColumns = parentKey.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    ParentConstraintName = parentKey.ParentKey.Name?.LocalName,
                    DeleteRule = parentKey.DeleteRule,
                    UpdateRule = parentKey.UpdateRule
                };
                renderParentKeys.Add(fk);
            }
            result.ForeignKeys = renderParentKeys;

            var renderChecks = new List<Table.CheckConstraint>();
            foreach (var check in checks)
            {
                var ck = new Table.CheckConstraint
                {
                    Definition = check.Definition,
                    ConstraintName = check.Name?.LocalName
                };
                renderChecks.Add(ck);
            }

            result.Columns = columns;
            result.Indexes = mappedIndexes;

            return result;
        }
    }
}
