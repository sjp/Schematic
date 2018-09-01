using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Parsing
{
    internal struct OracleKeyword
    {
        public OracleKeyword(string keyword, OracleToken token)
        {
            if (keyword.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(keyword));
            if (!token.IsValid())
                throw new ArgumentException($"The { nameof(OracleToken) } provided must be a valid enum.", nameof(token));

            Text = keyword;
            Token = token;
        }

        public string Text { get; }

        public OracleToken Token { get; }
    }
}
