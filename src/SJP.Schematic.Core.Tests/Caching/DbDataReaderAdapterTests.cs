using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class DbDataReaderAdapterTests
    {
        private static Mock<IDataReader> ReaderMock => new Mock<IDataReader>();

        [Test]
        public static void Ctor_GivenNullReader_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbDataReaderAdapter(null, CommandBehavior.CloseConnection));
        }

        [Test]
        public static void Indexer_GetByIndex_ReadsProvidedCollection()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter[index];

            mock.Verify(c => c[index]);
        }

        [Test]
        public static void Indexer_GetByName_ReadsProvidedCollection()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const string name = "test";
            _ = adapter[name];

            mock.Verify(c => c[name]);
        }

        [Test]
        public static void Depth_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.Depth;

            mock.VerifyGet(r => r.Depth);
        }

        [Test]
        public static void FieldCount_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.FieldCount;

            mock.VerifyGet(r => r.FieldCount);
        }

        [Test]
        public static void IsClosed_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.IsClosed;

            mock.VerifyGet(r => r.IsClosed);
        }

        [Test]
        public static void RecordsAffected_PropertyGet_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.RecordsAffected;

            mock.VerifyGet(r => r.RecordsAffected);
        }

        [Test]
        public static void GetBoolean_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetBoolean(index);

            mock.Verify(r => r.GetBoolean(index));
        }

        [Test]
        public static void GetByte_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetByte(index);

            mock.Verify(r => r.GetByte(index));
        }

        [Test]
        public static void GetBytes_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            const int offset = 1;
            var buffer = Array.Empty<byte>();
            const int bufferOffset = 1;
            const int length = 10;
            _ = adapter.GetBytes(index, offset, buffer, bufferOffset, length);

            mock.Verify(r => r.GetBytes(index, offset, buffer, bufferOffset, length));
        }

        [Test]
        public static void GetChar_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetChar(index);

            mock.Verify(r => r.GetChar(index));
        }

        [Test]
        public static void GetChars_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            const int offset = 1;
            var buffer = Array.Empty<char>();
            const int bufferOffset = 1;
            const int length = 10;
            _ = adapter.GetChars(index, offset, buffer, bufferOffset, length);

            mock.Verify(r => r.GetChars(index, offset, buffer, bufferOffset, length));
        }

        [Test]
        public static void GetDataTypeName_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetDataTypeName(index);

            mock.Verify(r => r.GetDataTypeName(index));
        }

        [Test]
        public static void GetDateTime_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetDateTime(index);

            mock.Verify(r => r.GetDateTime(index));
        }

        [Test]
        public static void GetDecimal_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetDecimal(index);

            mock.Verify(r => r.GetDecimal(index));
        }

        [Test]
        public static void GetDouble_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetDouble(index);

            mock.Verify(r => r.GetDouble(index));
        }

        [Test]
        public static void GetFieldType_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetFieldType(index);

            mock.Verify(r => r.GetFieldType(index));
        }

        [Test]
        public static void GetFloat_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetFloat(index);

            mock.Verify(r => r.GetFloat(index));
        }

        [Test]
        public static void GetGuid_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetGuid(index);

            mock.Verify(r => r.GetGuid(index));
        }

        [Test]
        public static void GetInt16_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetInt16(index);

            mock.Verify(r => r.GetInt16(index));
        }

        [Test]
        public static void GetInt32_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetInt32(index);

            mock.Verify(r => r.GetInt32(index));
        }

        [Test]
        public static void GetInt64_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetInt64(index);

            mock.Verify(r => r.GetInt64(index));
        }

        [Test]
        public static void GetName_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetName(index);

            mock.Verify(r => r.GetName(index));
        }

        [Test]
        public static void GetOrdinal_GetByName_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const string paramName = "test";
            _ = adapter.GetOrdinal(paramName);

            mock.Verify(r => r.GetOrdinal(paramName));
        }

        [Test]
        public static void GetString_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetString(index);

            mock.Verify(r => r.GetString(index));
        }

        [Test]
        public static void GetValue_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.GetValue(index);

            mock.Verify(r => r.GetValue(index));
        }

        [Test]
        public static void GetValues_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            var values = Array.Empty<object>();
            _ = adapter.GetValues(values);

            mock.Verify(r => r.GetValues(values));
        }

        [Test]
        public static void IsDBNull_GetByIndex_ReadsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            const int index = 1;
            _ = adapter.IsDBNull(index);

            mock.Verify(r => r.IsDBNull(index));
        }

        [Test]
        public static void NextResult_WhenInvoked_CallsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            adapter.NextResult();

            mock.Verify(r => r.NextResult());
        }

        [Test]
        public static void Read_WhenInvoked_CallsProvidedReader()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            adapter.Read();

            mock.Verify(r => r.Read());
        }

        [Test]
        public static void GetSchemaTable_WhenInvoked_ReturnsNull()
        {
            var mock = ReaderMock;
            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);

            var table = adapter.GetSchemaTable();
            Assert.IsNull(table);
        }

        [Test]
        public static void HasRows_PropertyGet_ReturnsTrueIfReadFromProvidedReader()
        {
            var mock = ReaderMock;
            mock.Setup(m => m.Read()).Returns(true);

            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.Read();

            Assert.IsTrue(adapter.HasRows);
        }

        [Test]
        public static void HasRows_PropertyGet_ReturnsFalseIfNotReadFromProvidedReader()
        {
            var mock = ReaderMock;
            mock.Setup(m => m.Read()).Returns(false);

            var adapter = new DbDataReaderAdapter(mock.Object, CommandBehavior.Default);
            _ = adapter.Read();

            Assert.IsFalse(adapter.HasRows);
        }
    }
}
