using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SequenceModelMapper
{
    public Sequence Map(IDatabaseSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        return new Sequence(
            sequence.Name,
            sequence.Start,
            sequence.Increment,
            sequence.MinValue,
            sequence.MaxValue,
            sequence.Cache,
            sequence.Cycle,
            "../"
        );
    }
}