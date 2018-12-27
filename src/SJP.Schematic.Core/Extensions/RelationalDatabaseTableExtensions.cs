using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions
{
    public static class RelationalDatabaseTableExtensions
    {
        public static IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> GetCheckLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var checks = table.Checks;
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
            {
                check.Name.IfSome(name => result[name.LocalName] = check);
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> GetCheckLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetCheckLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseCheckConstraint>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columns = table.Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns)
            {
                if (column.Name != null)
                    result[column.Name.LocalName] = column;
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetColumnLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseColumn>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetIndexLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var indexes = table.Indexes;
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
            {
                if (index.Name != null)
                    result[index.Name.LocalName] = index;
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetIndexLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetIndexLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseIndex>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> GetParentKeyLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var parentKeys = table.ParentKeys;
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
            {
                parentKey.ChildKey.Name.IfSome(name => result[name.LocalName] = parentKey);
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> GetParentKeyLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetParentKeyLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseRelationalKey>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseTrigger> GetTriggerLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var triggers = table.Triggers;
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
            {
                if (trigger.Name != null)
                    result[trigger.Name.LocalName] = trigger;
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseTrigger> GetTriggerLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetTriggerLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseTrigger>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseKey> GetUniqueKeyLookup(this IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var uniqueKeys = table.UniqueKeys;
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var key in uniqueKeys)
            {
                key.Name.IfSome(name => result[name.LocalName] = key);
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseKey> GetUniqueKeyLookup(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetUniqueKeyLookup(table);
            return new IdentifierResolvingDictionary<IDatabaseKey>(lookup, identifierResolver);
        }
    }
}
