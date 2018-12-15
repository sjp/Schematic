using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class InvalidViewDefinitionRule : Rule
    {
        public InvalidViewDefinitionRule(IDbConnection connection, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDbConnection Connection { get; }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (database.Dialect == null)
                throw new ArgumentException("The dialect on the given database is null.", nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var views = await database.GetAllViews(cancellationToken).ConfigureAwait(false);
            if (views.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();
            foreach (var view in views)
            {
                var messages = await AnalyseViewAsync(database.Dialect, view, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseDialect dialect, IRelationalDatabaseView view)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            try
            {
                var simpleViewName = Identifier.CreateQualifiedIdentifier(view.Name.Schema, view.Name.LocalName);
                var quotedViewName = dialect.QuoteName(simpleViewName);
                var query = "select 1 as tmp from " + quotedViewName;
                Connection.ExecuteScalar<long>(query);

                return Array.Empty<IRuleMessage>();
            }
            catch
            {
                var message = BuildMessage(view.Name);
                return new[] { message };
            }
        }

        protected Task<IEnumerable<IRuleMessage>> AnalyseViewAsync(IDatabaseDialect dialect, IRelationalDatabaseView view, CancellationToken cancellationToken)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return AnalyseViewAsyncCore(dialect, view, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseViewAsyncCore(IDatabaseDialect dialect, IRelationalDatabaseView view, CancellationToken cancellationToken)
        {
            try
            {
                var simpleViewName = Identifier.CreateQualifiedIdentifier(view.Name.Schema, view.Name.LocalName);
                var quotedViewName = dialect.QuoteName(simpleViewName);
                var query = "select 1 as tmp from " + quotedViewName;
                await Connection.ExecuteScalarAsync<long>(query, cancellationToken).ConfigureAwait(false);

                return Array.Empty<IRuleMessage>();
            }
            catch
            {
                var message = BuildMessage(view.Name);
                return new[] { message };
            }
        }

        protected virtual IRuleMessage BuildMessage(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var messageText = $"The view { viewName } was unable to be queried. This may indicate an incorrect view definition.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Invalid view definition.";
    }
}
