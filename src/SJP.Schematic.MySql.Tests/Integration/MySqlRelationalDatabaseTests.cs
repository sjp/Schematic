﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal sealed class MySqlRelationalDatabaseTests : MySqlTest
    {
        private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual(Connection.Database, Database.DefaultSchema);
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
