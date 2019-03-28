using System;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot.Themes;

namespace SJP.Schematic.Reporting.Dot
{
    public class DotRenderOptions
    {
        public bool ShowColumnDataType { get; set; }

        public bool IsReducedColumnSet { get; set; }

        public Identifier HighlightedTable { get; set; }

        public IGraphTheme Theme
        {
            get => _theme;
            set => _theme = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IGraphTheme _theme = new DefaultTheme();

        public string RootPath
        {
            get => _rootPath;
            set => _rootPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _rootPath = "../";

        public static DotRenderOptions Default => new DotRenderOptions();
    }
}
