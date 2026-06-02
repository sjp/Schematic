using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The search index (<c>data/search.json</c>): a flat list of every searchable object — tables,
/// views, sequences, synonyms, routines, and their columns — each carrying a display name, an
/// object-type label, and the in-app hash route to navigate to. Consumed by the Cmd/Ctrl-K
/// command palette.
/// </summary>
public sealed class Search
{
    public Search(IEnumerable<SearchEntry> entries)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        EntriesCount = entries.UCount();
    }

    public IEnumerable<SearchEntry> Entries { get; }

    public uint EntriesCount { get; }

    /// <summary>
    /// A single searchable object.
    /// </summary>
    public sealed class SearchEntry
    {
        public SearchEntry(string name, string objectType, string url, string? parent)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Parent = parent;
        }

        public string Name { get; }

        public string ObjectType { get; }

        public string Url { get; }

        /// <summary>
        /// The owning object's name for a column entry; <c>null</c> for top-level objects.
        /// Omitted from the JSON when null.
        /// </summary>
        public string? Parent { get; }
    }
}
