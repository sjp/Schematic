using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbCommand"/> from an <see cref="IDbCommand"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbCommandAdapter : DbCommand
    {
        public DbCommandAdapter(DbConnection connection, IDbCommand command)
        {
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            InnerCommand = command ?? throw new ArgumentNullException(nameof(command));

            DbParameterCollection = new DbParameterCollectionAdapter(command.Parameters);
        }

        protected override DbConnection DbConnection { get; set; }

        protected override DbTransaction DbTransaction { get; set; }

        protected override DbParameterCollection DbParameterCollection { get; }

        protected IDbCommand InnerCommand { get; }

        public override string CommandText
        {
            get => InnerCommand.CommandText;
            set => InnerCommand.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => InnerCommand.CommandTimeout;
            set => InnerCommand.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => InnerCommand.CommandType;
            set => InnerCommand.CommandType = value;
        }

        public override bool DesignTimeVisible
        {
            get => false;
            set { }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => InnerCommand.UpdatedRowSource;
            set => InnerCommand.UpdatedRowSource = value;
        }

        public override void Cancel() => InnerCommand.Cancel();

        public override int ExecuteNonQuery() => InnerCommand.ExecuteNonQuery();

        public override object ExecuteScalar() => InnerCommand.ExecuteScalar();

        public override void Prepare() => InnerCommand.Prepare();

        protected override DbParameter CreateDbParameter()
        {
            var parameter = InnerCommand.CreateParameter();
            return parameter as DbParameter ?? new DbParameterAdapter(parameter);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var reader = InnerCommand.ExecuteReader(behavior);
            return reader as DbDataReader ?? new DbDataReaderAdapter(reader, behavior);
        }
    }
}