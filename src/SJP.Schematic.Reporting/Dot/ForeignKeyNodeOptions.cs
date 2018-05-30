using System;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class ForeignKeyNodeOptions : ConstraintNodeOptions
    {
        public override RgbColor HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => _headerBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        private RgbColor _headerBackgroundColor = new RgbColor("#E5E5E5");
    }
}
