using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Dot
{
    public class DotRenderOptions
    {
        public bool ShowColumnDataType { get; set; }

        public bool IsReducedColumnSet { get; set; }

        public Identifier HighlightedTable { get; set; }

        public string RootPath
        {
            get => _rootPath;
            set => _rootPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _rootPath = "../";

        public static DotRenderOptions Default => new DotRenderOptions();
    }
}
