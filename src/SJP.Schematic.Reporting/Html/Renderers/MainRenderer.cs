using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class MainRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var columns = 0U;
        var constraints = 0U;
        var indexesCount = 0U;
        var tablesCount = 0U;

        var tableNames = new List<Identifier>();
        foreach (var table in data.Tables)
        {
            tablesCount++;

            var uniqueKeyCount = table.GetUniqueKeyLookup().UCount();
            var checksCount = table.GetCheckLookup().UCount();
            indexesCount += table.GetIndexLookup().UCount();

            await table.PrimaryKey.IfSomeAsync(_ => constraints++).ConfigureAwait(false);

            constraints += uniqueKeyCount;
            constraints += table.ParentKeys.UCount();
            constraints += checksCount;

            columns += table.Columns.UCount();

            tableNames.Add(table.Name);
        }

        var viewNames = new List<Identifier>();
        var viewsCount = 0U;
        foreach (var view in data.Views)
        {
            viewsCount++;
            columns += view.Columns.UCount();
            viewNames.Add(view.Name);
        }

        var sequenceNames = data.Sequences.Select(static s => s.Name).ToList();
        var routineNames = data.Routines.Select(static r => r.Name).ToList();
        var synonymNames = data.Synonyms.Select(static s => s.Name).ToList();

        var schemas = tableNames
            .Union(viewNames)
            .Union(sequenceNames)
            .Union(synonymNames)
            .Union(routineNames)
            .Select(static n => n.Schema)
            .Where(static n => n != null)
            .Distinct(StringComparer.Ordinal)
            .Select(static s => s!)
            .Order(StringComparer.Ordinal)
            .ToList();

        var mainModel = new Main(
            data.Database.IdentifierDefaults.Database,
            data.DatabaseVersion ?? string.Empty,
            columns,
            constraints,
            indexesCount,
            schemas,
            tablesCount,
            viewsCount,
            (uint)sequenceNames.Count,
            (uint)synonymNames.Count,
            (uint)routineNames.Count
        );

        var json = context.JsonWriter.Serialize(mainModel);
        context.Bundle.AddSummary("main", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "main.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken).ConfigureAwait(false);
    }
}
