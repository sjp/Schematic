using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    public class DbParameterAdapterTests
    {
        protected Mock<IDbDataParameter> ParameterMock => new Mock<IDbDataParameter>();

        [Test]
        public void Ctor_GivenNullParameter_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbParameterAdapter(null));
        }

        [Test]
        public void DbType_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var dbType = adapter.DbType;

            mock.VerifyGet(p => p.DbType);
        }

        [Test]
        public void DbType_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const DbType testType = DbType.Int32;
            adapter.DbType = testType;

            mock.VerifySet(p => p.DbType = testType);
        }

        [Test]
        public void Direction_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var direction = adapter.Direction;

            mock.VerifyGet(p => p.Direction);
        }

        [Test]
        public void Direction_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const ParameterDirection testDirection = ParameterDirection.Output;
            adapter.Direction = testDirection;

            mock.VerifySet(p => p.Direction = testDirection);
        }

        [Test]
        public void IsNullable_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var nullable = adapter.IsNullable;

            mock.VerifyGet(p => p.IsNullable);
        }

        [Test]
        public void ParameterName_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var paramName = adapter.ParameterName;

            mock.VerifyGet(p => p.ParameterName);
        }

        [Test]
        public void ParameterName_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const string paramName = "abc";
            adapter.ParameterName = paramName;

            mock.VerifySet(p => p.ParameterName = paramName);
        }

        [Test]
        public void Size_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var size = adapter.Size;

            mock.VerifyGet(p => p.Size);
        }

        [Test]
        public void Size_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const int size = 123;
            adapter.Size = size;

            mock.VerifySet(p => p.Size = size);
        }

        [Test]
        public void SourceColumn_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var columnName = adapter.SourceColumn;

            mock.VerifyGet(p => p.SourceColumn);
        }

        [Test]
        public void SourceColumn_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const string columnName = "abc";
            adapter.SourceColumn = columnName;

            mock.VerifySet(p => p.SourceColumn = columnName);
        }

        [Test]
        public void Value_PropertyGet_ReadsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            var value = adapter.Value;

            mock.VerifyGet(p => p.Value);
        }

        [Test]
        public void Value_PropertySet_SetsProvidedParameter()
        {
            var mock = ParameterMock;
            var adapter = new DbParameterAdapter(mock.Object);
            const string value = "abc";
            adapter.Value = value;

            mock.VerifySet(p => p.Value = value);
        }
    }
}
