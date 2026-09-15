using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;
using StringHashSet = System.Collections.Generic.HashSet<string>;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore;

/// <summary>
/// The navigation properties declared on a single generated entity class.
/// </summary>
/// <param name="ParentKeyPropertyNames">Property names for the entity's foreign keys, in the same order as <see cref="IRelationalDatabaseTable.ParentKeys"/>.</param>
/// <param name="ChildKeyPropertyNames">Property names for the entity's child keys, in the same order as <see cref="IRelationalDatabaseTable.ChildKeys"/>.</param>
internal sealed record EntityNavigations(IReadOnlyList<string> ParentKeyPropertyNames, IReadOnlyList<string> ChildKeyPropertyNames);

/// <summary>
/// Describes how a foreign key is represented by navigation properties on the two entities it relates.
/// </summary>
/// <param name="DependentPropertyName">The property on the child entity that refers to the parent.</param>
/// <param name="PrincipalPropertyName">The property on the parent entity that refers to the child.</param>
/// <param name="IsOneToOne">Whether the parent's property is a single reference rather than a collection.</param>
internal sealed record RelationshipNavigations(string DependentPropertyName, string PrincipalPropertyName, bool IsOneToOne);

/// <summary>
/// Determines the navigation property names that entity classes receive, so that a generated
/// <c>DbContext</c> can be configured against exactly the members those classes declare.
/// </summary>
internal sealed class EFCoreNavigationResolver
{
    private readonly INameTranslator _nameTranslator;
    private readonly Dictionary<Identifier, IRelationalDatabaseTable> _tablesByName;
    // concurrent because a table generator shares one resolver across calls that may run in parallel
    private readonly ConcurrentDictionary<Identifier, EntityNavigations> _navigationsByTableName = new();
    // keyed by table instance rather than name, so a table sharing another's name never reads that table's keys
    private readonly ConcurrentDictionary<IRelationalDatabaseTable, ParentKeyRelationship[]> _parentKeyRelationships = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<IRelationalDatabaseTable, Dictionary<RelationshipSignature, List<int>>> _childKeyIndicesBySignature = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreNavigationResolver"/> class.
    /// </summary>
    /// <param name="nameTranslator">The name translator used to generate the entity classes.</param>
    /// <param name="tables">Every table that has an entity class generated for it.</param>
    /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/> or <paramref name="tables"/> is <see langword="null" />.</exception>
    public EFCoreNavigationResolver(INameTranslator nameTranslator, IEnumerable<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(nameTranslator);
        ArgumentNullException.ThrowIfNull(tables);

        _nameTranslator = nameTranslator;
        _tablesByName = [];
        // duplicate names cannot generate distinct classes anyway, so the first definition wins
        foreach (var table in tables)
            _tablesByName.TryAdd(table.Name, table);
    }

    /// <summary>
    /// Retrieves the navigation property names declared on the entity generated for a given table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>The navigation property names, in declaration order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    public EntityNavigations GetNavigations(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (_navigationsByTableName.TryGetValue(table.Name, out var cached))
            return cached;

        var className = _nameTranslator.TableToClassName(table.Name);

        // mirrors the declaration order used when generating the entity: columns, then parent keys, then child keys
        var usedNames = new StringHashSet(
            table.Columns.Select(c => _nameTranslator.ColumnToPropertyName(className, c.Name.LocalName)),
            StringComparer.Ordinal
        ) { className };

        var parentKeyPropertyNames = table.ParentKeys
            .Select(fk => UniqueNameGenerator.GenerateUniqueName(usedNames, _nameTranslator.TableToClassName(fk.ParentTable)))
            .ToList();
        var childKeyPropertyNames = table.ChildKeys
            .Select(ck => UniqueNameGenerator.GenerateUniqueName(usedNames, _nameTranslator.TableToClassName(ck.ChildTable).Pluralize()))
            .ToList();

