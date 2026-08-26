using System;
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
    private readonly Dictionary<Identifier, EntityNavigations> _navigationsByTableName = [];

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
        // duplicate names cannot generate distinct classes anyway, so the first definition wins
        _tablesByName = tables
            .GroupBy(static t => t.Name)
            .ToDictionary(static g => g.Key, static g => g.First());
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

        var navigations = new EntityNavigations(parentKeyPropertyNames, childKeyPropertyNames);
        _navigationsByTableName[table.Name] = navigations;

        return navigations;
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

        var parentKeys = table.ParentKeys.ToList();
        var relationalKey = parentKeys[parentKeyIndex];
        var dependentPropertyName = GetNavigations(table).ParentKeyPropertyNames[parentKeyIndex];
        var childKeyIndex = -1;

        if (_tablesByName.TryGetValue(relationalKey.ParentTable, out var parentTable))
        {
            // a table may declare the same relationship more than once, so match on position within the duplicates
            var signature = GetRelationshipSignature(relationalKey);
            var occurrence = parentKeys
                .Take(parentKeyIndex)
                .Count(fk => GetRelationshipSignature(fk) == signature);
            childKeyIndex = IndexOfOccurrence(parentTable.ChildKeys, signature, occurrence);
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

    private static int IndexOfOccurrence(IEnumerable<IDatabaseRelationalKey> relationalKeys, RelationshipSignature signature, int occurrence)
    {
        var index = 0;
        var seen = 0;
        foreach (var relationalKey in relationalKeys)
        {
            if (GetRelationshipSignature(relationalKey) == signature && seen++ == occurrence)
                return index;

            index++;
        }

        return -1;
    }

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
}
