using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal struct ExpressionKeyword
    {
        public ExpressionKeyword(string keyword, ExpressionToken token)
        {
            if (keyword.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(keyword));

            Text = keyword;
            Token = token;
        }

        public string Text { get; }

        public ExpressionToken Token { get; }
    }
}
