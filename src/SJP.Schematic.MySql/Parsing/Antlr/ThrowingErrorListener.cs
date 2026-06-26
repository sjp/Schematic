using System;
using System.IO;
using Antlr4.Runtime;

namespace SJP.Schematic.MySql.Parsing.Antlr;

/// <summary>
/// An exception thrown when the ANTLR lexer encounters a syntax error.
/// </summary>
internal sealed class MySqlSyntaxErrorException : Exception
{
    public MySqlSyntaxErrorException()
    {
    }

    public MySqlSyntaxErrorException(string message)
        : base(message)
    {
    }

    public MySqlSyntaxErrorException(string message, Exception innerException)
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
        => throw new MySqlSyntaxErrorException($"line {line}:{charPositionInLine} {msg}");
}
