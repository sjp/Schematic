﻿using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseViewProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerDatabaseViewProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();

            Assert.That(() => new SqlServerDatabaseViewProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var viewProvider = new SqlServerDatabaseViewProvider(connection, identifierDefaults);

            Assert.That(() => viewProvider.GetView(null), Throws.ArgumentNullException);
        }
    }
}
