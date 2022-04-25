using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetDatabaseVersion
{
    internal const string Sql = "select SERVERPROPERTY('ProductVersion') as DatabaseVersion";
}