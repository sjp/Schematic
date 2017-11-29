using NUnit.Framework;
using Moq;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    public class DbParameterCollectionAdapterTests
    {
        protected Mock<IDataParameterCollection> CollectionMock => new Mock<IDataParameterCollection>();

        [Test]
        public void Ctor_GivenNullParameterCollection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbParameterCollectionAdapter(null));
        }

        [Test]
        public void Indexer_GetByIndex_ReadsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var index = adapter[1];

            mock.Verify(c => c[1]);
        }

        [Test]
        public void Count_PropertyGet_ReadsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var count = adapter.Count;

            mock.Verify(c => c.Count);
        }

        [Test]
        public void SyncRoot_PropertyGet_ReadsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var dbType = adapter.SyncRoot;

            mock.Verify(p => p.SyncRoot);
        }

        [Test]
        public void Add_GivenObjectInput_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var input = new object();
            adapter.Add(input);

            mock.Verify(p => p.Add(input));
        }

        [Test]
        public void AddRange_GivenObjectInput_CallsProvidedCollectionAddMethod()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var obj = new object();
            var input = new[] { obj };
            adapter.AddRange(input);

            mock.Verify(p => p.Add(obj));
        }

        [Test]
        public void AddRange_GivenNullInput_ThrowsArgumentNullException()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);

            Assert.Throws<ArgumentNullException>(() => adapter.AddRange(null));
        }

        [Test]
        public void Clear_WhenInvoked_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            adapter.Clear();

            mock.Verify(p => p.Clear());
        }

        [Test]
        public void Contains_WhenGivenValue_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var value = new object();
            var contains = adapter.Contains(value);

            mock.Verify(p => p.Contains(value));
        }

        [Test]
        public void Contains_WhenGivenParameterName_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            const string paramName = "asd";
            var contains = adapter.Contains(paramName);

            mock.Verify(p => p.Contains(paramName));
        }

        [Test]
        public void CopyTo_WhenInvoked_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var array = new[] { new object() };
            const int index = 1;
            adapter.CopyTo(array, index);

            mock.Verify(p => p.CopyTo(array, index));
        }

        [Test]
        public void GetEnumerator_WhenInvoked_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var enumerator = adapter.GetEnumerator();

            mock.Verify(p => p.GetEnumerator());
        }

        [Test]
        public void IndexOf_WhenInvokedByValue_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var arg = new object();
            var index = adapter.IndexOf(arg);

            mock.Verify(p => p.IndexOf(arg));
        }

        [Test]
        public void IndexOf_WhenInvokedByParameterName_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            const string paramName = "abc";
            var index = adapter.IndexOf(paramName);

            mock.Verify(p => p.IndexOf(paramName));
        }

        [Test]
        public void Insert_WhenInvoked_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            const int index = 1;
            var value = new object();
            adapter.Insert(index, value);

            mock.Verify(p => p.Insert(index, value));
        }

        [Test]
        public void Remove_WhenInvoked_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            var arg = new object();
            adapter.Remove(arg);

            mock.Verify(p => p.Remove(arg));
        }

        [Test]
        public void RemoveAt_WhenInvokedByIndex_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            const int index = 1;
            adapter.RemoveAt(index);

            mock.Verify(p => p.RemoveAt(index));
        }

        [Test]
        public void RemoveAt_WhenInvokedByName_CallsProvidedCollection()
        {
            var mock = CollectionMock;
            var adapter = new DbParameterCollectionAdapter(mock.Object);
            const string columnName = "abc";
            adapter.RemoveAt(columnName);

            mock.Verify(p => p.RemoveAt(columnName));
        }

        [Test]
        public void GetParameter_WhenInvokedByIndex_CallsProvidedCollectionIndexer()
        {
            var mock = CollectionMock;
            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            const int index = 1;
            var value = adapter.GetInnerParameter(index);

            mock.Verify(p => p[index]);
        }

        [Test]
        public void GetParameter_WhenInvokedByName_CallsProvidedCollectionIndexer()
        {
            const string paramName = "asd";
            const int index = 3;
            var mock = CollectionMock;
            mock.Setup(m => m.IndexOf(paramName)).Returns(index);

            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            var value = adapter.GetInnerParameter(paramName);

            mock.Verify(p => p[index]);
        }

        [Test]
        public void GetParameter_WhenInvokedByNameAndNotFound_ThrowsKeyNotFoundException()
        {
            const string paramName = "asd";
            const int index = -1;
            var mock = CollectionMock;
            mock.Setup(m => m.IndexOf(paramName)).Returns(index);

            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            Assert.Throws<KeyNotFoundException>(() => adapter.GetInnerParameter(paramName));
        }

        [Test]
        public void SetParameter_WhenInvokedByIndex_CallsProvidedCollectionIndexer()
        {
            var mock = CollectionMock;
            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            const int index = 1;
            var value = new object() as DbParameter;
            adapter.SetInnerParameter(index, value);

            mock.Verify(p => p.RemoveAt(index));
            mock.Verify(p => p.Insert(index, value));
        }

        [Test]
        public void SetParameter_WhenInvokedByName_CallsProvidedCollectionIndexer()
        {
            const string paramName = "asd";
            const int index = 3;
            var mock = CollectionMock;
            mock.Setup(m => m.IndexOf(paramName)).Returns(index);

            var value = new object() as DbParameter;
            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            adapter.SetInnerParameter(paramName, value);

            mock.Verify(p => p.RemoveAt(index));
            mock.Verify(p => p.Insert(index, value));
        }

        [Test]
        public void SetParameter_WhenInvokedByNameAndNotFound_ThrowsKeyNotFoundException()
        {
            const string paramName = "asd";
            const int index = -1;
            var mock = CollectionMock;
            mock.Setup(m => m.IndexOf(paramName)).Returns(index);

            var value = new object() as DbParameter;
            var adapter = new FakeDbParameterCollectionAdapter(mock.Object);
            Assert.Throws<KeyNotFoundException>(() => adapter.SetInnerParameter(paramName, value));
        }

        protected class FakeDbParameterCollectionAdapter : DbParameterCollectionAdapter
        {
            public FakeDbParameterCollectionAdapter(IDataParameterCollection collection)
                : base(collection)
            {
            }

            public DbParameter GetInnerParameter(int index) => base.GetParameter(index);

            public DbParameter GetInnerParameter(string parameterName) => base.GetParameter(parameterName);

            public void SetInnerParameter(int index, DbParameter value) => base.SetParameter(index, value);

            public void SetInnerParameter(string parameterName, DbParameter value) => base.SetParameter(parameterName, value);
        }
    }
}
