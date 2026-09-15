using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

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

        foreach (var table in data.Tables)
        {
            tablesCount++;

            // Counted from the collections rather than the name-keyed lookups, which leave out
            // unnamed constraints and collapse duplicate names, so that the totals match the rows
            // listed on the constraints and indexes pages.
            if (table.PrimaryKey.IsSome)
                constraints++;
            constraints += table.UniqueKeys.UCount();
            constraints += table.ParentKeys.UCount();
            constraints += table.Checks.UCount();

            indexesCount += table.Indexes.UCount();
            columns += table.Columns.UCount();
        }

        var viewsCount = 0U;
        foreach (var view in data.Views)
        {
            viewsCount++;
            columns += view.Columns.UCount();
        }

        // The schema list comes from the report's shared resolution so that the dashboard, the
        // schemas page and the per-schema pages cannot disagree about which schemas exist or what
        // they hold.
        var schemaMapper = new SchemaModelMapper();
        var schemas = data.ResolvedSchemas.Select(schemaMapper.MapSummary).ToList();

        var mainModel = new Main(
            data.Database.IdentifierDefaults.Database,
            data.DatabaseVersion ?? string.Empty,
            columns,
            constraints,
            indexesCount,
            schemas,
            tablesCount,
            viewsCount,
            (uint)data.Sequences.Count,
            (uint)data.Synonyms.Count,
            (uint)data.Routines.Count,
            (uint)data.UserDefinedTypes.Count
        );

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "main.json"));
        await context.JsonWriter.SerializeToFileAsync(outputFile, mainModel, cancellationToken);
        context.Bundle.AddSummary("main", outputFile);
    }
}
