using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

// Which catalog queries a provider runs is decided once from the server's catalog features, and every
// later table load depends on that decision. A failure while reading those features must not stick.
internal sealed class MySqlRelationalDatabaseTableCatalogFeatureTests : MySqlTest
{
    [OneTimeSetUp]
    public Task Init() => DbConnection.ExecuteAsync("create table catalog_feature_check_test ( feature_value int, check (feature_value > 0) )", TestContext.CurrentContext.CancellationToken);

    [OneTimeTearDown]
    public Task CleanUp() => DropTablesAsync("catalog_feature_check_test");

    [Test]
    public async Task LoadChecksAsync_AfterCatalogFeatureLoadFailed_LoadsChecksOnceConnectionsSucceed()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var connectionFactory = new SwitchableFailureConnectionFactory(Config.ConnectionFactory) { IsFailing = true };
        var tableProvider = new ChecksTableProvider(new SchematicConnection(connectionFactory, Dialect), IdentifierDefaults);
        var tableName = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Schema, "catalog_feature_check_test");

        Assert.That(() => tableProvider.GetChecksAsync(tableName, cancellationToken), Throws.TypeOf<SimulatedConnectionFailureException>());

        connectionFactory.IsFailing = false;
        var checks = await tableProvider.GetChecksAsync(tableName, cancellationToken);

        Assert.That(checks, Has.Exactly(1).Items);
    }

    private sealed class ChecksTableProvider : MySqlRelationalDatabaseTableProvider
    {
        public ChecksTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
            : base(connection, identifierDefaults)
        {
        }

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> GetChecksAsync(Identifier tableName, CancellationToken cancellationToken)
            => LoadChecksAsync(tableName, cancellationToken);
    }
}
