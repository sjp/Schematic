using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql
{
    /// <summary>
    /// A view provider for MySQL.
    /// </summary>
    /// <seealso cref="IDatabaseViewProvider" />
    public class MySqlDatabaseViewProvider : IDatabaseViewProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseViewProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public MySqlDatabaseViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection that is specific to a given MySQL database.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// A database connection factory to query the database.
        /// </summary>
        /// <value>A connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// A dialect for the MySQL connection.
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
            var queryResult = await DbConnection.QueryAsync<QualifiedName>(
                ViewsQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            var viewNames = queryResult
                .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .Select(QualifyViewName);

            foreach (var viewName in viewNames)
                yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves the names of views in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewsQuery => ViewsQuerySql;

        private static readonly string ViewsQuerySql = @$"
select
    TABLE_SCHEMA as `{ nameof(QualifiedName.SchemaName) }`,
    TABLE_NAME as `{ nameof(QualifiedName.ObjectName) }`
from information_schema.views
where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

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
        /// <param name="viewName">A view name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = DbConnection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private static readonly string ViewNameQuerySql = @$"
select
    table_schema as `{ nameof(QualifiedName.SchemaName) }`,
    table_name as `{ nameof(QualifiedName.ObjectName) }`
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName
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
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        /// <summary>
        /// A SQL query that retrieves the definition of a view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select view_definition
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName";

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
            var query = await DbConnection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = !row.Collation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                        : Option<Identifier>.None,
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation != null
                    && row.ExtraInformation.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
                var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;

                var column = new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private static readonly string ColumnsQuerySql = @$"
select
    column_name as `{ nameof(ColumnData.ColumnName) }`,
    data_type as `{ nameof(ColumnData.DataTypeName) }`,
    character_maximum_length as `{ nameof(ColumnData.CharacterMaxLength) }`,
    numeric_precision as `{ nameof(ColumnData.Precision) }`,
    numeric_scale as `{ nameof(ColumnData.Scale) }`,
    datetime_precision as `{ nameof(ColumnData.DateTimePrecision) }`,
    collation_name as `{ nameof(ColumnData.Collation) }`,
    is_nullable as `{ nameof(ColumnData.IsNullable) }`,
    column_default as `{ nameof(ColumnData.DefaultValue) }`,
    generation_expression as `{ nameof(ColumnData.ComputedColumnDefinition) }`,
    extra as `{ nameof(ColumnData.ExtraInformation) }`
from information_schema.columns
where table_schema = @SchemaName and table_name = @ViewName
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
            public const string AutoIncrement = "auto_increment";

            public const string No = "NO";
        }
    }
}
