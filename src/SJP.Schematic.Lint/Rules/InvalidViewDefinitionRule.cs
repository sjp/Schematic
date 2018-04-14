using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SJP.Schematic.Core;

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

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var dialect = database.Dialect;
            if (dialect == null)
                throw new ArgumentException("The dialect on the given database is null.", nameof(database));

            return database.Views.SelectMany(v => AnalyseView(dialect, v)).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseDialect dialect, IRelationalDatabaseView view)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            try
            {
                var quotedViewName = dialect.QuoteName(view.Name);
                var query = "select 1 as tmp from " + quotedViewName;
                Connection.ExecuteScalar<long>(query);

                return Enumerable.Empty<IRuleMessage>();
            }
            catch
            {
                var messageText = $"The view { view.Name } was unable to be queried. This may indicate an incorrect view definition.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);
                return new[] { ruleMessage };
            }
        }

        private const string RuleTitle = "Invalid view definition.";
    }
}
