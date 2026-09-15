using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SequenceModelMapper
{
    /// <summary>
    /// Maps a sequence to its detail payload.
    /// </summary>
    /// <param name="sequence">The sequence to map.</param>
    /// <param name="userDefinedTypeTargets">Resolves the sequence's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sequence"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public Sequence Map(IDatabaseSequence sequence, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        return new Sequence(
            sequence.Name,
            sequence.Type.Definition,
            userDefinedTypeTargets.GetTypeUrl(sequence.Type),
            sequence.Start,
            sequence.Increment,
            sequence.MinValue,
            sequence.MaxValue,
            sequence.CacheMode,
            sequence.CacheSize,
            sequence.Cycle,
            sequence.IsOrdered
        );
    }
}