        // navigation names are deterministic per table, so a concurrent computation for the same table yields equal names
        var navigations = new EntityNavigations(parentKeyPropertyNames, childKeyPropertyNames);
        return _navigationsByTableName.GetOrAdd(table.Name, navigations);
    }

    /// <summary>
    /// Determines the navigation properties that represent one of a table's foreign keys.
    /// </summary>
    /// <param name="table">The child table declaring the foreign key.</param>
    /// <param name="parentKeyIndex">The index of the foreign key within <see cref="IRelationalDatabaseTable.ParentKeys"/>.</param>
    /// <returns>The navigation properties on either side of the relationship.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parentKeyIndex"/> does not refer to a foreign key on <paramref name="table"/>.</exception>
    public RelationshipNavigations ResolveRelationship(IRelationalDatabaseTable table, int parentKeyIndex)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentOutOfRangeException.ThrowIfNegative(parentKeyIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(parentKeyIndex, table.ParentKeys.Count);

        var (relationalKey, signature, occurrence) = GetParentKeyRelationships(table)[parentKeyIndex];
        var dependentPropertyName = GetNavigations(table).ParentKeyPropertyNames[parentKeyIndex];
        var childKeyIndex = -1;

        // a table may declare the same relationship more than once, so match on position within the duplicates
        if (_tablesByName.TryGetValue(relationalKey.ParentTable, out var parentTable)
            && GetChildKeyIndicesBySignature(parentTable).TryGetValue(signature, out var childKeyIndices)
            && occurrence < childKeyIndices.Count)
        {
            childKeyIndex = childKeyIndices[occurrence];
        }

        // without a matching child key on the parent there is no generated navigation to refer to,
        // so fall back to the name the parent's entity would have received for a plain collection
        if (childKeyIndex < 0)
            return new RelationshipNavigations(dependentPropertyName, _nameTranslator.TableToClassName(table.Name).Pluralize(), false);

        return new RelationshipNavigations(
            dependentPropertyName,
            GetNavigations(parentTable!).ChildKeyPropertyNames[childKeyIndex],
            IsChildKeyUnique(relationalKey)
        );
    }

    // a heavily referenced table is consulted once for every foreign key that refers to it,
    // so the signatures on both sides are built once per table instead of on every lookup
    private ParentKeyRelationship[] GetParentKeyRelationships(IRelationalDatabaseTable table) =>
        _parentKeyRelationships.GetOrAdd(table, static t =>
        {
            var relationships = new ParentKeyRelationship[t.ParentKeys.Count];
            var occurrencesBySignature = new Dictionary<RelationshipSignature, int>();
            var index = 0;
            foreach (var parentKey in t.ParentKeys)
            {
                var signature = GetRelationshipSignature(parentKey);
                occurrencesBySignature.TryGetValue(signature, out var occurrence);
                occurrencesBySignature[signature] = occurrence + 1;
                relationships[index++] = new ParentKeyRelationship(parentKey, signature, occurrence);
            }

            return relationships;
        });

    private Dictionary<RelationshipSignature, List<int>> GetChildKeyIndicesBySignature(IRelationalDatabaseTable table) =>
        _childKeyIndicesBySignature.GetOrAdd(table, static t =>
        {
            var indicesBySignature = new Dictionary<RelationshipSignature, List<int>>();
            var index = 0;
            foreach (var childKey in t.ChildKeys)
            {
                var signature = GetRelationshipSignature(childKey);
                if (!indicesBySignature.TryGetValue(signature, out var indices))
                {
                    indices = [];
                    indicesBySignature.Add(signature, indices);
                }

                indices.Add(index++);
            }

            return indicesBySignature;
        });

    private static RelationshipSignature GetRelationshipSignature(IDatabaseRelationalKey relationalKey) =>
        new(
            relationalKey.ChildTable,
            string.Join(",", relationalKey.ChildKey.Columns.Select(static c => c.Name.LocalName)),
            relationalKey.ParentTable,
            string.Join(",", relationalKey.ParentKey.Columns.Select(static c => c.Name.LocalName))
        );

    /// <summary>
    /// Determines whether a relationship is one-to-one, i.e. whether the child key's columns are constrained to be unique.
    /// </summary>
    /// <param name="relationalKey">A foreign key relationship.</param>
    /// <returns><see langword="true" /> if the child key's columns are unique, otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="relationalKey"/> is <see langword="null" />.</exception>
    public bool IsChildKeyUnique(IDatabaseRelationalKey relationalKey)
    {
        ArgumentNullException.ThrowIfNull(relationalKey);

        return _tablesByName.TryGetValue(relationalKey.ChildTable, out var childTable)
            && IsChildKeyUnique(childTable, relationalKey.ChildKey);
    }

    private static bool IsChildKeyUnique(IRelationalDatabaseTable table, IDatabaseKey key)
    {
        var keyColumnNames = key.Columns.Select(static c => c.Name.LocalName).ToList();
        var matchesPkColumns = table.PrimaryKey
            .Match(
                pk => keyColumnNames.SequenceEqual(pk.Columns.Select(static c => c.Name.LocalName), StringComparer.Ordinal),
                static () => false
            );
        if (matchesPkColumns)
            return true;

        var matchesUkColumns = table.UniqueKeys
            .Any(uk => keyColumnNames.SequenceEqual(uk.Columns.Select(static c => c.Name.LocalName), StringComparer.Ordinal));
        if (matchesUkColumns)
            return true;

        return table.Indexes
            .Where(static i => i.IsUnique)
            .Any(i =>
            {
                var indexColumnExpressions = i.Columns
                    .Select(static ic => ic.DependentColumns.Select(static dc => dc.Name.LocalName).FirstOrDefault() ?? ic.Expression);
                return keyColumnNames.SequenceEqual(indexColumnExpressions, StringComparer.Ordinal);
            });
    }

    private readonly record struct RelationshipSignature(Identifier ChildTable, string ChildKeyColumns, Identifier ParentTable, string ParentKeyColumns);

    /// <summary>
    /// A foreign key together with its signature and how many earlier foreign keys on the same table share that signature.
    /// </summary>
    private readonly record struct ParentKeyRelationship(IDatabaseRelationalKey RelationalKey, RelationshipSignature Signature, int Occurrence);
}
