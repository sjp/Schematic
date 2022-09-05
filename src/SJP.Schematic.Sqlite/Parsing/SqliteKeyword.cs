using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing;

internal struct SqliteKeyword
{
    public SqliteKeyword(string keyword, SqliteToken token)
    {
        if (keyword.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(keyword));
        if (!token.IsValid())
            throw new ArgumentException($"The {nameof(SqliteToken)} provided must be a valid enum.", nameof(token));

        Text = keyword;
        Token = token;
    }

    public string Text { get; }

    public SqliteToken Token { get; }
}