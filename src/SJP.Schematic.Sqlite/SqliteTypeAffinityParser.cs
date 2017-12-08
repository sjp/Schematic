using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteTypeAffinityParser
    {
        // https://sqlite.org/datatype3.html#determination_of_column_affinity
        public SqliteTypeAffinity ParseTypeName(string typeName)
        {
            if (typeName.IsNullOrWhiteSpace())
                return SqliteTypeAffinity.Numeric;

            if (typeName.Contains("INT", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Integer;

            if (typeName.Contains("CHAR", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("CLOB", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("TEXT", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Text;

            if (typeName.Contains("BLOB", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Blob;

            if (typeName.Contains("REAL", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("FLOA", StringComparison.OrdinalIgnoreCase)
                || typeName.Contains("DOUB", StringComparison.OrdinalIgnoreCase))
                return SqliteTypeAffinity.Real;

            return SqliteTypeAffinity.Numeric;
        }
    }
}
