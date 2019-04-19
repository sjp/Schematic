using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class SynonymMappingExtensions
    {
        public static Dto.DatabaseSynonym ToDto(this IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            var name = synonym.Name.ToDto();
            var target = synonym.Target.ToDto();

            return new Dto.DatabaseSynonym
            {
                Name = name,
                Target = target
            };
        }

        public static IDatabaseSynonym FromDto(this Dto.DatabaseSynonym dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var name = dto.Name.FromDto();
            var target = dto.Target.FromDto();

            return new DatabaseSynonym((Identifier)name, (Identifier)target);
        }
    }
}
