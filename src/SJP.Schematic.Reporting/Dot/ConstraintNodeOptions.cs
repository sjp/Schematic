using EnumsNET;
using SJP.Schematic.Core;
using System;

namespace SJP.Schematic.Reporting.Dot
{
    internal abstract class ConstraintNodeOptions
    {
        public bool ShowColumnDataType { get; set; }

        public RgbColor TableBackgroundColor
        {
            get => _tableBackgroundColor;
            set => _tableBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        private RgbColor _tableBackgroundColor = new RgbColor("#FFFFFF");

        public abstract RgbColor HeaderBackgroundColor { get; set; }

        public static ConstraintNodeOptions GetDefaultOptions(DatabaseKeyType keyType)
        {
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            if (keyType == DatabaseKeyType.Primary)
                return new PrimaryKeyNodeOptions();
            if (keyType == DatabaseKeyType.Unique)
                return new UniqueKeyNodeOptions();
            if (keyType == DatabaseKeyType.Foreign)
                return new ForeignKeyNodeOptions();

            return null;
        }
    }
}
