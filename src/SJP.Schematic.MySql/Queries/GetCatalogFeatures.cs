namespace SJP.Schematic.MySql.Queries;

internal static class GetCatalogFeatures
{
    /// <summary>
    /// Describes the parts of <c>information_schema</c> that vary between MySQL and MariaDB releases,
    /// determining which of the catalog queries can be used against the connected server.
    /// </summary>
    internal sealed record Result
    {
        /// <summary>
        /// Whether <c>information_schema.check_constraints</c> is present. Added in MySQL 8.0.16 and MariaDB 10.2.22.
        /// </summary>
        public required bool HasCheckConstraints { get; init; }

        /// <summary>
        /// Whether <c>information_schema.table_constraints</c> has an <c>enforced</c> column. MySQL added it
        /// alongside <c>NOT ENFORCED</c> in 8.0.16; MariaDB has no such syntax, so a check is always enforced.
        /// </summary>
        public required bool HasConstraintEnforcedColumn { get; init; }

        /// <summary>
        /// Whether <c>information_schema.statistics</c> has an <c>expression</c> column. MySQL added it alongside
        /// functional key parts in 8.0.13; MariaDB has no equivalent syntax and reports index visibility
        /// through an inverted <c>ignored</c> column instead of <c>is_visible</c>.
        /// </summary>
        public required bool HasIndexExpressionColumn { get; init; }
    }

    internal const string Sql = $"""

select
    exists (
        select 1 from information_schema.tables
        where table_schema = 'information_schema' and table_name = 'CHECK_CONSTRAINTS'
    ) as `{nameof(Result.HasCheckConstraints)}`,
    exists (
        select 1 from information_schema.columns
        where table_schema = 'information_schema' and table_name = 'TABLE_CONSTRAINTS' and column_name = 'ENFORCED'
    ) as `{nameof(Result.HasConstraintEnforcedColumn)}`,
    exists (
        select 1 from information_schema.columns
        where table_schema = 'information_schema' and table_name = 'STATISTICS' and column_name = 'EXPRESSION'
    ) as `{nameof(Result.HasIndexExpressionColumn)}`
""";
}
