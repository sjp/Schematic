using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

internal sealed class SearchRenderer : IDataRenderer
{
    public async Task RenderAsync(ReportData data, RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(context);

        var entries = new List<Search.SearchEntry>();

        foreach (var table in data.Tables)
        {
            var tableUrl = UrlRouter.GetTableUrl(table.Name);
            var tableName = table.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(tableName, "Table", tableUrl, null));

            foreach (var column in table.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", tableUrl, tableName));
        }

        foreach (var view in data.Views)
        {
            var viewUrl = UrlRouter.GetViewUrl(view.Name);
            var viewName = view.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(viewName, "View", viewUrl, null));

            foreach (var column in view.Columns)
                entries.Add(new Search.SearchEntry(column.Name.LocalName, "Column", viewUrl, viewName));
        }

        foreach (var sequence in data.Sequences)
            entries.Add(new Search.SearchEntry(sequence.Name.ToVisibleName(), "Sequence", UrlRouter.GetSequenceUrl(sequence.Name), null));

        foreach (var synonym in data.Synonyms)
            entries.Add(new Search.SearchEntry(synonym.Name.ToVisibleName(), "Synonym", UrlRouter.GetSynonymUrl(synonym.Name), null));

        foreach (var routine in data.Routines)
            entries.Add(new Search.SearchEntry(routine.Name.ToVisibleName(), "Routine", UrlRouter.GetRoutineUrl(routine.Name), null));

        foreach (var userDefinedType in data.UserDefinedTypes)
        {
            var typeUrl = UrlRouter.GetUserDefinedTypeUrl(userDefinedType.Name);
            var typeName = userDefinedType.Name.ToVisibleName();
            entries.Add(new Search.SearchEntry(typeName, "Type", typeUrl, null));

            // A composite or table type's attributes are as searchable as a table's columns, and
            // are labelled distinctly so a hit leads to the type page rather than looking like a
            // column of a table that does not exist.
            foreach (var attribute in userDefinedType.Attributes)
                entries.Add(new Search.SearchEntry(attribute.Name.LocalName, "Attribute", typeUrl, typeName));
        }

        // Schemas are resolved rather than read straight off data.Schemas, so that the palette
        // lists exactly the schemas the report has a page for.
        var schemaMapper = new SchemaModelMapper();
        foreach (var schema in schemaMapper.GetSchemas(data))
            entries.Add(new Search.SearchEntry(schema.Name.LocalName, "Schema", UrlRouter.GetSchemaUrl(schema.Name), null));

        var searchVm = new Search(entries);

        var json = context.JsonWriter.Serialize(searchVm);
        context.Bundle.AddSummary("search", json);

        var outputFile = new FileInfo(Path.Combine(context.ExportDirectory.FullName, "data", "search.json"));
        await context.JsonWriter.WriteJsonAsync(outputFile, json, cancellationToken);
    }
}
