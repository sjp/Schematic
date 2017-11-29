using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using EnumsNET;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbDataReader"/> from an <see cref="IDataReader"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbDataReaderAdapter : DbDataReader
    {
        /// <summary>
        /// Creates an instance of <see cref="DbDataReaderAdapter"/> to wrap an <see cref="IDataReader"/> as a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="commandBehavior"></param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public DbDataReaderAdapter(IDataReader reader, CommandBehavior commandBehavior)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            Behavior = commandBehavior;
        }

        /// <summary>
        /// The <see cref="IDataReader"/> instance that is being wrapped as a <see cref="DbDataReader"/>.
        /// </summary>
        protected IDataReader Reader { get; }

        /// <summary>
        /// Controls which operation will be applied to the database.
        /// </summary>
        protected CommandBehavior Behavior { get; }

        /// <summary>
        /// Gets the value of the specified column as an instance of Object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object this[int ordinal] => Reader[ordinal];

        /// <summary>
        /// Gets the value of the specified column as an instance of Object.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The value of the specified column.</returns>
        public override object this[string name] => Reader[name];

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        public override int Depth => Reader.Depth;

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public override int FieldCount => Reader.FieldCount;

        /// <summary>
        /// Gets a value that indicates whether this <see cref="DbDataReader"/> contains one or more rows.
        /// </summary>
        public override bool HasRows => _hasRows;

        /// <summary>
        /// Gets a value indicating whether the <see cref="DbDataReader"/> is closed.
        /// </summary>
        public override bool IsClosed => Reader.IsClosed;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public override int RecordsAffected => Reader.RecordsAffected;

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override bool GetBoolean(int ordinal) => Reader.GetBoolean(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override byte GetByte(int ordinal) => Reader.GetByte(ordinal);

        /// <summary>
        /// Reads a stream of bytes from the specified column, starting at location indicated by <c>dataOffset</c>, into the buffer, starting at the location indicated by <c>bufferOffset</c>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => Reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        /// Gets the value of the specified column as a single character.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override char GetChar(int ordinal) => Reader.GetChar(ordinal);

        /// <summary>
        /// Reads a stream of characters from the specified column, starting at location indicated by <c>dataOffset</c>, into the buffer, starting at the location indicated by <c>bufferOffset</c>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => Reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        /// Gets name of the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>A string representing the name of the data type.</returns>
        public override string GetDataTypeName(int ordinal) => Reader.GetDataTypeName(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override DateTime GetDateTime(int ordinal) => Reader.GetDateTime(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Decimal"/> object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override decimal GetDecimal(int ordinal) => Reader.GetDecimal(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating point number.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override double GetDouble(int ordinal) => Reader.GetDouble(ordinal);

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the rows in the data reader.</returns>
        public override IEnumerator GetEnumerator() => new DbEnumerator(this, Behavior.CommonFlags(CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection);

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the specified column.</returns>
        public override Type GetFieldType(int ordinal) => Reader.GetFieldType(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a single-precision floating point number.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override float GetFloat(int ordinal) => Reader.GetFloat(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a globally-unique identifier(GUID).
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override Guid GetGuid(int ordinal) => Reader.GetGuid(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override short GetInt16(int ordinal) => Reader.GetInt16(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override int GetInt32(int ordinal) => Reader.GetInt32(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override long GetInt64(int ordinal) => Reader.GetInt64(ordinal);

        /// <summary>
        /// Gets the name of the column, given the zero-based column ordinal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the specified column.</returns>
        public override string GetName(int ordinal) => Reader.GetName(ordinal);

        /// <summary>
        /// Gets the column ordinal given the name of the column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
        public override int GetOrdinal(string name) => Reader.GetOrdinal(name);

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="String"/>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override string GetString(int ordinal) => Reader.GetString(ordinal);

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="Object"/>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetValue(int ordinal) => Reader.GetValue(ordinal);

        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <param name="values">An array of <see cref="Object"/> into which to copy the attribute columns.</param>
        /// <returns>The number of instances of <see cref="Object"/> in the array.</returns>
        public override int GetValues(object[] values) => Reader.GetValues(values);

        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns><c>true</c> if the specified column is equivalent to <see cref="DBNull"/>; otherwise <c>false</c>.</returns>
        public override bool IsDBNull(int ordinal) => Reader.IsDBNull(ordinal);

        /// <summary>
        /// Advances the reader to the next result when reading the results of a batch of statements.
        /// </summary>
        /// <returns><c>true</c> if there are more result sets; otherwise <c>false</c>.</returns>
        public override bool NextResult()
        {
            _hasRows = false;
            return Reader.NextResult();
        }

        /// <summary>
        /// Advances the reader to the next record in a result set.
        /// </summary>
        /// <returns><c>true</c> if there are more rows; otherwise <c>false</c>.</returns>
        public override bool Read()
        {
            var hasRow = Reader.Read();

            // set flag to ensure that we detect whether rows are present
            if (hasRow && !_hasRows)
                _hasRows = hasRow;

            return hasRow;
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> that describes the column metadata of the <see cref="DbDataReader"/>. Always <c>null</c>.
        /// </summary>
        /// <returns>A <c>null</c> <see cref="DataTable"/>.</returns>
        public override DataTable GetSchemaTable() => null; // returning null for avoids a NotSupportedException when using DataTable.Load()

        private bool _hasRows;
    }
}