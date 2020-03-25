using System;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class TableMappingExtensions
    {
        public static Dto.RelationalDatabaseTable ToDto(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var tableName = table.Name.ToDto();
            var columns = table.Columns.Select(c => c.ToDto()).ToList();
            var primaryKey = table.PrimaryKey.MatchUnsafe(pk => pk.ToDto(), () => (Dto.DatabaseKey?)null);
            var uniqueKeys = table.UniqueKeys.Select(uk => uk.ToDto()).ToList();
            var parentKeys = table.ParentKeys.Select(fk => fk.ToDto()).ToList();
            var childKeys = table.ChildKeys.Select(ck => ck.ToDto()).ToList();
            var indexes = table.Indexes.Select(i => i.ToDto()).ToList();
            var checks = table.Checks.Select(ck => ck.ToDto()).ToList();
            var triggers = table.Triggers.Select(t => t.ToDto()).ToList();

            return new Dto.RelationalDatabaseTable
            {
                Name = tableName,
                PrimaryKey = primaryKey,
                UniqueKeys = uniqueKeys,
                Checks = checks,
                ChildKeys = childKeys,
                ParentKeys = parentKeys,
                Columns = columns,
                Indexes = indexes,
                Triggers = triggers
            };
        }

        public static IRelationalDatabaseTable FromDto(this Dto.RelationalDatabaseTable dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var tableName = dto.Name.FromDto();
            var columns = dto.Columns.Select(c => c.FromDto()).ToList();
            var primaryKey = dto.PrimaryKey?.FromDto();
            var primaryKeyOption = primaryKey != null
                ? Option<IDatabaseKey>.Some(primaryKey)
                : Option<IDatabaseKey>.None;
            var uniqueKeys = dto.UniqueKeys.Select(uk => uk.FromDto()).ToList();
            var parentKeys = dto.ParentKeys.Select(fk => fk.FromDto()).ToList();
            var childKeys = dto.ChildKeys.Select(ck => ck.FromDto()).ToList();
            var indexes = dto.Indexes.Select(i => i.FromDto()).ToList();
            var checks = dto.Checks.Select(ck => ck.FromDto()).ToList();
            var triggers = dto.Triggers.Select(t => t.FromDto()).ToList();

            return new RelationalDatabaseTable(
                (Identifier)tableName,
                columns,
                primaryKeyOption,
                uniqueKeys,
                parentKeys,
                childKeys,
                indexes,
                checks,
                triggers
            );
        }
    }
}
