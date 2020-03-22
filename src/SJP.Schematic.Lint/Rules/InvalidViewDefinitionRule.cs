using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class InvalidViewDefinitionRule : Rule, IViewRule
    {
        public InvalidViewDefinitionRule(ISchematicConnection connection, RuleLevel level)
            : base(RuleTitle, level)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected ISchematicConnection Connection { get; }

        public IAsyncEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));

            return AnalyseViewsCore(views, cancellationToken);
        }

        private async IAsyncEnumerable<IRuleMessage> AnalyseViewsCore(IEnumerable<IDatabaseView> views, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var view in views)
            {
                var messages = await AnalyseViewAsync(view, cancellationToken).ConfigureAwait(false);
                foreach (var message in messages)
                    yield return message;
            }
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
                var quotedViewName = Connection.Dialect.QuoteName(simpleViewName);
                var query = "select 1 as dummy from " + quotedViewName;
                await Connection.DbConnection.ExecuteScalarAsync<long>(query, cancellationToken).ConfigureAwait(false);

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
