using System;
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// The kinds of token reported by <see cref="SqlServerNameScanner"/>.
/// </summary>
internal enum SqlServerNameTokenKind
{
    /// <summary>A regular (undelimited) identifier that is not a keyword.</summary>
    Identifier,

    /// <summary>An identifier delimited by square brackets.</summary>
    BracketedIdentifier,

    /// <summary>An identifier delimited by double quotes.</summary>
    QuotedIdentifier,

    /// <summary>The dot that separates the parts of a qualified name.</summary>
    Dot,

    /// <summary>Anything else: keywords, literals, variables, comments, labels and punctuation.</summary>
    Other,
}

/// <summary>
/// A token reported by <see cref="SqlServerNameScanner"/>, as a range of the scanned text.
/// </summary>
/// <param name="Kind">The kind of token.</param>
/// <param name="Start">The index of the first character of the token.</param>
/// <param name="Length">The number of characters in the token.</param>
internal readonly record struct SqlServerNameToken(SqlServerNameTokenKind Kind, int Start, int Length)
{
    public bool IsIdentifier => Kind is SqlServerNameTokenKind.Identifier or SqlServerNameTokenKind.BracketedIdentifier or SqlServerNameTokenKind.QuotedIdentifier;
}

/// <summary>
/// A single-pass scanner over T-SQL text that reports the tokens that can make up object names
/// (identifiers and the dots between them), and everything else only as <see cref="SqlServerNameTokenKind.Other"/>.
/// </summary>
/// <remarks>
/// The text is split exactly as ScriptDom's lexer splits it: the same character classes, comments,
/// literals, labels and failure cases. Unlike ScriptDom, no object is created per token, which
/// matters because module definitions can be long. Whitespace is skipped; comments are reported
/// as <see cref="SqlServerNameTokenKind.Other"/> because ScriptDom keeps them as tokens, so they
/// separate the parts of a name. Double-quoted text is treated as an identifier, as it is with
/// <c>QUOTED_IDENTIFIER ON</c>. The set of keywords is taken from ScriptDom itself.
/// </remarks>
internal ref struct SqlServerNameScanner
{
    private readonly string _text;
    private readonly string _paramName;
    private int _position;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerNameScanner"/> struct.
    /// </summary>
    /// <param name="text">The T-SQL text to scan.</param>
    /// <param name="paramName">The name of the calling argument that supplied <paramref name="text"/>, used when reporting scan failures.</param>
    public SqlServerNameScanner(string text, string paramName)
    {
        _text = text;
        _paramName = paramName;
        _position = 0;
        Current = default;
    }

    /// <summary>
    /// The token most recently read by <see cref="MoveNext"/>.
    /// </summary>
    public SqlServerNameToken Current { get; private set; }

    /// <summary>
    /// Reads the next token.
    /// </summary>
    /// <returns><see langword="true"/> if a token was read; <see langword="false"/> at the end of the text.</returns>
    /// <exception cref="ArgumentException">The text contains an unterminated literal, quoted identifier or comment, or a character that is not valid T-SQL.</exception>
    public bool MoveNext()
    {
        var text = _text;
        while (_position < text.Length && IsWhitespace(text[_position]))
            _position++;

        if (_position >= text.Length)
            return false;

        var start = _position;
        var kind = ScanToken(text[start]);
        Current = new SqlServerNameToken(kind, start, _position - start);
        return true;
    }

    /// <summary>
    /// Gets the name that an identifier token refers to, without delimiters or escapes.
    /// </summary>
    /// <param name="token">An identifier token read from this scanner.</param>
    /// <returns>The identifier's value.</returns>
    public readonly string GetIdentifierValue(SqlServerNameToken token)
    {
        return token.Kind switch
        {
            SqlServerNameTokenKind.Identifier => _text.Substring(token.Start, token.Length),
            SqlServerNameTokenKind.BracketedIdentifier => Unescape(_text.AsSpan(token.Start + 1, token.Length - 2), ']'),
            SqlServerNameTokenKind.QuotedIdentifier => Unescape(_text.AsSpan(token.Start + 1, token.Length - 2), '"'),
            _ => throw new ArgumentException($"Expected an identifier token. Received {token.Kind}.", nameof(token)),
        };
    }

    private SqlServerNameTokenKind ScanToken(char c)
    {
        switch (c)
        {
            case '-' when Peek(1) == '-':
                SkipLineComment();
                return SqlServerNameTokenKind.Other;
            case '/' when Peek(1) == '*':
                SkipBlockComment();
                return SqlServerNameTokenKind.Other;
            case '\'':
                _position++;
                SkipDelimited('\'', "string literal");
                return SqlServerNameTokenKind.Other;
            case '[':
                return ScanBracketedIdentifier();
            case '"':
                return ScanQuotedIdentifier();
            case '@':
                // a variable, or a system function such as @@ROWCOUNT
                _position++;
                SkipIdentifierPart();
                return SqlServerNameTokenKind.Other;
            case '$':
                ScanDollar();
                return SqlServerNameTokenKind.Other;
            case '.' when IsAsciiDigit(Peek(1)):
                SkipNumber();
                return SqlServerNameTokenKind.Other;
            case '.':
                _position++;
                return SqlServerNameTokenKind.Dot;
            case >= '0' and <= '9':
                SkipNumber();
                return SqlServerNameTokenKind.Other;
        }

        if (IsIdentifierStart(c))
            return ScanWord();

        if (IsInvalid(c))
            throw Fail($"unexpected character '{c}'");

        // punctuation and operators; a multi-character operator is reported one character at a
        // time, which makes no difference to the names around it
        _position++;
        return SqlServerNameTokenKind.Other;
    }

    private SqlServerNameTokenKind ScanWord()
    {
        var start = _position;
        _position++;
        SkipIdentifierPart();

        // N'...' is a Unicode string literal rather than the identifier N
        if (_position - start == 1 && (_text[start] == 'N' || _text[start] == 'n') && Peek(0) == '\'')
        {
            _position++;
            SkipDelimited('\'', "string literal");
            return SqlServerNameTokenKind.Other;
        }

        // a word directly followed by a single colon is a GOTO label, keywords included
        if (Peek(0) == ':' && Peek(1) != ':')
        {
            _position++;
            return SqlServerNameTokenKind.Other;
        }

        return IsKeyword(_text.AsSpan(start, _position - start))
            ? SqlServerNameTokenKind.Other
            : SqlServerNameTokenKind.Identifier;
    }

    private SqlServerNameTokenKind ScanBracketedIdentifier()
    {
        var start = _position;
        _position++;
        SkipDelimited(']', "bracketed identifier");

        if (_position - start == 2)
            throw Fail("empty bracketed identifier");

        return SqlServerNameTokenKind.BracketedIdentifier;
    }

    private SqlServerNameTokenKind ScanQuotedIdentifier()
    {
        var start = _position;
        _position++;
        SkipDelimited('"', "quoted identifier");

        // "" is lexed as an empty string literal, as there is no empty identifier
        return _position - start == 2
            ? SqlServerNameTokenKind.Other
            : SqlServerNameTokenKind.QuotedIdentifier;
    }

    private void ScanDollar()
    {
        var start = _position;
        _position++;

        // a SQLCMD variable, $(name)
        if (Peek(0) == '(')
        {
            var end = _text.IndexOf(')', _position + 1);
            if (end < 0)
                throw Fail("unterminated SQLCMD variable", start);

            _position = end + 1;
            return;
        }

        // a pseudo-column such as $action, $identity or $partition
        if (IsIdentifierStart(Peek(0)))
        {
            SkipIdentifierPart();
            return;
        }

        // a money literal, which may have whitespace and a sign between the $ and its digits
        while (_position < _text.Length && IsWhitespace(_text[_position]))
            _position++;
        if (Peek(0) is '+' or '-')
            _position++;
        if (!IsAsciiDigit(Peek(0)))
            throw Fail("invalid money literal", start);

        SkipNumber();
    }

    private void SkipNumber()
    {
        if (Peek(0) == '0' && Peek(1) is 'x' or 'X')
        {
            _position += 2;
            while (char.IsAsciiHexDigit(Peek(0)))
                _position++;
            return;
        }

        SkipDigits();
        if (Peek(0) == '.')
        {
            _position++;
            SkipDigits();
        }

        if (Peek(0) is 'e' or 'E')
        {
            _position++;
            if (Peek(0) is '+' or '-')
                _position++;
            SkipDigits();
        }
    }

    private void SkipDigits()
    {
        while (IsAsciiDigit(Peek(0)))
            _position++;
    }

    private void SkipIdentifierPart()
    {
        while (_position < _text.Length && IsIdentifierPart(_text[_position]))
            _position++;
    }

    private void SkipLineComment()
    {
        var end = _text.AsSpan(_position).IndexOfAny('\r', '\n');
        _position = end < 0 ? _text.Length : _position + end;
    }

    private void SkipBlockComment()
    {
        // block comments nest
        var start = _position;
        _position += 2;
        var depth = 1;
        while (_position < _text.Length - 1)
        {
            var c = _text[_position];
            if (c == '/' && _text[_position + 1] == '*')
            {
                depth++;
                _position += 2;
            }
            else if (c == '*' && _text[_position + 1] == '/')
            {
                _position += 2;
                if (--depth == 0)
                    return;
            }
            else
            {
                _position++;
            }
        }

        throw Fail("unterminated comment", start);
    }

    // Skips to just past the closing delimiter, where a doubled delimiter is an escaped one.
    // The position must be just past the opening delimiter.
    private void SkipDelimited(char delimiter, string description)
    {
        var start = _position - 1;
        while (true)
        {
            var end = _text.IndexOf(delimiter, _position);
            if (end < 0)
                throw Fail($"unterminated {description}", start);

            if (end + 1 < _text.Length && _text[end + 1] == delimiter)
            {
                _position = end + 2;
                continue;
            }

            _position = end + 1;
            return;
        }
    }

    private readonly char Peek(int offset)
    {
        var index = _position + offset;
        return index < _text.Length ? _text[index] : '\0';
    }

    private readonly ArgumentException Fail(string reason) => Fail(reason, _position);

    private readonly ArgumentException Fail(string reason, int position) =>
        new($"Could not parse the given expression as a SQL expression. Given: {_text}. Error: {reason} at position {position}.", _paramName);

    private static string Unescape(ReadOnlySpan<char> content, char delimiter)
    {
        // any delimiter inside the content is one half of an escaped pair
        if (!content.Contains(delimiter))
            return content.ToString();

        var builder = new StringBuilder(content.Length);
        for (var i = 0; i < content.Length; i++)
        {
            builder.Append(content[i]);
            if (content[i] == delimiter)
                i++;
        }

        return builder.ToString();
    }

    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    // ScriptDom treats almost every non-ASCII character as a letter, including digits and
    // marks, apart from the whitespace below and a few characters it rejects outright
    private static bool IsIdentifierStart(char c) => c switch
    {
        >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_' or '#' => true,
        < '\u0080' or '\uFFFF' => false,
        _ => !IsWhitespace(c) && !IsInvalid(c),
    };

    private static bool IsIdentifierPart(char c) => IsIdentifierStart(c) || IsAsciiDigit(c) || c is '@' or '$' or '\uFFFF';

    private static bool IsWhitespace(char c) => c switch
    {
        <= ' ' => true,
        '\u0085' or '\u00A0' or '\u1680' or '\u2028' or '\u2029' or '\u202F' or '\u205F' or '\u3000' => true,
        >= '\u2000' and <= '\u200B' => true,
        _ => false,
    };

    private static bool IsInvalid(char c) => c is '?' or '\\' or '`' or ']' or '\u007F' or (>= '\u167A' and <= '\u167F');

    private static bool IsKeyword(ReadOnlySpan<char> word)
    {
        // keywords are ASCII, and an ASCII-only check avoids case-insensitive matches such as
        // the long s (U+017F) folding to 's'
        return Ascii.IsValid(word) && KeywordLookup.Contains(word);
    }

    private static readonly FrozenSet<string> Keywords = CreateKeywords();

    private static readonly FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> KeywordLookup = Keywords.GetAlternateLookup<ReadOnlySpan<char>>();

    // Every keyword has its own ScriptDom token type, named after the keyword in Pascal case
    // (CURRENT_USER is CurrentUser). Lexing each name, with and without underscores between its
    // words, keeps those that ScriptDom does not lex as an identifier.
    private static FrozenSet<string> CreateKeywords()
    {
        var names = Enum.GetNames<TSqlTokenType>();
        var candidates = names.Concat(names.Select(SeparateWords)).Distinct(StringComparer.Ordinal);

        var parser = new TSql180Parser(true, SqlEngineType.All);
        using var reader = new StringReader(string.Join(' ', candidates));
        var tokens = parser.GetTokenStream(reader, out _);

        return tokens
            .Where(t => t.TokenType is not (TSqlTokenType.Identifier or TSqlTokenType.WhiteSpace or TSqlTokenType.EndOfFile))
            .Select(t => t.Text)
            .Where(w => w.Length > 0 && w.All(c => char.IsAsciiLetter(c) || c == '_'))
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string SeparateWords(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsAsciiLetterUpper(name[i]) && char.IsAsciiLetterLower(name[i - 1]))
                builder.Append('_');
            builder.Append(name[i]);
        }

        return builder.ToString();
    }
}
