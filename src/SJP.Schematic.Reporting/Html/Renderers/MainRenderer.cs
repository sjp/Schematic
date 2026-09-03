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

            await table.PrimaryKey.IfSomeAsync(_ => constraints++);

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

        var objectCountsBySchema = tableNames
            .Concat(viewNames)
            .Concat(sequenceNames)
            .Concat(synonymNames)
            .Concat(routineNames)
            .Select(static n => n.Schema)
            .Where(static n => n != null)
            .GroupBy(static n => n!, StringComparer.Ordinal)
            .ToDictionary(static g => g.Key, static g => (uint)g.Count(), StringComparer.Ordinal);

        var schemas = BuildSchemas(data, objectCountsBySchema);

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
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }

    /// <summary>
    /// Combines the schemas the database declares with the schemas that the report's objects are
    /// named in. A dialect that reports no schemas still gets a list, and a schema holding no
    /// objects is still listed as long as a user declared it. System schemas are only listed when
    /// they hold something the report covers, so that e.g. SQL Server's fixed-role schemas do not
    /// crowd out the ones a reader cares about.
    /// </summary>
    private static IReadOnlyCollection<Main.Schema> BuildSchemas(ReportData data, IReadOnlyDictionary<string, uint> objectCountsBySchema)
    {
        var defaultSchema = data.Database.IdentifierDefaults.Schema;
        var schemas = new Dictionary<string, Main.Schema>(StringComparer.Ordinal);

        foreach (var schema in data.Schemas)
        {
            var name = schema.Name.LocalName;
            var objectCount = objectCountsBySchema.GetValueOrDefault(name);
            if (schema.IsSystem && objectCount == 0)
                continue;

            schemas[name] = new Main.Schema(name, schema.IsDefault, schema.IsSystem, objectCount);
        }

        foreach (var (name, objectCount) in objectCountsBySchema)
        {
            if (schemas.ContainsKey(name))
                continue;

            var isDefault = string.Equals(name, defaultSchema, StringComparison.Ordinal);
            schemas[name] = new Main.Schema(name, isDefault, false, objectCount);
        }

        return schemas.Values
            .OrderBy(static s => s.Name, StringComparer.Ordinal)
            .ToList();
    }
}
