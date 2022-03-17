﻿using System;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Dot.Themes;

namespace SJP.Schematic.Dot;

internal sealed class TableNodeOptions
{
    public static TableNodeOptions Default { get; } = new TableNodeOptions();

    public bool ShowColumnDataType { get; set; }

    public bool IsReducedColumnSet { get; set; }

    public bool ShowRowCounts { get; set; } = true;

    public bool IsHighlighted { get; set; }

    public IGraphTheme Theme
    {
        get => _theme;
        set => _theme = value ?? throw new ArgumentNullException(nameof(value));
    }

    private IGraphTheme _theme = new DefaultTheme();

    public RgbColor GetKeyHeaderBackgroundColor(DatabaseKeyType keyType)
    {
        if (!keyType.IsValid())
            throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

        return keyType switch
        {
            DatabaseKeyType.Primary => Theme.PrimaryKeyHeaderBackgroundColor,
            DatabaseKeyType.Unique => Theme.UniqueKeyHeaderBackgroundColor,
            DatabaseKeyType.Foreign => Theme.ForeignKeyHeaderBackgroundColor,
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), "Unknown or unsupported key type: " + keyType.GetName()),
        };
    }

    public RgbColor GetHighlightedKeyHeaderBackgroundColor(DatabaseKeyType keyType)
    {
        if (!keyType.IsValid())
            throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

        return keyType switch
        {
            DatabaseKeyType.Primary => Theme.HighlightedPrimaryKeyHeaderBackgroundColor,
            DatabaseKeyType.Unique => Theme.HighlightedUniqueKeyHeaderBackgroundColor,
            DatabaseKeyType.Foreign => Theme.HighlightedForeignKeyHeaderBackgroundColor,
            _ => throw new ArgumentOutOfRangeException(nameof(keyType), "Unknown or unsupported key type: " + keyType.GetName()),
        };
    }
}
