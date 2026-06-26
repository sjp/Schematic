using System.IO;
using Antlr4.Runtime;

namespace SJP.Schematic.Oracle.Parsing.Antlr;

/// <summary>
/// Base class for the generated <c>PlSqlLexer</c>. Required by the <c>superClass</c> option in
/// <c>Grammar/PlSqlLexer.g4</c>; provides the semantic-predicate helpers the grammar relies upon.
/// </summary>
public abstract class PlSqlLexerBase : Lexer
{
    private readonly ICharStream _input;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlSqlLexerBase"/> class.
    /// </summary>
    /// <param name="input">The character stream to lex.</param>
    protected PlSqlLexerBase(ICharStream input)
        : base(input)
    {
        _input = input;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlSqlLexerBase"/> class.
    /// </summary>
    /// <param name="input">The character stream to lex.</param>
    /// <param name="output">The writer used for standard output.</param>
    /// <param name="errorOutput">The writer used for error output.</param>
    protected PlSqlLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
        _input = input;
    }

    /// <summary>
    /// Determines whether the character at the given lookahead position is a newline or the end of input.
    /// </summary>
    /// <param name="pos">The lookahead offset, relative to the current position.</param>
    /// <returns><see langword="true" /> if the character is a newline or end-of-input; otherwise, <see langword="false" />.</returns>
    public bool IsNewlineAtPos(int pos)
    {
        var la = _input.LA(pos);
        return la == -1 || la == '\n';
    }
}
