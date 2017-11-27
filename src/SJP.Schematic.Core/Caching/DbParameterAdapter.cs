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
        public DbParameterAdapter(IDbDataParameter parameter)
        {
            InnerParameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        protected IDbDataParameter InnerParameter { get; }

        public override DbType DbType
        {
            get => InnerParameter.DbType;
            set => InnerParameter.DbType = value;
        }

        public override ParameterDirection Direction
        {
            get => InnerParameter.Direction;
            set => InnerParameter.Direction = value;
        }

        public override bool IsNullable
        {
            get => InnerParameter.IsNullable;
            set { }
        }

        public override string ParameterName
        {
            get => InnerParameter.ParameterName;
            set => InnerParameter.ParameterName = value;
        }

        public override int Size
        {
            get => InnerParameter.Size;
            set => InnerParameter.Size = value;
        }

        public override string SourceColumn
        {
            get => InnerParameter.SourceColumn;
            set => InnerParameter.SourceColumn = value;
        }

        public override bool SourceColumnNullMapping
        {
            get => InnerParameter.IsNullable;
            set { }
        }

        public override object Value
        {
            get => InnerParameter.Value;
            set => InnerParameter.Value = value;
        }

        public override void ResetDbType()
        {
        }
    }
}