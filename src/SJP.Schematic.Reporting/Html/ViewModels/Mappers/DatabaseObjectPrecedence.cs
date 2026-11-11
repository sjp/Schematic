using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// Ranks the kinds of database object for when one name is shared by several of them: a table
/// ranks first, then a view, sequence, synonym, routine and finally a user-defined type.
/// </summary>
internal static class DatabaseObjectPrecedence
{
    /// <summary>
    /// Gets the rank of the kind of <paramref name="databaseObject"/>. A lower rank comes first.
    /// </summary>
    public static int Of(DatabaseObject databaseObject) => databaseObject switch
    {
        IRelationalDatabaseTable => 0,
        IDatabaseView => 1,
        IDatabaseSequence => 2,
        IDatabaseSynonym => 3,
        IDatabaseRoutine => 4,
        DatabaseUserDefinedType => 5,
    };
}
