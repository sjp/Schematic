using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The synonyms summary payload (<c>data/synonyms.json</c>): the schema's synonyms and the objects
/// they alias.
/// </summary>
public sealed class Synonyms
{
    public Synonyms(IEnumerable<Main.Synonym> synonyms)
    {
        if (synonyms.NullOrAnyNull())
            throw new ArgumentNullException(nameof(synonyms));

        SynonymsCount = synonyms.UCount();
        AllSynonyms = synonyms;
    }

    public uint SynonymsCount { get; }

    public IEnumerable<Main.Synonym> AllSynonyms { get; }
}
