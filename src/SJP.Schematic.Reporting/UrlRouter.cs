using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting
{
    internal static class UrlRouter
    {
        public static string GetTableUrl(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return "tables/" + tableName.ToSafeKey() + ".html";
        }

        public static string GetViewUrl(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return "views/" + viewName.ToSafeKey() + ".html";
        }

        public static string GetSequenceUrl(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return "sequences/" + sequenceName.ToSafeKey() + ".html";
        }

        public static string GetSynonymUrl(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return "synonyms/" + synonymName.ToSafeKey() + ".html";
        }

        public static string GetRoutineUrl(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return "routines/" + routineName.ToSafeKey() + ".html";
        }
    }
}
