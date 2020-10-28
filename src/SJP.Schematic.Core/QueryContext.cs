using System;
using System.Runtime.CompilerServices;
using System.Threading;
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

        public static Task<QueryContext> CreateAsync(IDbConnectionFactory connectionFactory, QueryLoggingContext loggingContext, CancellationToken cancellationToken = default)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (loggingContext == null)
                throw new ArgumentNullException(nameof(loggingContext));

            return CreateAsyncCore(connectionFactory, loggingContext, cancellationToken);
        }

        private static async Task<QueryContext> CreateAsyncCore(IDbConnectionFactory connectionFactory, QueryLoggingContext loggingContext, CancellationToken cancellationToken)
        {
            if (SemaphoreLookup.TryGetValue(connectionFactory, out var semaphore))
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    loggingContext.Start();
                    return new QueryContext(semaphore, loggingContext);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            loggingContext.Start();
            return new QueryContext(loggingContext);
        }

        private static readonly ConditionalWeakTable<IDbConnectionFactory, AsyncSemaphore> SemaphoreLookup = new ConditionalWeakTable<IDbConnectionFactory, AsyncSemaphore>();

        public static void SetMaxConcurrentQueries(IDbConnectionFactory connectionFactory, AsyncSemaphore semaphore)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));
            if (semaphore == null)
                throw new ArgumentNullException(nameof(semaphore));

            SemaphoreLookup.AddOrUpdate(connectionFactory, semaphore);
        }
    }
}