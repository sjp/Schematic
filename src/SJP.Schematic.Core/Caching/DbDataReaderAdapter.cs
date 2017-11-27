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
        public DbDataReaderAdapter(IDataReader reader, CommandBehavior commandBehavior)
        {
            InnerReader = reader ?? throw new ArgumentNullException(nameof(reader));
            Behavior = commandBehavior;
        }

        protected IDataReader InnerReader { get; }

        protected CommandBehavior Behavior { get; }

        public override object this[int ordinal] => InnerReader[ordinal];

        public override object this[string name] => InnerReader[name];

        public override int Depth => InnerReader.Depth;

        public override int FieldCount => InnerReader.FieldCount;

        public override bool HasRows => _hasRows;

        public override bool IsClosed => InnerReader.IsClosed;

        public override int RecordsAffected => InnerReader.Depth;

        public override bool GetBoolean(int ordinal) => InnerReader.GetBoolean(ordinal);

        public override byte GetByte(int ordinal) => InnerReader.GetByte(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => InnerReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        public override char GetChar(int ordinal) => InnerReader.GetChar(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => InnerReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        public override string GetDataTypeName(int ordinal) => InnerReader.GetDataTypeName(ordinal);

        public override DateTime GetDateTime(int ordinal) => InnerReader.GetDateTime(ordinal);

        public override decimal GetDecimal(int ordinal) => InnerReader.GetDecimal(ordinal);

        public override double GetDouble(int ordinal) => InnerReader.GetDouble(ordinal);

        public override IEnumerator GetEnumerator() => new DbEnumerator(this, Behavior.CommonFlags(CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection);

        public override Type GetFieldType(int ordinal) => InnerReader.GetFieldType(ordinal);

        public override float GetFloat(int ordinal) => InnerReader.GetFloat(ordinal);

        public override Guid GetGuid(int ordinal) => InnerReader.GetGuid(ordinal);

        public override short GetInt16(int ordinal) => InnerReader.GetInt16(ordinal);

        public override int GetInt32(int ordinal) => InnerReader.GetInt32(ordinal);

        public override long GetInt64(int ordinal) => InnerReader.GetInt64(ordinal);

        public override string GetName(int ordinal) => InnerReader.GetName(ordinal);

        public override int GetOrdinal(string name) => InnerReader.GetOrdinal(name);

        public override string GetString(int ordinal) => InnerReader.GetString(ordinal);

        public override object GetValue(int ordinal) => InnerReader.GetValue(ordinal);

        public override int GetValues(object[] values) => InnerReader.GetValues(values);

        public override bool IsDBNull(int ordinal) => InnerReader.IsDBNull(ordinal);

        public override bool NextResult()
        {
            _hasRows = false;
            return InnerReader.NextResult();
        }

        public override bool Read()
        {
            var hasRow = InnerReader.Read();

            // set flag to ensure that we detect whether rows are present
            if (hasRow && !_hasRows)
                _hasRows = hasRow;

            return hasRow;
        }

        // Return null for GetSchemaTable()
        // This avoids a NotSupportedException when attempting to load a DataTable via DataTable.Load()
        public override DataTable GetSchemaTable() => null;

        private bool _hasRows;
    }
}