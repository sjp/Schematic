using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleRelationalDatabaseTests : OracleTest
    {
        private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection, IdentifierDefaults, IdentifierResolver);

        [Test]
        public void Database_PropertyGet_ShouldNotBeEmpty()
        {
            var isEmpty = string.IsNullOrWhiteSpace(Database.DatabaseName);
            Assert.IsFalse(isEmpty);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            // FIXME this could change when connected to a different database
            Assert.AreEqual("SYSTEM", Database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.IsNotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }
    }
}
