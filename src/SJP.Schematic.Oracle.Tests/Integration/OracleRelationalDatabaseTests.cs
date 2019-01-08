﻿using NUnit.Framework;
using SJP.Schematic.Core;

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
            Assert.IsNotNull(Database.DefaultSchema);
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
