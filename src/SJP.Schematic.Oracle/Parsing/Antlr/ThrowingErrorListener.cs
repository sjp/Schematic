using System;
using System.IO;
using Antlr4.Runtime;

namespace SJP.Schematic.Oracle.Parsing.Antlr;

/// <summary>
/// An exception thrown when the ANTLR lexer encounters a syntax error.
/// </summary>
internal sealed class OracleSyntaxErrorException : Exception
{
    public OracleSyntaxErrorException()
    {
    }

    public OracleSyntaxErrorException(string message)
        : base(message)
    {
    }

    public OracleSyntaxErrorException(string message, Exception innerException)
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
        => throw new OracleSyntaxErrorException($"line {line}:{charPositionInLine} {msg}");
}
