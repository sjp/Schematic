using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class MainRenderer : ITemplateRenderer
{
    public MainRenderer(
        IRelationalDatabase database,
        IHtmlFormatter formatter,
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        IEnumerable<IDatabaseSequence> sequences,
        IEnumerable<IDatabaseSynonym> synonyms,
        IEnumerable<IDatabaseRoutine> routines,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        string dbVersion,
        DirectoryInfo exportDirectory)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));

        Database = database ?? throw new ArgumentNullException(nameof(database));
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        DatabaseDisplayVersion = dbVersion;
        ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
    }

    private IRelationalDatabase Database { get; }

    private IHtmlFormatter Formatter { get; }

    private IEnumerable<IRelationalDatabaseTable> Tables { get; }

    private IEnumerable<IDatabaseView> Views { get; }

    private IEnumerable<IDatabaseSequence> Sequences { get; }

    private IEnumerable<IDatabaseSynonym> Synonyms { get; }

    private IEnumerable<IDatabaseRoutine> Routines { get; }

    private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    private string DatabaseDisplayVersion { get; }

    private DirectoryInfo ExportDirectory { get; }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new MainModelMapper();

        var columns = 0U;
        var constraints = 0U;
        var indexesCount = 0U;
        var tableViewModels = new List<Main.Table>();

        var tableNames = new List<Identifier>();

        foreach (var table in Tables)
        {
            if (!RowCounts.TryGetValue(table.Name, out var rowCount))
                rowCount = 0;

            var renderTable = mapper.Map(table, rowCount);

            var uniqueKeyLookup = table.GetUniqueKeyLookup();
            var uniqueKeyCount = uniqueKeyLookup.UCount();

            var checksLookup = table.GetCheckLookup();
            var checksCount = checksLookup.UCount();

            var indexesLookup = table.GetIndexLookup();
            var indexCount = indexesLookup.UCount();
            indexesCount += indexCount;

            await table.PrimaryKey.IfSomeAsync(_ => constraints++);

            constraints += uniqueKeyCount;
            constraints += renderTable.ParentsCount;
            constraints += checksCount;

            columns += renderTable.ColumnCount;

            tableViewModels.Add(renderTable);
            tableNames.Add(table.Name);
        }

        var viewNames = new List<Identifier>();
        var viewViewModels = new List<Main.View>();
        foreach (var view in Views)
        {
            var vm = mapper.Map(view);
            viewViewModels.Add(vm);
            viewNames.Add(view.Name);
        }
        columns += (uint)viewViewModels.Sum(static v => v.ColumnCount);

        var sequenceNames = new List<Identifier>();
        var sequenceViewModels = new List<Main.Sequence>();
        foreach (var sequence in Sequences)
        {
            var vm = mapper.Map(sequence);
            sequenceViewModels.Add(vm);
            sequenceNames.Add(sequence.Name);
        }

        var routineNames = new List<Identifier>();
        var routineViewModels = new List<Main.Routine>();
        foreach (var routine in Routines)
        {
            var vm = mapper.Map(routine);
            routineViewModels.Add(vm);
            routineNames.Add(routine.Name);
        }

        var synonymNames = Synonyms.Select(static s => s.Name).ToList();
        var synonymTargets = new SynonymTargets(tableNames, viewNames, sequenceNames, synonymNames, routineNames);
        var synonymViewModels = Synonyms.Select(s => mapper.Map(s, synonymTargets)).ToList();

        var schemas = tableNames
            .Union(viewNames)
            .Union(sequenceNames)
            .Union(synonymNames)
            .Union(routineNames)
            .Select(static n => n.Schema)
            .Where(static n => n != null)
            .Distinct(StringComparer.Ordinal)
            .Where(static s => s != null)
            .Select(static s => s!)
            .Order(StringComparer.Ordinal)
            .ToList();

        var templateParameter = new Main(
            Database.IdentifierDefaults.Database,
            DatabaseDisplayVersion ?? string.Empty,
            columns,
            constraints,
            indexesCount,
            schemas,
            tableViewModels,
            viewViewModels,
            sequenceViewModels,
            synonymViewModels,
            routineViewModels
        );

        var renderedMain = await Formatter.RenderTemplateAsync(templateParameter, cancellationToken);

        var databaseName = !Database.IdentifierDefaults.Database.IsNullOrWhiteSpace()
            ? Database.IdentifierDefaults.Database + " Database"
            : "Database";
        var pageTitle = "Home · " + databaseName;
        var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
        var renderedPage = await Formatter.RenderTemplateAsync(mainContainer, cancellationToken);

        if (!ExportDirectory.Exists)
            ExportDirectory.Create();
        var outputPath = Path.Combine(ExportDirectory.FullName, "index.html");

        await using var writer = File.CreateText(outputPath);
        await writer.WriteAsync(renderedPage.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }
}