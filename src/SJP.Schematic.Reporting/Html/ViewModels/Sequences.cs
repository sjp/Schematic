using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The sequences summary payload (<c>data/sequences.json</c>): the schema's sequences and their
/// generation parameters.
/// </summary>
public sealed class Sequences
{
    public Sequences(IEnumerable<Main.Sequence> sequences)
    {
        if (sequences.NullOrAnyNull())
            throw new ArgumentNullException(nameof(sequences));

        SequencesCount = sequences.UCount();
        AllSequences = sequences;
    }

    public uint SequencesCount { get; }

    public IEnumerable<Main.Sequence> AllSequences { get; }
}
