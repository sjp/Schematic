using System;
using System.IO;
using Antlr4.Runtime;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// An exception thrown when the ANTLR lexer or parser encounters a syntax error.
/// </summary>
internal sealed class SqliteSyntaxErrorException : Exception
{
    public SqliteSyntaxErrorException()
    {
    }

    public SqliteSyntaxErrorException(string message)
        : base(message)
    {
    }

    public SqliteSyntaxErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// An ANTLR error listener that throws on the first lexer or parser syntax error,
/// rather than printing to the console and attempting error recovery.
/// </summary>
internal sealed class ThrowingErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
{
    public static ThrowingErrorListener Instance { get; } = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        => throw new SqliteSyntaxErrorException($"line {line}:{charPositionInLine} {msg}");

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        => throw new SqliteSyntaxErrorException($"line {line}:{charPositionInLine} {msg}");
}
