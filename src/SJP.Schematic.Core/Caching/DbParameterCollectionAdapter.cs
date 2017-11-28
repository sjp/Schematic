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
        /// <summary>
        /// Creates an instance of <see cref="DbParameterCollectionAdapter"/> to wrap an <see cref="IDataParameterCollection"/> as a <see cref="DbParameterCollection"/>.
        /// </summary>
        /// <param name="collection">An <see cref="IDataParameterCollection"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c>.</exception>
        public DbParameterCollectionAdapter(IDataParameterCollection collection)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// The <see cref="IDbDataParameter"/> instance that is being wrapped as a <see cref="DbParameter"/>.
        /// </summary>
        protected IDataParameterCollection Collection { get; }

        /// <summary>
        /// Specifies the number of items in the collection.
        /// </summary>
        public override int Count => Collection.Count;

        /// <summary>
        /// Specifies the <see cref="Object"/> to be used to synchronize access to the collection.
        /// </summary>
        public override object SyncRoot => Collection.SyncRoot;

        /// <summary>
        /// Adds the specified <see cref="DbParameter"/> object to the <see cref="DbParameterCollection"/>.
        /// </summary>
        /// <param name="value">The <see cref="DbParameter.Value"/> of the <see cref="DbParameter"/> to add to the collection.</param>
        /// <returns>The index of the <see cref="DbParameter"/> object in the collection.</returns>
        public override int Add(object value) => Collection.Add(value);

        /// <summary>
        /// Adds an array of items with the specified values to the <see cref="DbParameterCollection"/>.
        /// </summary>
        /// <param name="values">An array of values of type <see cref="DbParameter"/> to add to the collection.</param>
        public override void AddRange(Array values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            for (var i = 0; i < values.Length; i++)
                Add(values.GetValue(i));
        }

        /// <summary>
        /// Removes all <see cref="DbParameter"/> values from the <see cref="DbParameterCollection"/>.
        /// </summary>
        public override void Clear() => Collection.Clear();

        /// <summary>
        /// Indicates whether a <see cref="DbParameter"/> with the specified <see cref="DbParameter.Value"/> is contained in the collection.
        /// </summary>
        /// <param name="value">The <see cref="DbParameter.Value"/> of the <see cref="DbParameter"/> to look for in the collection.</param>
        /// <returns></returns>
        public override bool Contains(object value) => Collection.Contains(value);

        /// <summary>
        /// Indicates whether a <see cref="DbParameter"/> with the specified name exists in the collection.
        /// </summary>
        /// <param name="value">The name of the <see cref="DbParameter"/> to look for in the collection.</param>
        /// <returns><c>true</c> if the <see cref="DbParameter"/> is in the collection; otherwise <c>false</c>.</returns>
        public override bool Contains(string value) => Collection.Contains(value);

        /// <summary>
        /// Copies an array of items to the collection starting at the specified index.
        /// </summary>
        /// <param name="array">The array of items to copy to the collection.</param>
        /// <param name="index">The index in the collection to copy the items.</param>
        public override void CopyTo(Array array, int index) => Collection.CopyTo(array, index);

        /// <summary>
        /// Exposes the <see cref="GetEnumerator"/> method, which supports a simple iteration over a collection by a .NET Framework data provider.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
        public override IEnumerator GetEnumerator() => Collection.GetEnumerator();

        /// <summary>
        /// Returns the index of the specified <see cref="DbParameter"/> object.
        /// </summary>
        /// <param name="value">The <see cref="DbParameter"/> object in the collection.</param>
        /// <returns>The index of the specified <see cref="DbParameter"/> object.</returns>
        public override int IndexOf(object value) => Collection.IndexOf(value);

        /// <summary>
        /// Returns the index of the <see cref="DbParameter"/> object with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DbParameter"/> object in the collection.</param>
        /// <returns>The index of the <see cref="DbParameter"/> object with the specified name.</returns>
        public override int IndexOf(string parameterName) => Collection.IndexOf(parameterName);

        /// <summary>
        /// Inserts the specified index of the <see cref="DbParameter"/> object with the specified name into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the <see cref="DbParameter"/> object.</param>
        /// <param name="value">The <see cref="DbParameter"/> object to insert into the collection.</param>
        public override void Insert(int index, object value) => Collection.Insert(index, value);

        /// <summary>
        /// Removes the specified <see cref="DbParameter"/> object from the collection.
        /// </summary>
        /// <param name="value">The <see cref="DbParameter"/> object to remove.</param>
        public override void Remove(object value) => Collection.Remove(value);

        /// <summary>
        /// Removes the <see cref="DbParameter"/> object at the specified from the collection.
        /// </summary>
        /// <param name="index">The index where the <see cref="DbParameter"/> object is located.</param>
        public override void RemoveAt(int index) => Collection.RemoveAt(index);

        /// <summary>
        /// Removes the <see cref="DbParameter"/> object with the specified name from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DbParameter"/> object to remove.</param>
        public override void RemoveAt(string parameterName) => Collection.RemoveAt(parameterName);

        /// <summary>
        /// Returns the <see cref="DbParameter"/> object at the specified index in the collection.
        /// </summary>
        /// <param name="index">The index of the <see cref="DbParameter"/> in the collection.</param>
        /// <returns>The <see cref="DbParameter"/> object at the specified index in the collection.</returns>
        protected override DbParameter GetParameter(int index) => Collection[index] as DbParameter;

        /// <summary>
        /// Returns <see cref="DbParameter"/> the object with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DbParameter"/> in the collection.</param>
        /// <returns>The <see cref="DbParameter"/> the object with the specified name.</returns>
        protected override DbParameter GetParameter(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
                throw new KeyNotFoundException($"The parameter name '{ parameterName }' does not exist");

            return Collection[index] as DbParameter;
        }

        /// <summary>
        /// Sets the <see cref="DbParameter"/> object at the specified index to a new value.
        /// </summary>
        /// <param name="index">The index where the <see cref="DbParameter"/> object is located.</param>
        /// <param name="value">The new <see cref="DbParameter"/> value.</param>
        protected override void SetParameter(int index, DbParameter value)
        {
            RemoveAt(index);
            Insert(index, value);
        }

        /// <summary>
        /// Sets the <see cref="DbParameter"/> object with the specified name to a new value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="DbParameter"/> object in the collection.</param>
        /// <param name="value">The new <see cref="DbParameter"/> value.</param>
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