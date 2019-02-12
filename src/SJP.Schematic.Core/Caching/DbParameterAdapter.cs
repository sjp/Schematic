using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbParameter"/> from an <see cref="IDbDataParameter"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbParameterAdapter : DbParameter
    {
        /// <summary>
        /// Creates an instance of <see cref="DbParameterAdapter"/> to wrap an <see cref="IDbDataParameter"/> as a <see cref="DbParameter"/>.
        /// </summary>
        /// <param name="parameter">An <see cref="IDbDataParameter"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <c>null</c>.</exception>
        public DbParameterAdapter(IDbDataParameter parameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        /// <summary>
        /// The <see cref="IDbDataParameter"/> instance that is being wrapped as a <see cref="DbParameter"/>.
        /// </summary>
        protected IDbDataParameter Parameter { get; }

        /// <summary>
        /// Gets or sets the <see cref="DbType"/> of the parameter.
        /// </summary>
        public override DbType DbType
        {
            get => Parameter.DbType;
            set => Parameter.DbType = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        public override ParameterDirection Direction
        {
            get => Parameter.Direction;
            set => Parameter.Direction = value;
        }

        /// <summary>
        /// Gets a value that indicates whether the parameter accepts null values. The setter is ignored.
        /// </summary>
        public override bool IsNullable
        {
            get => Parameter.IsNullable;
            set => _ = value;
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="DbParameter"/>.
        /// </summary>
        public override string ParameterName
        {
            get => Parameter.ParameterName;
            set => Parameter.ParameterName = value;
        }

        /// <summary>
        ///	Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        public override int Size
        {
            get => Parameter.Size;
            set => Parameter.Size = value;
        }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the <see cref="DataSet"/> and used for loading or returning the <see cref="Value"/>.
        /// </summary>
        public override string SourceColumn
        {
            get => Parameter.SourceColumn;
            set => Parameter.SourceColumn = value;
        }

        /// <summary>
        ///	Sets or gets a value which indicates whether the source column is nullable.This allows <see cref="DbCommandBuilder"/> to correctly generate Update statements for nullable columns. The setter is ignored.
        /// </summary>
        public override bool SourceColumnNullMapping
        {
            get => Parameter.IsNullable;
            set => _ = value;
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public override object Value
        {
            get => Parameter.Value;
            set => Parameter.Value = value;
        }

        /// <summary>
        /// Resets the <see cref="DbType"/> property to its original settings. This method does nothing and should be ignored.
        /// </summary>
        public override void ResetDbType()
        {
        }
    }
}