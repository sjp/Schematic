using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbParameterCollection"/> from an <see cref="IDataParameterCollection"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbParameterCollectionAdapter : DbParameterCollection
    {
        public DbParameterCollectionAdapter(IDataParameterCollection collection)
        {
            InnerCollection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        protected IDataParameterCollection InnerCollection { get; }

        public override int Count => InnerCollection.Count;

        public override object SyncRoot => InnerCollection.SyncRoot;

        public override int Add(object value) => InnerCollection.Add(value);

        public override void AddRange(Array values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            for (var i = 0; i < values.Length; i++)
                Add(values.GetValue(i));
        }

        public override void Clear() => InnerCollection.Clear();

        public override bool Contains(object value) => InnerCollection.Contains(value);

        public override bool Contains(string value) => InnerCollection.Contains(value);

        public override void CopyTo(Array array, int index) => InnerCollection.CopyTo(array, index);

        public override IEnumerator GetEnumerator() => InnerCollection.GetEnumerator();

        public override int IndexOf(object value) => InnerCollection.IndexOf(value);

        public override int IndexOf(string parameterName) => InnerCollection.IndexOf(parameterName);

        public override void Insert(int index, object value) => InnerCollection.Insert(index, value);

        public override void Remove(object value) => InnerCollection.Remove(value);

        public override void RemoveAt(int index) => InnerCollection.RemoveAt(index);

        public override void RemoveAt(string parameterName) => InnerCollection.RemoveAt(parameterName);

        protected override DbParameter GetParameter(int index) => InnerCollection[index] as DbParameter;

        protected override DbParameter GetParameter(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
                throw new KeyNotFoundException($"The parameter name '{ parameterName }' does not exist");

            return InnerCollection[index] as DbParameter;
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            RemoveAt(index);
            Insert(index, value);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
                throw new KeyNotFoundException($"The parameter name '{ parameterName }' does not exist");

            RemoveAt(index);
            Insert(index, value);
        }
    }
}