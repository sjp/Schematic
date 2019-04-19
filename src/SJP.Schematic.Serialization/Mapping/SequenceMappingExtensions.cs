using System;
using SJP.Schematic.Core;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class SequenceMappingExtensions
    {
        public static Dto.DatabaseSequence ToDto(this IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var name = sequence.Name.ToDto();

            return new Dto.DatabaseSequence
            {
                Name = name,
                Cache = sequence.Cache,
                Cycle = sequence.Cycle,
                Increment = sequence.Increment,
                MaxValue = sequence.MaxValue.ToNullable(),
                MinValue = sequence.MinValue.ToNullable(),
                Start = sequence.Start
            };
        }

        public static IDatabaseSequence FromDto(this Dto.DatabaseSequence dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var sequenceName = dto.Name.FromDto();
            return new DatabaseSequence(
                (Identifier)sequenceName,
                dto.Start,
                dto.Increment,
                dto.MinValue.ToOption(),
                dto.MaxValue.ToOption(),
                dto.Cycle,
                dto.Cache
            );
        }
    }
}
