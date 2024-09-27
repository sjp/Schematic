using System;
using EnumsNET;

namespace SJP.Schematic.Oracle.Parsing;

internal readonly struct OracleKeyword
{
    public OracleKeyword(string keyword, OracleToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        if (!token.IsValid())
            throw new ArgumentException($"The {nameof(OracleToken)} provided must be a valid enum.", nameof(token));

        Text = keyword;
        Token = token;
    }

    public string Text { get; }

    public OracleToken Token { get; }
}