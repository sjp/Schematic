using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Parsing;

internal struct SqlServerKeyword
{
    public SqlServerKeyword(string keyword, SqlServerToken token)
    {
        if (keyword.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(keyword));
        if (!token.IsValid())
            throw new ArgumentException($"The { nameof(SqlServerToken) } provided must be a valid enum.", nameof(token));

        Text = keyword;
        Token = token;
    }

    public string Text { get; }

    public SqlServerToken Token { get; }
}
