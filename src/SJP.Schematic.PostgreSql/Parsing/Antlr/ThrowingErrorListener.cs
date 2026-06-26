using System;
using System.IO;
using Antlr4.Runtime;

namespace SJP.Schematic.PostgreSql.Parsing.Antlr;

/// <summary>
/// An exception thrown when the ANTLR lexer encounters a syntax error.
/// </summary>
internal sealed class PostgreSqlSyntaxErrorException : Exception
{
    public PostgreSqlSyntaxErrorException()
    {
    }

    public PostgreSqlSyntaxErrorException(string message)
        : base(message)
    {
    }

    public PostgreSqlSyntaxErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// An ANTLR error listener that throws on the first lexer syntax error, rather than printing to the
/// console and attempting error recovery.
/// </summary>
internal sealed class ThrowingErrorListener : IAntlrErrorListener<int>
{
    public static ThrowingErrorListener Instance { get; } = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        => throw new PostgreSqlSyntaxErrorException($"line {line}:{charPositionInLine} {msg}");
}
