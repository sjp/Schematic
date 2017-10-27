using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal struct SqlServerKeyword
    {
        public SqlServerKeyword(string keyword, SqlServerToken token)
        {
            if (keyword.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(keyword));

            Text = keyword;
            Token = token;
        }

        public string Text { get; }

        public SqlServerToken Token { get; }
    }
}
