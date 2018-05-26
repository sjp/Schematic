using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal sealed class PrimaryKeyNodeOptions : ConstraintNodeOptions
    {
        public override RgbColor HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => _headerBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        private RgbColor _headerBackgroundColor = new RgbColor("#EFEBA8");
    }
}
