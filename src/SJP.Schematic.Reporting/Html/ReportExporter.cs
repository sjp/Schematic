using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html
{
    public class ReportExporter
    {
        public ReportExporter(IDbConnection connection, IRelationalDatabase database, string directory)
            : this(connection, database, new DirectoryInfo(directory))
        {
        }

        public ReportExporter(IDbConnection connection, IRelationalDatabase database, DirectoryInfo directory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            ExportDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected DirectoryInfo ExportDirectory { get; }

        public void Export()
        {
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            var assetExporter = new AssetExporter();
            assetExporter.SaveAssets(ExportDirectory);

            var tables = Database.Tables.ToList();
            foreach (var table in tables)
            {
                var tableContent = RenderTable(table);
                var tableCont = new Container
                {
                    Content = tableContent,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedTablesCont = _renderer.RenderTemplate(tableCont);

                var outputDir = Path.Combine(ExportDirectory.FullName, "tables");
                var outputPath = Path.Combine(outputDir, table.Name.ToSafeKey() + ".html");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                File.WriteAllText(outputPath, renderedTablesCont);
            }

            var views = Database.Views.ToList();
            foreach (var view in views)
            {
                var viewContent = RenderView(view);
                var viewCont = new Container
                {
                    Content = viewContent,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedViewsCont = _renderer.RenderTemplate(viewCont);

                var outputDir = Path.Combine(ExportDirectory.FullName, "views");
                var outputPath = Path.Combine(outputDir, view.Name.ToSafeKey() + ".html");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                File.WriteAllText(outputPath, renderedViewsCont);
            }

            var columnsContent = RenderColumns();
            var columnsCont = new Container
            {
                Content = columnsContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedColumnsCont = _renderer.RenderTemplate(columnsCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "columns.html"), renderedColumnsCont);

            var constraintContent = RenderConstraint();
            var constraintCont = new Container
            {
                Content = constraintContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedConstraintCont = _renderer.RenderTemplate(constraintCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "constraints.html"), renderedConstraintCont);

            var mainContent = RenderMain();
            var mainCont = new Container
            {
                Content = mainContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedMainCont = _renderer.RenderTemplate(mainCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "index.html"), renderedMainCont);

            var orphansContent = RenderOrphans();
            var orphanCont = new Container
            {
                Content = orphansContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedOrphansCont = _renderer.RenderTemplate(orphanCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "orphans.html"), renderedOrphansCont);

            var relationshipsContent = RenderRelationships();
            var relationshipsCont = new Container
            {
                Content = relationshipsContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedRelationshipsCont = _renderer.RenderTemplate(relationshipsCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "relationships.html"), renderedRelationshipsCont);

            var indexesContent = RenderIndexes();
            var indexesCont = new Container
            {
                Content = indexesContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedIndexesCont = _renderer.RenderTemplate(indexesCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "indexes.html"), renderedIndexesCont);

            var lintContent = RenderLint();
            var lintCont = new Container
            {
                Content = lintContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedLintCont = _renderer.RenderTemplate(lintCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "lint.html"), renderedLintCont);
        }

        public async Task ExportAsync()
        {
            if (!ExportDirectory.Exists)
                ExportDirectory.Create();

            var assetExporter = new AssetExporter();
            await assetExporter.SaveAssetsAsync(ExportDirectory).ConfigureAwait(false);

            var tables = await Database.TablesAsync().ConfigureAwait(false);
            await tables.ForEachAsync(async table =>
            {
                var tableContent = await RenderTableAsync(table).ConfigureAwait(false);
                var tableCont = new Container
                {
                    Content = tableContent,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedTablesCont = _renderer.RenderTemplate(tableCont);

                var outputDir = Path.Combine(ExportDirectory.FullName, "tables");
                var outputPath = Path.Combine(outputDir, table.Name.ToSafeKey() + ".html");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                File.WriteAllText(outputPath, renderedTablesCont);
            }).ConfigureAwait(false);

            var views = await Database.ViewsAsync().ConfigureAwait(false);
            await views.ForEachAsync(async view =>
            {
                var viewContent = await RenderViewAsync(view).ConfigureAwait(false);
                var viewCont = new Container
                {
                    Content = viewContent,
                    DatabaseName = Database.DatabaseName,
                    RootPath = "../"
                };
                var renderedViewsCont = _renderer.RenderTemplate(viewCont);

                var outputDir = Path.Combine(ExportDirectory.FullName, "views");
                var outputPath = Path.Combine(outputDir, view.Name.ToSafeKey() + ".html");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                File.WriteAllText(outputPath, renderedViewsCont);
            }).ConfigureAwait(false);

            var columnsContent = await RenderColumnsAsync().ConfigureAwait(false);
            var columnsCont = new Container
            {
                Content = columnsContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedColumnsCont = _renderer.RenderTemplate(columnsCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "columns.html"), renderedColumnsCont);

            var constraintContent = await RenderConstraintAsync().ConfigureAwait(false);
            var constraintCont = new Container
            {
                Content = constraintContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedConstraintCont = _renderer.RenderTemplate(constraintCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "constraints.html"), renderedConstraintCont);

            var mainContent = await RenderMainAsync().ConfigureAwait(false);
            var mainCont = new Container
            {
                Content = mainContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedMainCont = _renderer.RenderTemplate(mainCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "index.html"), renderedMainCont);

            var orphansContent = await RenderOrphansAsync().ConfigureAwait(false);
            var orphanCont = new Container
            {
                Content = orphansContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedOrphansCont = _renderer.RenderTemplate(orphanCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "orphans.html"), renderedOrphansCont);

            var relationshipsContent = await RenderRelationshipsAsync().ConfigureAwait(false);
            var relationshipsCont = new Container
            {
                Content = relationshipsContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedRelationshipsCont = _renderer.RenderTemplate(relationshipsCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "relationships.html"), renderedRelationshipsCont);

            var indexesContent = await RenderIndexesAsync().ConfigureAwait(false);
            var indexesCont = new Container
            {
                Content = indexesContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedIndexesCont = _renderer.RenderTemplate(indexesCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "indexes.html"), renderedIndexesCont);

            var lintContent = await RenderLintAsync().ConfigureAwait(false);
            var lintCont = new Container
            {
                Content = lintContent,
                DatabaseName = Database.DatabaseName
            };
            var renderedLintCont = _renderer.RenderTemplate(lintCont);
            File.WriteAllText(Path.Combine(ExportDirectory.FullName, "lint.html"), renderedLintCont);
        }

        private string RenderTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var mapper = new TableModelMapper(Connection, Database.Dialect);
            var renderTable = mapper.Map(table);

            return _renderer.RenderTemplate(renderTable);
        }

        private async Task<string> RenderTableAsync(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var mapper = new TableModelMapper(Connection, Database.Dialect);
            var renderTable = await mapper.MapAsync(table).ConfigureAwait(false);

            return _renderer.RenderTemplate(renderTable);
        }

        private string RenderView(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var mapper = new ViewModelMapper(Connection, Database.Dialect);
            var renderView = mapper.Map(view);

            return _renderer.RenderTemplate(renderView);
        }

        private async Task<string> RenderViewAsync(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var mapper = new ViewModelMapper(Connection, Database.Dialect);
            var renderView = await mapper.MapAsync(view).ConfigureAwait(false);

            return _renderer.RenderTemplate(renderView);
        }

        private string RenderColumns()
        {
            var tables = Database.Tables.ToList();
            var views = Database.Views.ToList();

            var mapper = new ColumnsModelMapper(Connection, Database.Dialect);

            var renderColumns = new List<Columns.Column>();
            foreach (var table in tables)
            {
                var renderTables = mapper.Map(table);
                renderColumns.AddRange(renderTables);
            }

            foreach (var view in views)
            {
                var renderViews = mapper.Map(view);
                renderColumns.AddRange(renderViews);
            }

            var column = new Columns
            {
                TableColumns = renderColumns.OrderBy(c => c.Name).ThenBy(c => c.Ordinal).ToList()
            };

            return _renderer.RenderTemplate(column);
        }

        private async Task<string> RenderColumnsAsync()
        {
            var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
            var viewCollection = await Database.ViewsAsync().ConfigureAwait(false);

            var tables = await tableCollection.ToList().ConfigureAwait(false);
            var views = await viewCollection.ToList().ConfigureAwait(false);

            var mapper = new ColumnsModelMapper(Connection, Database.Dialect);

            var renderColumns = new List<Columns.Column>();
            foreach (var table in tables)
            {
                var renderTables = await mapper.MapAsync(table).ConfigureAwait(false);
                renderColumns.AddRange(renderTables);
            }

            foreach (var view in views)
            {
                var renderViews = await mapper.MapAsync(view).ConfigureAwait(false);
                renderColumns.AddRange(renderViews);
            }

            var column = new Columns
            {
                TableColumns = renderColumns.OrderBy(c => c.Name).ThenBy(c => c.Ordinal).ToList()
            };

            return _renderer.RenderTemplate(column);
        }

        private string RenderConstraint()
        {
            var tables = Database.Tables.ToList();

            var primaryKeys = tables.SelectNotNull(t => t.PrimaryKey).ToList();
            var uniqueKeys = tables.SelectMany(t => t.UniqueKeys).ToList();
            var foreignKeys = tables.SelectMany(t => t.ParentKeys).ToList();
            var checkConstraints = tables.SelectMany(t => t.Checks).ToList();

            var mapper = new ConstraintsModelMapper();
            var pkMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>;
            var ukMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>;

            var renderPrimaryKeys = primaryKeys.Select(pkMapper.Map).OrderBy(pk => pk.TableName).ToList();
            var renderUniqueKeys = uniqueKeys.Select(ukMapper.Map).OrderBy(uk => uk.TableName).ToList();
            var renderForeignKeys = foreignKeys.Select(mapper.Map).OrderBy(fk => fk.TableName).ToList();
            var renderCheckConstraints = checkConstraints.Select(mapper.Map).OrderBy(ck => ck.TableName).ToList();

            var parent = new Constraints
            {
                PrimaryKeys = renderPrimaryKeys,
                UniqueKeys = renderUniqueKeys,
                ForeignKeys = renderForeignKeys,
                CheckConstraints = renderCheckConstraints
            };

            return _renderer.RenderTemplate(parent);
        }

        private async Task<string> RenderConstraintAsync()
        {
            var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
            var tables = await tableCollection.ToList().ConfigureAwait(false);

            var primaryKeys = await tables.SelectNotNullAsync(t => t.PrimaryKeyAsync()).ConfigureAwait(false);
            var uniqueKeys = await tables.SelectManyAsync(t => t.UniqueKeysAsync()).ConfigureAwait(false);
            var foreignKeys = await tables.SelectManyAsync(t => t.ParentKeysAsync()).ConfigureAwait(false);
            var checkConstraints = await tables.SelectManyAsync(t => t.ChecksAsync()).ConfigureAwait(false);

            var mapper = new ConstraintsModelMapper();
            var pkMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>;
            var ukMapper = mapper as IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>;

            var renderPrimaryKeys = primaryKeys.Select(pkMapper.Map).OrderBy(pk => pk.TableName).ToList();
            var renderUniqueKeys = uniqueKeys.Select(ukMapper.Map).OrderBy(uk => uk.TableName).ToList();
            var renderForeignKeys = foreignKeys.Select(mapper.Map).OrderBy(fk => fk.TableName).ToList();
            var renderCheckConstraints = checkConstraints.Select(mapper.Map).OrderBy(ck => ck.TableName).ToList();

            var parent = new Constraints
            {
                PrimaryKeys = renderPrimaryKeys,
                UniqueKeys = renderUniqueKeys,
                ForeignKeys = renderForeignKeys,
                CheckConstraints = renderCheckConstraints
            };

            return _renderer.RenderTemplate(parent);
        }

        private string RenderMain()
        {
            var tables = Database.Tables.ToList();
            var views = Database.Views.ToList();
            var sequences = Database.Sequences.ToList();
            var synonyms = Database.Synonyms.ToList();

            var mapper = new MainModelMapper(Connection, Database.Dialect);

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var renderTables = new List<Main.Table>();
            foreach (var table in tables)
            {
                var renderTable = mapper.Map(table);

                var uniqueKeyLookup = table.UniqueKey;
                var uniqueKeyCount = uniqueKeyLookup.UCount();

                var checksLookup = table.Check;
                var checksCount = checksLookup.UCount();

                var indexesLookup = table.Index;
                var indexCount = indexesLookup.UCount();
                indexesCount += indexCount;

                if (table.PrimaryKey != null)
                    constraints++;

                constraints += uniqueKeyCount;
                constraints += renderTable.ParentsCount;
                constraints += checksCount;

                columns += renderTable.ColumnCount;

                renderTables.Add(renderTable);
            }

            var renderViews = new List<Main.View>();
            foreach (var view in views)
            {
                var renderView = mapper.Map(view);
                columns += renderView.ColumnCount;

                renderViews.Add(renderView);
            }

            var renderSynonyms = new List<Main.Synonym>();
            foreach (var synonym in synonyms)
            {
                var model = mapper.Map(synonym);
                Uri targetUrl = null;
                var isTable = Database.TableExists(synonym.Target);
                if (isTable)
                {
                    targetUrl = new Uri("tables/" + synonym.Target.ToSafeKey() + ".html", UriKind.Relative);
                }
                else
                {
                    var isView = Database.ViewExists(synonym.Target);
                    if (isView)
                        targetUrl = new Uri("views/" + synonym.Target.ToSafeKey() + ".html", UriKind.Relative);
                }

                model.TargetUrl = targetUrl;
                renderSynonyms.Add(model);
            }

            var renderSequences = sequences.Select(mapper.Map).ToList();

            var main = new Main
            {
                DatabaseName = Database.DatabaseName ?? string.Empty,
                ProductName = string.Empty,
                ProductVersion = string.Empty,
                ColumnsCount = columns,
                ConstraintsCount = constraints,
                IndexesCount = indexesCount,
                Tables = renderTables,
                Views = renderViews,
                Sequences = renderSequences,
                Synonyms = renderSynonyms
            };

            return _renderer.RenderTemplate(main);
        }

        private async Task<string> RenderMainAsync()
        {
            var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
            var viewCollection = await Database.ViewsAsync().ConfigureAwait(false);
            var sequencesCollection = await Database.SequencesAsync().ConfigureAwait(false);
            var synonymsCollection = await Database.SynonymsAsync().ConfigureAwait(false);

            var tables = await tableCollection.ToList().ConfigureAwait(false);
            var views = await viewCollection.ToList().ConfigureAwait(false);
            var sequences = await sequencesCollection.ToList().ConfigureAwait(false);
            var synonyms = await synonymsCollection.ToList().ConfigureAwait(false);

            var mapper = new MainModelMapper(Connection, Database.Dialect);

            var columns = 0U;
            var constraints = 0U;
            var indexesCount = 0U;
            var renderTables = new List<Main.Table>();
            foreach (var table in tables)
            {
                var renderTable = await mapper.MapAsync(table).ConfigureAwait(false);

                var uniqueKeyLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
                var uniqueKeyCount = uniqueKeyLookup.UCount();

                var checksLookup = await table.CheckAsync().ConfigureAwait(false);
                var checksCount = checksLookup.UCount();

                var indexesLookup = await table.IndexAsync().ConfigureAwait(false);
                var indexCount = indexesLookup.UCount();
                indexesCount += indexCount;

                var primaryKey = await table.PrimaryKeyAsync().ConfigureAwait(false);
                if (primaryKey != null)
                    constraints++;

                constraints += uniqueKeyCount;
                constraints += renderTable.ParentsCount;
                constraints += checksCount;

                columns += renderTable.ColumnCount;

                renderTables.Add(renderTable);
            }

            var renderViews = new List<Main.View>();
            foreach (var view in views)
            {
                var renderView = await mapper.MapAsync(view).ConfigureAwait(false);
                columns += renderView.ColumnCount;

                renderViews.Add(renderView);
            }

            var renderSynonyms = new List<Main.Synonym>();
            foreach (var synonym in synonyms)
            {
                var model = mapper.Map(synonym);
                Uri targetUrl = null;
                var isTable = await Database.TableExistsAsync(synonym.Target).ConfigureAwait(false);
                if (isTable)
                {
                    targetUrl = new Uri("tables/" + synonym.Target.ToSafeKey() + ".html", UriKind.Relative);
                }
                else
                {
                    var isView = await Database.ViewExistsAsync(synonym.Target).ConfigureAwait(false);
                    if (isView)
                        targetUrl = new Uri("views/" + synonym.Target.ToSafeKey() + ".html", UriKind.Relative);
                }

                model.TargetUrl = targetUrl;
                renderSynonyms.Add(model);
            }

            var renderSequences = sequences.Select(mapper.Map).ToList();

            var main = new Main
            {
                DatabaseName = Database.DatabaseName ?? string.Empty,
                ProductName = string.Empty,
                ProductVersion = string.Empty,
                ColumnsCount = columns,
                ConstraintsCount = constraints,
                IndexesCount = indexesCount,
                Tables = renderTables,
                Views = renderViews,
                Sequences = renderSequences,
                Synonyms = renderSynonyms
            };

            return _renderer.RenderTemplate(main);
        }

        private string RenderOrphans()
        {
            var orphanedTables = Database.Tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);
            var renderTables = orphanedTables.Select(mapper.Map)
                .OrderBy(vm => vm.Name)
                .ToList();

            var templateParameter = new Orphans { Tables = renderTables };
            return _renderer.RenderTemplate(templateParameter);
        }

        private async Task<string> RenderOrphansAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            var orphanedTables = new List<IRelationalDatabaseTable>();
            await tables.ForEachAsync(async t =>
            {
                var parentKeys = await t.ParentKeysAsync().ConfigureAwait(false);
                var childKeys = await t.ChildKeysAsync().ConfigureAwait(false);
                if (parentKeys.Empty() && childKeys.Empty())
                    orphanedTables.Add(t);
            }).ConfigureAwait(false);

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);

            var mappingTasks = orphanedTables.Select(mapper.MapAsync).ToArray();
            var renderTables = await Task.WhenAll(mappingTasks).ConfigureAwait(false);

            var templateParameter = new Orphans { Tables = renderTables };
            return _renderer.RenderTemplate(templateParameter);
        }

        private string RenderRelationships()
        {
            var mapper = new RelationshipsModelMapper(Connection);
            var templateParameter = mapper.Map(Database);

            return _renderer.RenderTemplate(templateParameter);
        }

        private async Task<string> RenderRelationshipsAsync()
        {
            var mapper = new RelationshipsModelMapper(Connection);
            var templateParameter = await mapper.MapAsync(Database).ConfigureAwait(false);

            return _renderer.RenderTemplate(templateParameter);
        }

        private string RenderIndexes()
        {
            var mapper = new IndexesModelMapper(Database.Dialect);

            var indexes = Database.Tables
                .SelectMany(t => t.Indexes)
                .Select(mapper.Map)
                .OrderBy(i => i.TableName)
                .ThenBy(i => i.Name)
                .ToList();

            var templateParameter = new Indexes { TableIndexes = indexes };
            return _renderer.RenderTemplate(templateParameter);
        }

        private async Task<string> RenderIndexesAsync()
        {
            var mapper = new IndexesModelMapper(Database.Dialect);

            var tablesAsync = await Database.TablesAsync().ConfigureAwait(false);
            var tables = await tablesAsync.ToList().ConfigureAwait(false);
            var indexesTasks = tables.Select(t => t.IndexesAsync()).ToArray();
            var allIndexes = await Task.WhenAll(indexesTasks).ConfigureAwait(false);

            var indexes = allIndexes
                .SelectMany(i => i)
                .Select(mapper.Map)
                .OrderBy(i => i.TableName)
                .ThenBy(i => i.Name)
                .ToList();

            var templateParameter = new Indexes { TableIndexes = indexes };
            return _renderer.RenderTemplate(templateParameter);
        }

        private string RenderLint()
        {
            var linter = new DatabaseLinter(Connection, Database);
            var messages = linter.AnalyzeDatabase();

            var groupedRules = messages
                .GroupBy(m => m.Title)
                .Select(m => new LintResults.LintRule(m.Key, m.Select(r => r.Message).ToList()))
                .ToList();

            var templateParameter = new LintResults { LintRules = groupedRules };
            return _renderer.RenderTemplate(templateParameter);
        }

        private Task<string> RenderLintAsync()
        {
            var linter = new DatabaseLinter(Connection, Database);
            var messages = linter.AnalyzeDatabase();

            var groupedRules = messages
                .GroupBy(m => m.Title)
                .Select(m => new LintResults.LintRule(m.Key, m.Select(r => r.Message).ToList()))
                .ToList();

            var templateParameter = new LintResults { LintRules = groupedRules };
            return Task.FromResult(_renderer.RenderTemplate(templateParameter));
        }

        private static readonly IHtmlFormatter _renderer = new HtmlFormatter(new TemplateProvider());
    }
}
