using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class InvalidViewDefinitionRule : Rule, IViewRule
    {
        public InvalidViewDefinitionRule(IDbConnection connection, IDatabaseDialect dialect, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public IEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return views.SelectMany(AnalyseView).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseViewsAsync(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return AnalyseViewsAsyncCore(views, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseViewsAsyncCore(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken)
        {
            if (views.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var view in views)
            {
                var messages = await AnalyseViewAsync(view, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        protected Task<IEnumerable<IRuleMessage>> AnalyseViewAsync(IDatabaseView view, CancellationToken cancellationToken)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return AnalyseViewAsyncCore(view, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseViewAsyncCore(IDatabaseView view, CancellationToken cancellationToken)
        {
            try
            {
                var simpleViewName = Identifier.CreateQualifiedIdentifier(view.Name.Schema, view.Name.LocalName);
                var quotedViewName = Dialect.QuoteName(simpleViewName);
                var query = "select 1 as dummy from " + quotedViewName;
                await Connection.ExecuteScalarAsync<long>(query, cancellationToken).ConfigureAwait(false);

                return Array.Empty<IRuleMessage>();
            }
            catch
            {
                var message = BuildMessage(view.Name);
                return new[] { message };
            }
        }

        private IEnumerable<IRuleMessage> AnalyseView(IDatabaseView view)
        {
            try
            {
                var simpleViewName = Identifier.CreateQualifiedIdentifier(view.Name.Schema, view.Name.LocalName);
                var quotedViewName = Dialect.QuoteName(simpleViewName);
                var query = "select 1 as dummy from " + quotedViewName;
                _ = Connection.ExecuteFirstScalar<long>(query);

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
