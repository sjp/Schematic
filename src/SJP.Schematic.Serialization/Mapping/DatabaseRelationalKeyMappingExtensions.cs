using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DatabaseRelationalKeyMappingExtensions
    {
        public static Dto.DatabaseRelationalKey ToDto(this IDatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var childTable = relationalKey.ChildTable.ToDto();
            var childKey = relationalKey.ChildKey.ToDto();
            var parentTable = relationalKey.ParentTable.ToDto();
            var parentKey = relationalKey.ParentKey.ToDto();

            return new Dto.DatabaseRelationalKey
            {
                ChildTable = childTable,
                ChildKey = childKey,
                ParentTable = parentTable,
                ParentKey = parentKey,
                DeleteRule = relationalKey.DeleteRule,
                UpdateRule = relationalKey.UpdateRule
            };
        }

        public static IDatabaseRelationalKey FromDto(this Dto.DatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var childTable = relationalKey.ChildTable.FromDto();
            var childKey = relationalKey.ChildKey.FromDto();
            var parentTable = relationalKey.ParentTable.FromDto();
            var parentKey = relationalKey.ParentKey.FromDto();

            return new DatabaseRelationalKey(
                (Identifier)childTable,
                childKey,
                (Identifier)parentTable,
                parentKey,
                relationalKey.DeleteRule,
                relationalKey.UpdateRule
            );
        }
    }
}
