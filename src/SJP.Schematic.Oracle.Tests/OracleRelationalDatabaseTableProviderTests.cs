using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleRelationalDatabaseTableProviderTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(connection, null, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver"));
    }

    [Test]
    public static void GetTable_GivenNullTableName_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        var tableProvider = new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
    }

    [Test]
    public static void GetResolvedTableNameStrict_GivenNullTableName_ThrowsArgNullException()
    {
        Assert.That(
            () => ProviderAccessor.Create().ResolveNameStrict(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void QualifyTableName_GivenNullTableName_ThrowsArgNullException()
    {
        Assert.That(
            () => ProviderAccessor.Create().Qualify(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void GenerateNotNullDefinition_GivenNullColumnName_ThrowsArgNullException()
    {
        Assert.That(
            () => ProviderAccessor.GenerateNotNull(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void GenerateNotNullDefinition_GivenEmptyOrWhiteSpaceColumnName_ThrowsArgumentException(string columnName)
    {
        Assert.That(
            () => ProviderAccessor.GenerateNotNull(columnName),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [Test]
    public static void LoadTriggersAsync_GivenNullTableName_ThrowsArgNullException()
    {
        Assert.That(
            () => ProviderAccessor.Create().LoadTriggers(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [TestCaseSource(nameof(TableAndCacheLoaders))]
    public static void TableAndCacheLoader_GivenNullTableName_ThrowsArgNullException(string loaderName)
    {
        var provider = ProviderAccessor.Create();

        Assert.That(
            () => provider.InvokeLoader(loaderName, null!, provider.Cache()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [TestCaseSource(nameof(TableAndCacheLoaders))]
    public static void TableAndCacheLoader_GivenNullQueryCache_ThrowsArgNullException(string loaderName)
    {
        Assert.That(
            () => ProviderAccessor.Create().InvokeLoader(loaderName, TableName, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("queryCache")
        );
    }

    [Test]
    public static void ForeignKeyReferenceCtor_GivenInvalidParentKeyType_ThrowsArgumentException()
    {
        Assert.That(
            () => ProviderAccessor.CreateForeignKeyReference((DatabaseKeyType)55, ReferentialAction.NoAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("ParentKeyType")
        );
    }

    [Test]
    public static void ForeignKeyReferenceCtor_GivenInvalidDeleteAction_ThrowsArgumentException()
    {
        Assert.That(
            () => ProviderAccessor.CreateForeignKeyReference(DatabaseKeyType.Primary, (ReferentialAction)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("DeleteAction")
        );
    }

    [TestCaseSource(nameof(QueryCacheLoaderParameters))]
    public static void QueryCacheCtor_GivenNullLoader_ThrowsArgNullException(string loaderName)
    {
        Assert.That(
            () => ProviderAccessor.CreateQueryCacheWithNullLoader(loaderName),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo(loaderName)
        );
    }

    [TestCaseSource(nameof(QueryCacheTableNameMethods))]
    public static void QueryCacheMethod_GivenNullTableName_ThrowsArgNullException(string methodName)
    {
        var provider = ProviderAccessor.Create();

        Assert.That(
            () => provider.InvokeCacheMethod(methodName, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void QueryCacheTryAddTableName_GivenNullResolvedTableName_ThrowsArgNullException()
    {
        var provider = ProviderAccessor.Create();

        Assert.That(
            () => provider.TryAddTableName(TableName, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("resolvedTableName")
        );
    }

    private static readonly string[] TableAndCacheLoaders =
    [
        "LoadTable",
        "LoadConstraintsAsync",
        "LoadIndexesAsync",
        "LoadChildKeysAsync",
        "LoadChecksAsync",
        "LoadParentKeysAsync",
        "LoadColumnsAsync",
    ];

    private static readonly string[] QueryCacheLoaderParameters =
    [
        "tableNameLoader",
        "columnLoader",
        "constraintLoader",
        "indexLoader",
        "foreignKeyLoader",
        "columnLookupLoader",
    ];

    private static readonly string[] QueryCacheTableNameMethods =
    [
        "GetTableNameAsync",
        "TryAddTableName",
        "GetColumnsAsync",
        "GetConstraintsAsync",
        "GetPrimaryKeyAsync",
        "GetUniqueKeysAsync",
        "GetIndexesAsync",
        "GetForeignKeysAsync",
        "GetColumnLookupAsync",
    ];

    // The provider's loaders, its query cache and ForeignKeyReference are all protected, so they are
    // only reachable through a derived provider — which is exactly the surface the guards exist for.
    // The query cache type is protected too, so it crosses back out of here as object.
    private sealed class ProviderAccessor : OracleRelationalDatabaseTableProvider
    {
        private readonly OracleTableQueryCache _cache;

        private ProviderAccessor()
            : base(Mock.Of<ISchematicConnection>(), Mock.Of<IIdentifierDefaults>(), Mock.Of<IIdentifierResolutionStrategy>())
        {
            _cache = CreateQueryCache(CancellationToken.None);
        }

        public static ProviderAccessor Create() => new();

        public static string GenerateNotNull(string columnName) => GenerateNotNullDefinition(columnName);

        public static object CreateForeignKeyReference(DatabaseKeyType parentKeyType, ReferentialAction deleteAction)
        {
            return new ForeignKeyReference(
                Mock.Of<IDatabaseKey>(),
                Identifier.CreateQualifiedIdentifier("test_schema", "parent_table"),
                "parent_key",
                parentKeyType,
                deleteAction
            );
        }

        public static object CreateQueryCacheWithNullLoader(string nullLoaderName)
        {
            bool IsNull(string loaderName) => string.Equals(nullLoaderName, loaderName, StringComparison.Ordinal);

            return new OracleTableQueryCache(
                IsNull("tableNameLoader") ? null! : UnusedCache<Option<Identifier>>(),
                IsNull("columnLoader") ? null! : UnusedCache<IReadOnlyList<IDatabaseColumn>>(),
                IsNull("constraintLoader") ? null! : UnusedCache<TableConstraints>(),
                IsNull("indexLoader") ? null! : UnusedCache<IReadOnlyCollection<IDatabaseIndex>>(),
                IsNull("foreignKeyLoader") ? null! : UnusedCache<IReadOnlyCollection<IDatabaseRelationalKey>>(),
                IsNull("columnLookupLoader") ? null! : UnusedCache<IReadOnlyDictionary<Identifier, IDatabaseColumn>>()
            );
        }

        // The constructor only null-checks its loaders, so the ones that are not under test never run.
        private static AsyncCache<Identifier, TValue, OracleTableQueryCache> UnusedCache<TValue>()
            => new(static (_, _, _) => Task.FromResult<TValue>(default!));

        public object Cache() => _cache;

        public Identifier Qualify(Identifier tableName) => QualifyTableName(tableName);

        public object ResolveNameStrict(Identifier tableName) => GetResolvedTableNameStrict(tableName, CancellationToken.None);

        public object LoadTriggers(Identifier tableName) => LoadTriggersAsync(tableName, CancellationToken.None);

        public bool TryAddTableName(Identifier tableName, Identifier resolvedTableName) => _cache.TryAddTableName(tableName, resolvedTableName);

        public object InvokeLoader(string loaderName, Identifier tableName, object queryCache)
        {
            var cache = (OracleTableQueryCache)queryCache;

            return loaderName switch
            {
                "LoadTable" => LoadTable(tableName, cache, CancellationToken.None),
                "LoadConstraintsAsync" => LoadConstraintsAsync(tableName, cache, CancellationToken.None),
                "LoadIndexesAsync" => LoadIndexesAsync(tableName, cache, CancellationToken.None),
                "LoadChildKeysAsync" => LoadChildKeysAsync(tableName, cache, CancellationToken.None),
                "LoadChecksAsync" => LoadChecksAsync(tableName, cache, CancellationToken.None),
                "LoadParentKeysAsync" => LoadParentKeysAsync(tableName, cache, CancellationToken.None),
                "LoadColumnsAsync" => LoadColumnsAsync(tableName, cache, CancellationToken.None),
                _ => throw new ArgumentException($"Unknown loader '{loaderName}'.", nameof(loaderName)),
            };
        }

        public object InvokeCacheMethod(string methodName, Identifier tableName)
        {
            return methodName switch
            {
                "GetTableNameAsync" => _cache.GetTableNameAsync(tableName, CancellationToken.None),
                "TryAddTableName" => _cache.TryAddTableName(tableName, TableName),
                "GetColumnsAsync" => _cache.GetColumnsAsync(tableName, CancellationToken.None),
                "GetConstraintsAsync" => _cache.GetConstraintsAsync(tableName, CancellationToken.None),
                "GetPrimaryKeyAsync" => _cache.GetPrimaryKeyAsync(tableName, CancellationToken.None),
                "GetUniqueKeysAsync" => _cache.GetUniqueKeysAsync(tableName, CancellationToken.None),
                "GetIndexesAsync" => _cache.GetIndexesAsync(tableName, CancellationToken.None),
                "GetForeignKeysAsync" => _cache.GetForeignKeysAsync(tableName, CancellationToken.None),
                "GetColumnLookupAsync" => _cache.GetColumnLookupAsync(tableName, CancellationToken.None),
                _ => throw new ArgumentException($"Unknown cache method '{methodName}'.", nameof(methodName)),
            };
        }
    }
}
