using System;
using System.Data;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    internal sealed class LoggingAdapter : IDisposable
    {
        public LoggingAdapter(IDbConnection connection, string sql, object? param)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            _sql = sql;
            _param = param;

            if (Logging.IsCommandExecutedLogged(_connection))
                _stopWatch = new Stopwatch();

            if (Logging.IsCommandExecutingLogged(_connection))
                Logging.SchematicLogCommandExecuting(_connection, _id, _sql, _param);
        }

        public void Dispose()
        {
            if (!Logging.IsCommandExecutedLogged(_connection))
                return;

            _stopWatch!.Stop();
            Logging.SchematicLogCommandExecuted(_connection, _id, _sql, _param, _stopWatch.Elapsed);
        }

        private readonly IDbConnection _connection;
        private readonly string _sql;
        private readonly object? _param;
        private readonly Stopwatch? _stopWatch;
        private readonly Guid _id = Guid.NewGuid();
    }
}