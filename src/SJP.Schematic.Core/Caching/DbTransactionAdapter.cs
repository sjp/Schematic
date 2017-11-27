using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbTransaction"/> from an <see cref="IDbTransaction"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbTransactionAdapter : DbTransaction
    {
        public DbTransactionAdapter(DbConnection connection, IDbTransaction transaction)
        {
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            InnerTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        protected override DbConnection DbConnection { get; }

        protected IDbTransaction InnerTransaction { get; }

        public override IsolationLevel IsolationLevel => InnerTransaction.IsolationLevel;

        public override void Commit() => InnerTransaction.Commit();

        public override void Rollback() => InnerTransaction.Rollback();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed || !disposing)
                return;

            InnerTransaction.Dispose();
            _disposed = true;
        }

        private bool _disposed;
    }
}