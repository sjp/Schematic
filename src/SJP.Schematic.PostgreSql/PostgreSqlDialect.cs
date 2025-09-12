using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A database dialect intended to apply to common Oracle databases.
/// </summary>
/// <seealso cref="DatabaseDialect" />
public class PostgreSqlDialect : DatabaseDialect
{
    /// <summary>
    /// Retrieves the set of identifier defaults for the given database connection.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A set of identifier defaults.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
    }

    private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var result = await connection.DbConnection.QuerySingleAsync<GetIdentifierDefaults.Result>(GetIdentifierDefaults.Sql, cancellationToken).ConfigureAwait(false);

        if (result.Server.IsNullOrWhiteSpace())
            return result with { Server = "127.0.0.1" };

        return result;
    }

    /// <summary>
    /// Gets the database display version. Usually a more user-friendly form of the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A descriptive version.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public override Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken)!;
    }

    private const string DatabaseDisplayVersionQuerySql = "select pg_catalog.version() as DatabaseVersion";

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A version.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public override Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetDatabaseVersionAsyncCore(connection, cancellationToken);
    }

    private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseVersionQuerySql, cancellationToken).ConfigureAwait(false);
        return ParsePostgresVersionString(versionStr!) ?? new Version(0, 0);
    }

    private const string DatabaseVersionQuerySql = "select current_setting('server_version_num') as DatabaseVersion";

    private static Version? ParsePostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);

        return versionStr.Length >= 6
            ? ParseNewPostgresVersionString(versionStr)
            : ParseOldPostgresVersionString(versionStr);
    }

    // for v10 or newer
    private static Version? ParseNewPostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);
        if (versionStr.Length != 6)
            throw new ArgumentException("The version string must be 6 characters long", nameof(versionStr));

        var majorVersionStr = versionStr[..2];
        var minorVersionStr = versionStr.Substring(4, 2);
        var parsedMajor = int.TryParse(majorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var majorVersion);
        var parsedMinor = int.TryParse(minorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minorVersion);

        return parsedMajor && parsedMinor
            ? new Version(majorVersion, minorVersion)
            : null;
    }

    // for v9 or older
    private static Version? ParseOldPostgresVersionString(string versionStr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionStr);
        if (versionStr.Length != 5)
            throw new ArgumentException("The version string must be 5 characters long", nameof(versionStr));

        var majorVersionStr = versionStr[..1];
        var minorVersionStr = versionStr.Substring(1, 2);
        var patchVersionStr = versionStr.Substring(3, 2);
        var parsedMajorVersion = int.TryParse(majorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var majorVersion);
        var parsedMinorVersion = int.TryParse(minorVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minorVersion);
        var parsedPatchVersion = int.TryParse(patchVersionStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var patchVersion);

        return parsedMajorVersion && parsedMinorVersion && parsedPatchVersion
            ? new Version(majorVersion, minorVersion, patchVersion)
            : null;
    }

    /// <summary>
    /// Retrieves a relational database for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public override Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetRelationalDatabaseAsyncCore(connection, cancellationToken);
    }

    private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
        return new PostgreSqlRelationalDatabase(connection, identifierDefaults, identifierResolver);
    }

    /// <summary>
    /// Retrieves a relational database comment provider for the given dialect.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A comment provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public override Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetRelationalDatabaseCommentProviderAsyncCore(connection, cancellationToken);
    }

    private static async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
    {
        var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
        return new PostgreSqlDatabaseCommentProvider(connection.DbConnection, identifierDefaults, identifierResolver);
    }

    /// <summary>
    /// Determines whether the given text is a reserved keyword.
    /// </summary>
    /// <param name="text">A piece of text.</param>
    /// <returns><see langword="true" /> if the given text is a reserved keyword; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null" />, empty or whitespace.</exception>
    public override bool IsReservedKeyword(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return Keywords.Contains(text, StringComparer.OrdinalIgnoreCase);
    }

    // https://www.postgresql.org/docs/current/static/sql-keywords-appendix.html
    private static readonly IEnumerable<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "A",
        "ABORT",
        "ABS",
        "ABSENT",
        "ABSOLUTE",
        "ACCESS",
        "ACCORDING",
        "ACTION",
        "ADA",
        "ADD",
        "ADMIN",
        "AFTER",
        "AGGREGATE",
        "ALL",
        "ALLOCATE",
        "ALSO",
        "ALTER",
        "ALWAYS",
        "ANALYSE",
        "ANALYZE",
        "AND",
        "ANY",
        "ARE",
        "ARRAY",
        "ARRAY_AGG",
        "ARRAY_MAX_CARDINALITY",
        "AS",
        "ASC",
        "ASENSITIVE",
        "ASSERTION",
        "ASSIGNMENT",
        "ASYMMETRIC",
        "AT",
        "ATOMIC",
        "ATTACH",
        "ATTRIBUTE",
        "ATTRIBUTES",
        "AUTHORIZATION",
        "AVG",
        "BACKWARD",
        "BASE64",
        "BEFORE",
        "BEGIN",
        "BEGIN_FRAME",
        "BEGIN_PARTITION",
        "BERNOULLI",
        "BETWEEN",
        "BIGINT",
        "BINARY",
        "BIT",
        "BIT_LENGTH",
        "BLOB",
        "BLOCKED",
        "BOM",
        "BOOLEAN",
        "BOTH",
        "BREADTH",
        "BY",
        "C",
        "CACHE",
        "CALL",
        "CALLED",
        "CARDINALITY",
        "CASCADE",
        "CASCADED",
        "CASE",
        "CAST",
        "CATALOG",
        "CATALOG_NAME",
        "CEIL",
        "CEILING",
        "CHAIN",
        "CHAR",
        "CHARACTER",
        "CHARACTERISTICS",
        "CHARACTERS",
        "CHARACTER_LENGTH",
        "CHARACTER_SET_CATALOG",
        "CHARACTER_SET_NAME",
        "CHARACTER_SET_SCHEMA",
        "CHAR_LENGTH",
        "CHECK",
        "CHECKPOINT",
        "CLASS",
        "CLASS_ORIGIN",
        "CLOB",
        "CLOSE",
        "CLUSTER",
        "COALESCE",
        "COBOL",
        "COLLATE",
        "COLLATION",
        "COLLATION_CATALOG",
        "COLLATION_NAME",
        "COLLATION_SCHEMA",
        "COLLECT",
        "COLUMN",
        "COLUMNS",
        "COLUMN_NAME",
        "COMMAND_FUNCTION",
        "COMMAND_FUNCTION_CODE",
        "COMMENT",
        "COMMENTS",
        "COMMIT",
        "COMMITTED",
        "CONCURRENTLY",
        "CONDITION",
        "CONDITION_NUMBER",
        "CONFIGURATION",
        "CONFLICT",
        "CONNECT",
        "CONNECTION",
        "CONNECTION_NAME",
        "CONSTRAINT",
        "CONSTRAINTS",
        "CONSTRAINT_CATALOG",
        "CONSTRAINT_NAME",
        "CONSTRAINT_SCHEMA",
        "CONSTRUCTOR",
        "CONTAINS",
        "CONTENT",
        "CONTINUE",
        "CONTROL",
        "CONVERSION",
        "CONVERT",
        "COPY",
        "CORR",
        "CORRESPONDING",
        "COST",
        "COUNT",
        "COVAR_POP",
        "COVAR_SAMP",
        "CREATE",
        "CROSS",
        "CSV",
        "CUBE",
        "CUME_DIST",
        "CURRENT",
        "CURRENT_CATALOG",
        "CURRENT_DATE",
        "CURRENT_DEFAULT_TRANSFORM_GROUP",
        "CURRENT_PATH",
        "CURRENT_ROLE",
        "CURRENT_ROW",
        "CURRENT_SCHEMA",
        "CURRENT_TIME",
        "CURRENT_TIMESTAMP",
        "CURRENT_TRANSFORM_GROUP_FOR_TYPE",
        "CURRENT_USER",
        "CURSOR",
        "CURSOR_NAME",
        "CYCLE",
        "DATA",
        "DATABASE",
        "DATALINK",
        "DATE",
        "DATETIME_INTERVAL_CODE",
        "DATETIME_INTERVAL_PRECISION",
        "DAY",
        "DB",
        "DEALLOCATE",
        "DEC",
        "DECIMAL",
        "DECLARE",
        "DEFAULT",
        "DEFAULTS",
        "DEFERRABLE",
        "DEFERRED",
        "DEFINED",
        "DEFINER",
        "DEGREE",
        "DELETE",
        "DELIMITER",
        "DELIMITERS",
        "DENSE_RANK",
        "DEPENDS",
        "DEPTH",
        "DEREF",
        "DERIVED",
        "DESC",
        "DESCRIBE",
        "DESCRIPTOR",
        "DETACH",
        "DETERMINISTIC",
        "DIAGNOSTICS",
        "DICTIONARY",
        "DISABLE",
        "DISCARD",
        "DISCONNECT",
        "DISPATCH",
        "DISTINCT",
        "DLNEWCOPY",
        "DLPREVIOUSCOPY",
        "DLURLCOMPLETE",
        "DLURLCOMPLETEONLY",
        "DLURLCOMPLETEWRITE",
        "DLURLPATH",
        "DLURLPATHONLY",
        "DLURLPATHWRITE",
        "DLURLSCHEME",
        "DLURLSERVER",
        "DLVALUE",
        "DO",
        "DOCUMENT",
        "DOMAIN",
        "DOUBLE",
        "DROP",
        "DYNAMIC",
        "DYNAMIC_FUNCTION",
        "DYNAMIC_FUNCTION_CODE",
        "EACH",
        "ELEMENT",
        "ELSE",
        "EMPTY",
        "ENABLE",
        "ENCODING",
        "ENCRYPTED",
        "END",
        "END-EXEC",
        "END_FRAME",
        "END_PARTITION",
        "ENFORCED",
        "ENUM",
        "EQUALS",
        "ESCAPE",
        "EVENT",
        "EVERY",
        "EXCEPT",
        "EXCEPTION",
        "EXCLUDE",
        "EXCLUDING",
        "EXCLUSIVE",
        "EXEC",
        "EXECUTE",
        "EXISTS",
        "EXP",
        "EXPLAIN",
        "EXPRESSION",
        "EXTENSION",
        "EXTERNAL",
        "EXTRACT",
        "FALSE",
        "FAMILY",
        "FETCH",
        "FILE",
        "FILTER",
        "FINAL",
        "FIRST",
        "FIRST_VALUE",
        "FLAG",
        "FLOAT",
        "FLOOR",
        "FOLLOWING",
        "FOR",
        "FORCE",
        "FOREIGN",
        "FORTRAN",
        "FORWARD",
        "FOUND",
        "FRAME_ROW",
        "FREE",
        "FREEZE",
        "FROM",
        "FS",
        "FULL",
        "FUNCTION",
        "FUNCTIONS",
        "FUSION",
        "G",
        "GENERAL",
        "GENERATED",
        "GET",
        "GLOBAL",
        "GO",
        "GOTO",
        "GRANT",
        "GRANTED",
        "GREATEST",
        "GROUP",
        "GROUPING",
        "GROUPS",
        "HANDLER",
        "HAVING",
        "HEADER",
        "HEX",
        "HIERARCHY",
        "HOLD",
        "HOUR",
        "ID",
        "IDENTITY",
        "IF",
        "IGNORE",
        "ILIKE",
        "IMMEDIATE",
        "IMMEDIATELY",
        "IMMUTABLE",
        "IMPLEMENTATION",
        "IMPLICIT",
        "IMPORT",
        "IN",
        "INCLUDING",
        "INCREMENT",
        "INDENT",
        "INDEX",
        "INDEXES",
        "INDICATOR",
        "INHERIT",
        "INHERITS",
        "INITIALLY",
        "INLINE",
        "INNER",
        "INOUT",
        "INPUT",
        "INSENSITIVE",
        "INSERT",
        "INSTANCE",
        "INSTANTIABLE",
        "INSTEAD",
        "INT",
        "INTEGER",
        "INTEGRITY",
        "INTERSECT",
        "INTERSECTION",
        "INTERVAL",
        "INTO",
        "INVOKER",
        "IS",
        "ISNULL",
        "ISOLATION",
        "JOIN",
        "K",
        "KEY",
        "KEY_MEMBER",
        "KEY_TYPE",
        "LABEL",
        "LAG",
        "LANGUAGE",
        "LARGE",
        "LAST",
        "LAST_VALUE",
        "LATERAL",
        "LEAD",
        "LEADING",
        "LEAKPROOF",
        "LEAST",
        "LEFT",
        "LENGTH",
        "LEVEL",
        "LIBRARY",
        "LIKE",
        "LIKE_REGEX",
        "LIMIT",
        "LINK",
        "LISTEN",
        "LN",
        "LOAD",
        "LOCAL",
        "LOCALTIME",
        "LOCALTIMESTAMP",
        "LOCATION",
        "LOCATOR",
        "LOCK",
        "LOCKED",
        "LOGGED",
        "LOWER",
        "M",
        "MAP",
        "MAPPING",
        "MATCH",
        "MATCHED",
        "MATERIALIZED",
        "MAX",
        "MAXVALUE",
        "MAX_CARDINALITY",
        "MEMBER",
        "MERGE",
        "MESSAGE_LENGTH",
        "MESSAGE_OCTET_LENGTH",
        "MESSAGE_TEXT",
        "METHOD",
        "MIN",
        "MINUTE",
        "MINVALUE",
        "MOD",
        "MODE",
        "MODIFIES",
        "MODULE",
        "MONTH",
        "MORE",
        "MOVE",
        "MULTISET",
        "MUMPS",
        "NAME",
        "NAMES",
        "NAMESPACE",
        "NATIONAL",
        "NATURAL",
        "NCHAR",
        "NCLOB",
        "NESTING",
        "NEW",
        "NEXT",
        "NFC",
        "NFD",
        "NFKC",
        "NFKD",
        "NIL",
        "NO",
        "NONE",
        "NORMALIZE",
        "NORMALIZED",
        "NOT",
        "NOTHING",
        "NOTIFY",
        "NOTNULL",
        "NOWAIT",
        "NTH_VALUE",
        "NTILE",
        "NULL",
        "NULLABLE",
        "NULLIF",
        "NULLS",
        "NUMBER",
        "NUMERIC",
        "OBJECT",
        "OCCURRENCES_REGEX",
        "OCTETS",
        "OCTET_LENGTH",
        "OF",
        "OFF",
        "OFFSET",
        "OIDS",
        "OLD",
        "ON",
        "ONLY",
        "OPEN",
        "OPERATOR",
        "OPTION",
        "OPTIONS",
        "OR",
        "ORDER",
        "ORDERING",
        "ORDINALITY",
        "OTHERS",
        "OUT",
        "OUTER",
        "OUTPUT",
        "OVER",
        "OVERLAPS",
        "OVERLAY",
        "OVERRIDING",
        "OWNED",
        "OWNER",
        "P",
        "PAD",
        "PARALLEL",
        "PARAMETER",
        "PARAMETER_MODE",
        "PARAMETER_NAME",
        "PARAMETER_ORDINAL_POSITION",
        "PARAMETER_SPECIFIC_CATALOG",
        "PARAMETER_SPECIFIC_NAME",
        "PARAMETER_SPECIFIC_SCHEMA",
        "PARSER",
        "PARTIAL",
        "PARTITION",
        "PASCAL",
        "PASSING",
        "PASSTHROUGH",
        "PASSWORD",
        "PATH",
        "PERCENT",
        "PERCENTILE_CONT",
        "PERCENTILE_DISC",
        "PERCENT_RANK",
        "PERIOD",
        "PERMISSION",
        "PLACING",
        "PLANS",
        "PLI",
        "POLICY",
        "PORTION",
        "POSITION",
        "POSITION_REGEX",
        "POWER",
        "PRECEDES",
        "PRECEDING",
        "PRECISION",
        "PREPARE",
        "PREPARED",
        "PRESERVE",
        "PRIMARY",
        "PRIOR",
        "PRIVILEGES",
        "PROCEDURAL",
        "PROCEDURE",
        "PROGRAM",
        "PUBLIC",
        "PUBLICATION",
        "QUOTE",
        "RANGE",
        "RANK",
        "READ",
        "READS",
        "REAL",
        "REASSIGN",
        "RECHECK",
        "RECOVERY",
        "RECURSIVE",
        "REF",
        "REFERENCES",
        "REFERENCING",
        "REFRESH",
        "REGR_AVGX",
        "REGR_AVGY",
        "REGR_COUNT",
        "REGR_INTERCEPT",
        "REGR_R2",
        "REGR_SLOPE",
        "REGR_SXX",
        "REGR_SXY",
        "REGR_SYY",
        "REINDEX",
        "RELATIVE",
        "RELEASE",
        "RENAME",
        "REPEATABLE",
        "REPLACE",
        "REPLICA",
        "REQUIRING",
        "RESET",
        "RESPECT",
        "RESTART",
        "RESTORE",
        "RESTRICT",
        "RESULT",
        "RETURN",
        "RETURNED_CARDINALITY",
        "RETURNED_LENGTH",
        "RETURNED_OCTET_LENGTH",
        "RETURNED_SQLSTATE",
        "RETURNING",
        "RETURNS",
        "REVOKE",
        "RIGHT",
        "ROLE",
        "ROLLBACK",
        "ROLLUP",
        "ROUTINE",
        "ROUTINE_CATALOG",
        "ROUTINE_NAME",
        "ROUTINE_SCHEMA",
        "ROW",
        "ROWS",
        "ROW_COUNT",
        "ROW_NUMBER",
        "RULE",
        "SAVEPOINT",
        "SCALE",
        "SCHEMA",
        "SCHEMAS",
        "SCHEMA_NAME",
        "SCOPE",
        "SCOPE_CATALOG",
        "SCOPE_NAME",
        "SCOPE_SCHEMA",
        "SCROLL",
        "SEARCH",
        "SECOND",
        "SECTION",
        "SECURITY",
        "SELECT",
        "SELECTIVE",
        "SELF",
        "SENSITIVE",
        "SEQUENCE",
        "SEQUENCES",
        "SERIALIZABLE",
        "SERVER",
        "SERVER_NAME",
        "SESSION",
        "SESSION_USER",
        "SET",
        "SETOF",
        "SETS",
        "SHARE",
        "SHOW",
        "SIMILAR",
        "SIMPLE",
        "SIZE",
        "SKIP",
        "SMALLINT",
        "SNAPSHOT",
        "SOME",
        "SOURCE",
        "SPACE",
        "SPECIFIC",
        "SPECIFICTYPE",
        "SPECIFIC_NAME",
        "SQL",
        "SQLCODE",
        "SQLERROR",
        "SQLEXCEPTION",
        "SQLSTATE",
        "SQLWARNING",
        "SQRT",
        "STABLE",
        "STANDALONE",
        "START",
        "STATE",
        "STATEMENT",
        "STATIC",
        "STATISTICS",
        "STDDEV_POP",
        "STDDEV_SAMP",
        "STDIN",
        "STDOUT",
        "STORAGE",
        "STRICT",
        "STRIP",
        "STRUCTURE",
        "STYLE",
        "SUBCLASS_ORIGIN",
        "SUBMULTISET",
        "SUBSCRIPTION",
        "SUBSTRING",
        "SUBSTRING_REGEX",
        "SUCCEEDS",
        "SUM",
        "SYMMETRIC",
        "SYSID",
        "SYSTEM",
        "SYSTEM_TIME",
        "SYSTEM_USER",
        "T",
        "TABLE",
        "TABLES",
        "TABLESAMPLE",
        "TABLESPACE",
        "TABLE_NAME",
        "TEMP",
        "TEMPLATE",
        "TEMPORARY",
        "TEXT",
        "THEN",
        "TIES",
        "TIME",
        "TIMESTAMP",
        "TIMEZONE_HOUR",
        "TIMEZONE_MINUTE",
        "TO",
        "TOKEN",
        "TOP_LEVEL_COUNT",
        "TRAILING",
        "TRANSACTION",
        "TRANSACTIONS_COMMITTED",
        "TRANSACTIONS_ROLLED_BACK",
        "TRANSACTION_ACTIVE",
        "TRANSFORM",
        "TRANSFORMS",
        "TRANSLATE",
        "TRANSLATE_REGEX",
        "TRANSLATION",
        "TREAT",
        "TRIGGER",
        "TRIGGER_CATALOG",
        "TRIGGER_NAME",
        "TRIGGER_SCHEMA",
        "TRIM",
        "TRIM_ARRAY",
        "TRUE",
        "TRUNCATE",
        "TRUSTED",
        "TYPE",
        "TYPES",
        "UESCAPE",
        "UNBOUNDED",
        "UNCOMMITTED",
        "UNDER",
        "UNENCRYPTED",
        "UNION",
        "UNIQUE",
        "UNKNOWN",
        "UNLINK",
        "UNLISTEN",
        "UNLOGGED",
        "UNNAMED",
        "UNNEST",
        "UNTIL",
        "UNTYPED",
        "UPDATE",
        "UPPER",
        "URI",
        "USAGE",
        "USER",
        "USER_DEFINED_TYPE_CATALOG",
        "USER_DEFINED_TYPE_CODE",
        "USER_DEFINED_TYPE_NAME",
        "USER_DEFINED_TYPE_SCHEMA",
        "USING",
        "VACUUM",
        "VALID",
        "VALIDATE",
        "VALIDATOR",
        "VALUE",
        "VALUES",
        "VALUE_OF",
        "VARBINARY",
        "VARCHAR",
        "VARIADIC",
        "VARYING",
        "VAR_POP",
        "VAR_SAMP",
        "VERBOSE",
        "VERSION",
        "VERSIONING",
        "VIEW",
        "VIEWS",
        "VOLATILE",
        "WHEN",
        "WHENEVER",
        "WHERE",
        "WHITESPACE",
        "WIDTH_BUCKET",
        "WINDOW",
        "WITH",
        "WITHIN",
        "WITHOUT",
        "WORK",
        "WRAPPER",
        "WRITE",
        "XML",
        "XMLAGG",
        "XMLATTRIBUTES",
        "XMLBINARY",
        "XMLCAST",
        "XMLCOMMENT",
        "XMLCONCAT",
        "XMLDECLARATION",
        "XMLDOCUMENT",
        "XMLELEMENT",
        "XMLEXISTS",
        "XMLFOREST",
        "XMLITERATE",
        "XMLNAMESPACES",
        "XMLPARSE",
        "XMLPI",
        "XMLQUERY",
        "XMLROOT",
        "XMLSCHEMA",
        "XMLSERIALIZE",
        "XMLTABLE",
        "XMLTEXT",
        "XMLVALIDATE",
        "YEAR",
        "YES",
        "ZONE",
    };

    /// <summary>
    /// Quotes a string identifier, e.g. a column name.
    /// </summary>
    /// <param name="identifier">An identifier.</param>
    /// <returns>A quoted identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />, empty or whitespace.</exception>
    public override string QuoteIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    /// <summary>
    /// Quotes a qualified name.
    /// </summary>
    /// <param name="name">An object name.</param>
    /// <returns>A quoted name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
    public override string QuoteName(Identifier name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var pieces = new List<string>();

        if (name.Server != null)
            pieces.Add(QuoteIdentifier(name.Server));
        if (name.Database != null)
            pieces.Add(QuoteIdentifier(name.Database));
        if (name.Schema != null)
            pieces.Add(QuoteIdentifier(name.Schema));
        if (name.LocalName != null)
            pieces.Add(QuoteIdentifier(name.LocalName));

        return pieces.Join(".");
    }

    /// <summary>
    /// Gets a database column data type provider.
    /// </summary>
    /// <value>The type provider.</value>
    public override IDbTypeProvider TypeProvider => InnerTypeProvider;

    private static readonly IDbTypeProvider InnerTypeProvider = new PostgreSqlDbTypeProvider();
}