using System;
using Antlr4.Runtime;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// A token source that passes on only the significant tokens of a lexer, i.e. those on the
/// default channel, followed by the end-of-file marker.
/// </summary>
/// <remarks>
/// The SQLite grammar emits each whitespace character as its own hidden token, so a formatted
/// definition produces about as many hidden tokens as significant ones. This source becomes the
/// lexer's token factory and writes every hidden token into one reused token instead of
/// allocating each, then drops it. Nothing downstream can see hidden tokens (whitespace or
/// comments); any text that is needed must be cut from the input by character position.
/// </remarks>
internal sealed class SignificantTokenSource : ITokenSource, ITokenFactory
{
    private readonly Lexer _lexer;
    private readonly CommonToken _hiddenToken = new(TokenConstants.InvalidType);

    /// <summary>
    /// Initializes a new instance of the <see cref="SignificantTokenSource"/> class, and makes it
    /// the token factory of <paramref name="lexer"/>.
    /// </summary>
    /// <param name="lexer">The lexer to read tokens from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="lexer"/> is <see langword="null" />.</exception>
    public SignificantTokenSource(Lexer lexer)
    {
        ArgumentNullException.ThrowIfNull(lexer);

        _lexer = lexer;
        _lexer.TokenFactory = this;
    }

    /// <summary>
    /// Returns the next token on the default channel, or the end-of-file marker.
    /// </summary>
    /// <returns>A significant token, or the end-of-file marker.</returns>
    public IToken NextToken()
    {
        while (true)
        {
            var token = _lexer.NextToken();

            // Compare by reference rather than channel: a token the lexer builds itself, such as
            // the end-of-file marker, is on the default channel unless told otherwise.
            if (!ReferenceEquals(token, _hiddenToken))
                return token;
        }
    }

    /// <inheritdoc />
    public int Line => _lexer.Line;

    /// <inheritdoc />
    public int Column => _lexer.Column;

    /// <inheritdoc />
    public ICharStream InputStream => (ICharStream)_lexer.InputStream;

    /// <inheritdoc />
    public string SourceName => _lexer.SourceName;

    /// <summary>
    /// The factory used for significant tokens. This source is the lexer's factory, so this
    /// cannot be replaced.
    /// </summary>
    public ITokenFactory TokenFactory
    {
        get => this;
        set => throw new NotSupportedException("The token factory of a significant token source cannot be replaced.");
    }

    /// <inheritdoc />
    public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
    {
        return channel != Lexer.DefaultTokenChannel && type != TokenConstants.EOF
            ? _hiddenToken
            : CommonTokenFactory.Default.Create(source, type, text, channel, start, stop, line, charPositionInLine);
    }

    /// <inheritdoc />
    public IToken Create(int type, string text) => CommonTokenFactory.Default.Create(type, text);
}
