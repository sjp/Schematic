using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal struct SqliteKeyword
    {
        public SqliteKeyword(string keyword, SqliteToken token)
        {
            if (keyword.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(keyword));

            Text = keyword;
            Token = token;
        }

        public string Text { get; }

        public SqliteToken Token { get; }
    }
}
