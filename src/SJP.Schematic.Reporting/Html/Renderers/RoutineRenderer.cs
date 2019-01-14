using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class RoutineRenderer : ITemplateRenderer
    {
        public RoutineRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "routines"));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var routines = await Database.GetAllRoutines(cancellationToken).ConfigureAwait(false);
            var mapper = new RoutineModelMapper();

            var routineTasks = routines.Select(async routine =>
            {
                var viewModel = mapper.Map(routine);
                var renderedRoutine = Formatter.RenderTemplate(viewModel);

                var routineContainer = new Container(renderedRoutine, Database.IdentifierDefaults.Database, "../");
                var renderedPage = Formatter.RenderTemplate(routineContainer);

                var outputPath = Path.Combine(ExportDirectory.FullName, routine.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
            });
            await Task.WhenAll(routineTasks).ConfigureAwait(false);
        }
    }
}
