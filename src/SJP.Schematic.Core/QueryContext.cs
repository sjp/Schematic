using System;
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

        public static Task<QueryContext> CreateAsync(IDbConnectionFactory connectionFactory, QueryLoggingContext loggingContext)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (loggingContext == null)
                throw new ArgumentNullException(nameof(loggingContext));

            return CreateAsyncCore(connectionFactory, loggingContext);
        }

        private static async Task<QueryContext> CreateAsyncCore(IDbConnectionFactory connectionFactory, QueryLoggingContext loggingContext)
        {
            if (SemaphoreLookup.TryGetValue(connectionFactory, out var semaphore))
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

        private static readonly ConditionalWeakTable<IDbConnectionFactory, AsyncSemaphore> SemaphoreLookup = new ConditionalWeakTable<IDbConnectionFactory, AsyncSemaphore>();

        public static void SetMaxConcurrentQueries(IDbConnectionFactory connectionFactory, AsyncSemaphore semaphore)
        {
            SemaphoreLookup.AddOrUpdate(connectionFactory, semaphore);
        }
    }
}