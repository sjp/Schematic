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
        /// <summary>
        /// Creates an instance of <see cref="DbTransactionAdapter"/> to wrap an <see cref="IDbTransaction"/> as a <see cref="DbTransaction"/>.
        /// </summary>
        /// <param name="connection">A <see cref="DbConnection"/> associated with the transaction.</param>
        /// <param name="transaction">An <see cref="IDbTransaction"/> associated with <paramref name="connection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="transaction"/> is <c>null</c>.</exception>
        public DbTransactionAdapter(DbConnection connection, IDbTransaction transaction)
        {
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Specifies the <see cref="DbConnection"/> object associated with the transaction.
        /// </summary>
        protected override DbConnection DbConnection { get; }

        /// <summary>
        /// The <see cref="IDbTransaction"/> instance that is being wrapped as a <see cref="DbTransaction"/>.
        /// </summary>
        protected IDbTransaction Transaction { get; }

        /// <summary>
        /// Specifies the <see cref="System.Data.IsolationLevel"/> for this transaction.
        /// </summary>
        public override IsolationLevel IsolationLevel => Transaction.IsolationLevel;

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public override void Commit() => Transaction.Commit();

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public override void Rollback() => Transaction.Rollback();

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DbTransaction"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed || !disposing)
                return;

            Transaction.Dispose();
            _disposed = true;
        }

        private bool _disposed;
    }
}