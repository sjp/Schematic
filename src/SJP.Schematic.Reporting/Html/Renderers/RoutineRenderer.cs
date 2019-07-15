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

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class RoutineRenderer : ITemplateRenderer
    {
        public RoutineRenderer(
            IIdentifierDefaults identifierDefaults,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseRoutine> routines,
            DirectoryInfo exportDirectory
        )
        {
            if (routines == null || routines.AnyNull())
                throw new ArgumentNullException(nameof(routines));

            Routines = routines;

            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

            if (exportDirectory == null)
                throw new ArgumentNullException(nameof(exportDirectory));

            ExportDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, "routines"));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

        private DirectoryInfo ExportDirectory { get; }

        public Task RenderAsync(CancellationToken cancellationToken = default)
        {
            var mapper = new RoutineModelMapper();

            var routineTasks = Routines.Select(async routine =>
            {
                var viewModel = mapper.Map(routine);
                var renderedRoutine = await Formatter.RenderTemplateAsync(viewModel).ConfigureAwait(false);

                var databaseName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                    ? IdentifierDefaults.Database + " Database"
                    : "Database";
                var pageTitle = routine.Name.ToVisibleName() + " · Routine · " + databaseName;
                var routineContainer = new Container(renderedRoutine, pageTitle, "../");
                var renderedPage = await Formatter.RenderTemplateAsync(routineContainer).ConfigureAwait(false);

                var outputPath = Path.Combine(ExportDirectory.FullName, routine.Name.ToSafeKey() + ".html");
                if (!ExportDirectory.Exists)
                    ExportDirectory.Create();

                using (var writer = File.CreateText(outputPath))
                {
                    await writer.WriteAsync(renderedPage).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }
            });

            return Task.WhenAll(routineTasks);
        }
    }
}
