using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                if (check.Name != null)
                    result[check.Name.LocalName] = check;
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> GetCheckLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetCheckLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> GetCheckLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var checks = await table.ChecksAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
            {
                if (check.Name != null)
                    result[check.Name.LocalName] = check;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> GetCheckLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetCheckLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> GetCheckLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetCheckLookupAsync(table, cancellationToken).ConfigureAwait(false);
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetColumnLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var columns = await table.ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns)
            {
                if (column.Name != null)
                    result[column.Name.LocalName] = column;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetColumnLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetColumnLookupAsync(table, cancellationToken).ConfigureAwait(false);
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetIndexLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var indexes = await table.IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
            {
                if (index.Name != null)
                    result[index.Name.LocalName] = index;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetIndexLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetIndexLookupAsync(table, cancellationToken).ConfigureAwait(false);
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
                if (parentKey.ChildKey.Name != null)
                    result[parentKey.ChildKey.Name.LocalName] = parentKey;
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> GetParentKeyLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetParentKeyLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> GetParentKeyLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var parentKeys = await table.ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
            {
                if (parentKey.ChildKey.Name != null)
                    result[parentKey.ChildKey.Name.LocalName] = parentKey;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> GetParentKeyLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetParentKeyLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> GetParentKeyLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetParentKeyLookupAsync(table, cancellationToken).ConfigureAwait(false);
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> GetTriggerLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetTriggerLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> GetTriggerLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var triggers = await table.TriggersAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
            {
                if (trigger.Name != null)
                    result[trigger.Name.LocalName] = trigger;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> GetTriggerLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetTriggerLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> GetTriggerLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetTriggerLookupAsync(table, cancellationToken).ConfigureAwait(false);
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
                if (key.Name != null)
                    result[key.Name.LocalName] = key;
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

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> GetUniqueKeyLookupAsync(this IRelationalDatabaseTable table, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetUniqueKeyLookupAsyncCore(table, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> GetUniqueKeyLookupAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var uniqueKeys = await table.UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var key in uniqueKeys)
            {
                if (key.Name != null)
                    result[key.Name.LocalName] = key;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> GetUniqueKeyLookupAsync(this IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetUniqueKeyLookupAsyncCore(table, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> GetUniqueKeyLookupAsyncCore(IRelationalDatabaseTable table, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetUniqueKeyLookupAsync(table, cancellationToken).ConfigureAwait(false);
            return new IdentifierResolvingDictionary<IDatabaseKey>(lookup, identifierResolver);
        }
    }
}
