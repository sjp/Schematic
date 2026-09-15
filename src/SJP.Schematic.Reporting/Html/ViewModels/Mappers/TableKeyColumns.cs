using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// The names of the columns in a table's primary key, unique keys and foreign keys, so that a column's
/// key membership is a set lookup instead of a scan over every key.
/// </summary>
/// <param name="PrimaryKeyColumns">The names of the columns in the primary key.</param>
/// <param name="UniqueKeyColumns">The names of the columns in any unique key.</param>
/// <param name="ForeignKeyColumns">The names of the child columns of any foreign key.</param>
internal sealed record TableKeyColumns(
    IReadOnlySet<string> PrimaryKeyColumns,
    IReadOnlySet<string> UniqueKeyColumns,
    IReadOnlySet<string> ForeignKeyColumns
);
