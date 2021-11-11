using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A query view provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IDatabaseViewProvider" />
    public class PostgreSqlDatabaseQueryViewProvider : IDatabaseViewProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseQueryViewProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <param name="identifierResolver">An identifier resolver that enables non-strict name resolution.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlDatabaseQueryViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        /// <summary>
        /// A database connection that is specific to a given PostgreSQL database.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets an identifier resolver that enables more relaxed matching against database object names.
        /// </summary>
        /// <value>An identifier resolver.</value>
        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// A database connection factory used to query the database.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// The dialect for the associated database.
        /// </summary>
        /// <value>A database dialect.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// Gets all database views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        public virtual async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await DbConnection.QueryAsync<GetAllViewNamesQueryResult>(ViewsQuery, cancellationToken).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
                .Select(QualifyViewName);

            foreach (var viewName in viewNames)
                yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves the names of views available in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @$"
select schemaname as ""{ nameof(GetAllViewNamesQueryResult.SchemaName) }"", viewname as ""{ nameof(GetAllViewNamesQueryResult.ViewName) }""
from pg_catalog.pg_views
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, viewname";

        /// <summary>
        /// Gets a database view.
        /// </summary>
        /// <param name="viewName">A database view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadView(candidateViewName, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(viewName)
                .Select(QualifyViewName);

            return resolvedNames
                .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
                .FirstSome(cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the view without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="viewName">A view name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = DbConnection.QueryFirstOrNone<GetViewNameQueryResult>(
                ViewNameQuery,
                new GetViewNameQuery { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @$"
select schemaname as ""{ nameof(GetViewNameQueryResult.SchemaName) }"", viewname as ""{ nameof(GetViewNameQueryResult.ViewName) }""
from pg_catalog.pg_views
where schemaname = @{ nameof(GetViewNameQuery.SchemaName) } and viewname = @{ nameof(GetViewNameQuery.ViewName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        /// <summary>
        /// Retrieves a database view, if available.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view definition, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return GetResolvedViewName(candidateViewName, cancellationToken)
                .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var columnsTask = LoadColumnsAsync(viewName, cancellationToken);
            var definitionTask = LoadDefinitionAsync(viewName, cancellationToken);

            await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

            var columns = await columnsTask.ConfigureAwait(false);
            var definition = await definitionTask.ConfigureAwait(false);

            return new DatabaseView(viewName, definition, columns);
        }

        /// <summary>
        /// Retrieves the definition of a view.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A string representing the definition of a view.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return DbConnection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new GetViewDefinitionQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        /// <summary>
        /// A SQL query that retrieves the definition of a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @$"
select view_definition
from information_schema.views
where table_schema = @{ nameof(GetViewDefinitionQuery.SchemaName) } and table_name = @{ nameof(GetViewDefinitionQuery.ViewName) }";

        /// <summary>
        /// Retrieves the columns for a given view.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An ordered collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadColumnsAsyncCore(viewName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var query = await DbConnection.QueryAsync<GetViewColumnsQueryResult>(
                ColumnsQuery,
                new GetViewColumnsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataType),
                    Collation = !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.NumericPrecision, row.NumericScale)
                };

                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var autoIncrement = Option<IAutoIncrement>.None;
                var defaultValue = !row.ColumnDefault.IsNullOrWhiteSpace()
                     ? Option<string>.Some(row.ColumnDefault)
                     : Option<string>.None;

                var column = new DatabaseColumn(columnName, columnType, string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal), defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @$"
select
    column_name as ""{ nameof(GetViewColumnsQueryResult.ColumnName) }"",
    ordinal_position as ""{ nameof(GetViewColumnsQueryResult.OrdinalPosition) }"",
    column_default as ""{ nameof(GetViewColumnsQueryResult.ColumnDefault) }"",
    is_nullable as ""{ nameof(GetViewColumnsQueryResult.IsNullable) }"",
    data_type as ""{ nameof(GetViewColumnsQueryResult.DataType) }"",
    character_maximum_length as ""{ nameof(GetViewColumnsQueryResult.CharacterMaximumLength) }"",
    character_octet_length as ""{ nameof(GetViewColumnsQueryResult.CharacterOctetLength) }"",
    numeric_precision as ""{ nameof(GetViewColumnsQueryResult.NumericPrecision) }"",
    numeric_precision_radix as ""{ nameof(GetViewColumnsQueryResult.NumericPrecisionRadix) }"",
    numeric_scale as ""{ nameof(GetViewColumnsQueryResult.NumericScale) }"",
    datetime_precision as ""{ nameof(GetViewColumnsQueryResult.DatetimePrecision) }"",
    interval_type as ""{ nameof(GetViewColumnsQueryResult.IntervalType) }"",
    collation_catalog as ""{ nameof(GetViewColumnsQueryResult.CollationCatalog) }"",
    collation_schema as ""{ nameof(GetViewColumnsQueryResult.CollationSchema) }"",
    collation_name as ""{ nameof(GetViewColumnsQueryResult.CollationName) }"",
    domain_catalog as ""{ nameof(GetViewColumnsQueryResult.DomainCatalog) }"",
    domain_schema as ""{ nameof(GetViewColumnsQueryResult.DomainSchema) }"",
    domain_name as ""{ nameof(GetViewColumnsQueryResult.DomainName) }"",
    udt_catalog as ""{ nameof(GetViewColumnsQueryResult.UdtCatalog) }"",
    udt_schema as ""{ nameof(GetViewColumnsQueryResult.UdtSchema) }"",
    udt_name as ""{ nameof(GetViewColumnsQueryResult.UdtName) }"",
    dtd_identifier as ""{ nameof(GetViewColumnsQueryResult.DtdIdentifier) }""
from information_schema.columns
where table_schema = @{ nameof(GetViewColumnsQuery.SchemaName) } and table_name = @{ nameof(GetViewColumnsQuery.ViewName) }
order by ordinal_position";

        /// <summary>
        /// Qualifies the name of the view.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <returns>A view name is at least as qualified as the given view name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
        }

        private static class Constants
        {
            public const string Yes = "YES";
        }
    }
}
