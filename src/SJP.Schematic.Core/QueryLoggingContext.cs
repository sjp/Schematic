using System;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    internal sealed class QueryLoggingContext
    {
        public QueryLoggingContext(IDbConnectionFactory connectionFactory, string sql, object? param)
        {
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _sql = sql;
            _param = param;
        }

        public void Start()
        {
            if (!Logging.IsLoggingConfigured(_connectionFactory) || _started)
                return;

            _started = true;
            _stopWatch = Stopwatch.StartNew();
            Logging.LogCommandExecuting(_connectionFactory, _id, _sql, _param);
        }

        public void Stop()
        {
            if (!Logging.IsLoggingConfigured(_connectionFactory) || !_started)
                return;

            _stopWatch?.Stop();
            Logging.LogCommandExecuted(_connectionFactory, _id, _sql, _param, _stopWatch?.Elapsed ?? TimeSpan.Zero);
        }

        private bool _started;
        private Stopwatch? _stopWatch;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _sql;
        private readonly object? _param;
        private readonly Guid _id = Guid.NewGuid();
    }
}