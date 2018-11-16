using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions
{
    public static class ConnectionExtensions
    {
        public static Option<T> QueryFirstOrNone<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where T : class
        {
            if (cnn == null)
                throw new ArgumentNullException(nameof(cnn));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            var result = cnn.QueryFirstOrDefault<T>(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static Task<Option<T>> QueryFirstOrNoneAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where T : class
        {
            if (cnn == null)
                throw new ArgumentNullException(nameof(cnn));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QueryFirstOrNoneAsyncCore<T>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        private static async Task<Option<T>> QueryFirstOrNoneAsyncCore<T>(IDbConnection cnn, string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType)
            where T : class
        {
            var result = await cnn.QueryFirstOrDefaultAsync<T>(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
            return result != null
                ? Option<T>.Some(result)
                : Option<T>.None;
        }

        public static Option<T> QuerySingleOrNone<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where T : class
        {
            if (cnn == null)
                throw new ArgumentNullException(nameof(cnn));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            try
            {
                var result = cnn.QuerySingleOrDefault<T>(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }

        public static Task<Option<T>> QuerySingleOrNoneAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where T : class
        {
            if (cnn == null)
                throw new ArgumentNullException(nameof(cnn));
            if (sql.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sql));

            return QuerySingleOrNoneAsyncCore<T>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        private static async Task<Option<T>> QuerySingleOrNoneAsyncCore<T>(IDbConnection cnn, string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType)
            where T : class
        {
            try
            {
                var result = await cnn.QuerySingleOrDefaultAsync<T>(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType).ConfigureAwait(false);
                return result != null
                    ? Option<T>.Some(result)
                    : Option<T>.None;
            }
            catch (InvalidOperationException) // for > 1 case
            {
                return Option<T>.None;
            }
        }
    }
}