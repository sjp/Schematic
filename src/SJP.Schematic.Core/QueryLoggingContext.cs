using System;
using System.Data;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    internal sealed class QueryLoggingContext : IDisposable
    {
        public QueryLoggingContext(IDbConnection connection, string sql, object? param)
        {
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _sql = sql;
            _param = param;

            if (Logging.IsLoggingConfigured(_connection))
                _stopWatch = Stopwatch.StartNew();

            if (Logging.IsLoggingConfigured(_connection))
                Logging.LogCommandExecuting(_connection, _id, _sql, _param);
        }

        public void Dispose()
        {
            if (!Logging.IsLoggingConfigured(_connection))
                return;

            _stopWatch!.Stop();
            Logging.LogCommandExecuted(_connection, _id, _sql, _param, _stopWatch.Elapsed);
        }

        private readonly IDbConnection _connection;
        private readonly string _sql;
        private readonly object? _param;
        private readonly Stopwatch? _stopWatch;
        private readonly Guid _id = Guid.NewGuid();
    }
}