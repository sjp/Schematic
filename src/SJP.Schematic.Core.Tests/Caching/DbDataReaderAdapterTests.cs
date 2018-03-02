using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal class DbDataReaderAdapterTests
    {
        protected Mock<IDataReader> ReaderMock => new Mock<IDataReader>();

        [Test]
        public void Ctor_GivenNullReader_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbDataReaderAdapter(null, CommandBehavior.CloseConnection));
        }

        [Test]
        public void Indexer_GetByIndex_ReadsProvidedCollection()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var value = adapter[index];

            mock.Verify(c => c[index]);
        }

        [Test]
        public void Indexer_GetByName_ReadsProvidedCollection()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const string name = "asd";
            var value = adapter[name];

            mock.Verify(c => c[name]);
        }

        [Test]
        public void Depth_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var depth = adapter.Depth;

            mock.VerifyGet(r => r.Depth);
        }

        [Test]
        public void FieldCount_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var fieldCount = adapter.FieldCount;

            mock.VerifyGet(r => r.FieldCount);
        }

        [Test]
        public void IsClosed_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var isClosed = adapter.IsClosed;

            mock.VerifyGet(r => r.IsClosed);
        }

        [Test]
        public void RecordsAffected_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var affected = adapter.RecordsAffected;

            mock.VerifyGet(r => r.RecordsAffected);
        }

        [Test]
        public void GetBoolean_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetBoolean(index);

            mock.Verify(r => r.GetBoolean(index));
        }

        [Test]
        public void GetByte_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetByte(index);

            mock.Verify(r => r.GetByte(index));
        }

        [Test]
        public void GetBytes_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            const int offset = 1;
            var buffer = new byte[] { };
            const int bufferOffset = 1;
            const int length = 10;
            var result = adapter.GetBytes(index, offset, buffer, bufferOffset, length);

            mock.Verify(r => r.GetBytes(index, offset, buffer, bufferOffset, length));
        }

        [Test]
        public void GetChar_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetChar(index);

            mock.Verify(r => r.GetChar(index));
        }

        [Test]
        public void GetChars_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            const int offset = 1;
            var buffer = new char[] { };
            const int bufferOffset = 1;
            const int length = 10;
            var result = adapter.GetChars(index, offset, buffer, bufferOffset, length);

            mock.Verify(r => r.GetChars(index, offset, buffer, bufferOffset, length));
        }

        [Test]
        public void GetDataTypeName_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetDataTypeName(index);

            mock.Verify(r => r.GetDataTypeName(index));
        }

        [Test]
        public void GetDateTime_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetDateTime(index);

            mock.Verify(r => r.GetDateTime(index));
        }

        [Test]
        public void GetDecimal_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetDecimal(index);

            mock.Verify(r => r.GetDecimal(index));
        }

        [Test]
        public void GetDouble_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetDouble(index);

            mock.Verify(r => r.GetDouble(index));
        }

        [Test]
        public void GetFieldType_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetFieldType(index);

            mock.Verify(r => r.GetFieldType(index));
        }

        [Test]
        public void GetFloat_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetFloat(index);

            mock.Verify(r => r.GetFloat(index));
        }

        [Test]
        public void GetGuid_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetGuid(index);

            mock.Verify(r => r.GetGuid(index));
        }

        [Test]
        public void GetInt16_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetInt16(index);

            mock.Verify(r => r.GetInt16(index));
        }

        [Test]
        public void GetInt32_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetInt32(index);

            mock.Verify(r => r.GetInt32(index));
        }

        [Test]
        public void GetInt64_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetInt64(index);

            mock.Verify(r => r.GetInt64(index));
        }

        [Test]
        public void GetName_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetName(index);

            mock.Verify(r => r.GetName(index));
        }

        [Test]
        public void GetOrdinal_GetByName_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const string paramName = "asd";
            var result = adapter.GetOrdinal(paramName);

            mock.Verify(r => r.GetOrdinal(paramName));
        }

        [Test]
        public void GetString_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetString(index);

            mock.Verify(r => r.GetString(index));
        }

        [Test]
        public void GetValue_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.GetValue(index);

            mock.Verify(r => r.GetValue(index));
        }

        [Test]
        public void GetValues_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var values = new object[] { };
            var result = adapter.GetValues(values);

            mock.Verify(r => r.GetValues(values));
        }

        [Test]
        public void IsDBNull_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            var result = adapter.IsDBNull(index);

            mock.Verify(r => r.IsDBNull(index));
        }

        [Test]
        public void NextResult_WhenInvoked_CallsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            adapter.NextResult();

            mock.Verify(r => r.NextResult());
        }

        [Test]
        public void Read_WhenInvoked_CallsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            adapter.Read();

            mock.Verify(r => r.Read());
        }

        [Test]
        public void GetSchemaTable_WhenInvoked_ReturnsNull()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);

            var table = adapter.GetSchemaTable();
            Assert.IsNull(table);
        }

        [Test]
        public void HasRows_PropertyGet_ReturnsTrueIfReadFromProvidedReader()
        {
            var mock = ReaderMock;
            mock.Setup(m => m.Read()).Returns(true);

            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var hasRead = adapter.Read();

            Assert.IsTrue(adapter.HasRows);
        }

        [Test]
        public void HasRows_PropertyGet_ReturnsFalseIfNotReadFromProvidedReader()
        {
            var mock = ReaderMock;
            mock.Setup(m => m.Read()).Returns(false);

            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var hasRead = adapter.Read();

            Assert.IsFalse(adapter.HasRows);
        }
    }
}
