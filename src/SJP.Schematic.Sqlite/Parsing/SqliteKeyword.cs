using System;
using EnumsNET;

namespace SJP.Schematic.Sqlite.Parsing;

internal readonly struct SqliteKeyword
{
    public SqliteKeyword(string keyword, SqliteToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        if (!token.IsValid())
            throw new ArgumentException($"The {nameof(SqliteToken)} provided must be a valid enum.", nameof(token));

        Text = keyword;
        Token = token;
    }

    public string Text { get; }

    public SqliteToken Token { get; }
}