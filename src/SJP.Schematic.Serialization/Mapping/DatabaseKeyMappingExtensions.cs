using System;
using System.Linq;
using SJP.Schematic.Core;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DatabaseKeyMappingExtensions
    {
        public static Dto.DatabaseKey ToDto(this IDatabaseKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var keyColumns = key.Columns.Select(c => c.ToDto()).ToList();
            var keyName = key.Name.MatchUnsafe(name => name.ToDto(), () => null);

            return new Dto.DatabaseKey
            {
                Columns = keyColumns,
                KeyType = key.KeyType,
                Name = keyName,
                IsEnabled = key.IsEnabled
            };
        }

        public static IDatabaseKey FromDto(this Dto.DatabaseKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var keyName = key.Name.FromDto();
            var columnNames = key.Columns.Select(c => c.FromDto()).ToList();

            return new DatabaseKey(
                keyName,
                key.KeyType,
                columnNames,
                key.IsEnabled
            );
        }
    }
}
