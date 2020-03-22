using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SJP.Schematic.Core
{
    internal sealed class QueryContext : IDisposable
    {
        private QueryContext(QueryLoggingContext loggingContext)
        {
            _loggingContext = loggingContext ?? throw new ArgumentNullException(nameof(loggingContext));
        }

        private QueryContext(AsyncSemaphore semaphore, QueryLoggingContext loggingContext)
        {
            _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            _loggingContext = loggingContext ?? throw new ArgumentNullException(nameof(loggingContext));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _semaphore?.Release();
            _loggingContext.Stop();
            _disposed = true;
        }

        private bool _disposed;
        private readonly AsyncSemaphore? _semaphore;
        private readonly QueryLoggingContext _loggingContext;

        public static Task<QueryContext> CreateAsync(IDbConnection connection, QueryLoggingContext loggingContext)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (loggingContext == null)
                throw new ArgumentNullException(nameof(loggingContext));

            return CreateAsyncCore(connection, loggingContext);
        }

        private static async Task<QueryContext> CreateAsyncCore(IDbConnection connection, QueryLoggingContext loggingContext)
        {
            if (SemaphoreLookup.TryGetValue(connection, out var semaphore))
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    loggingContext.Start();
                    return new QueryContext(semaphore, loggingContext);
                }
                catch
                {
                    semaphore.Release();
                    throw;
                }
            }
            else
            {
                loggingContext.Start();
                return new QueryContext(loggingContext);
            }
        }

        private static readonly ConditionalWeakTable<IDbConnection, AsyncSemaphore> SemaphoreLookup = new ConditionalWeakTable<IDbConnection, AsyncSemaphore>();

        public static void SetMaxConcurrentQueries(IDbConnection connection, AsyncSemaphore semaphore)
        {
            SemaphoreLookup.AddOrUpdate(connection, semaphore);
        }
    }
}