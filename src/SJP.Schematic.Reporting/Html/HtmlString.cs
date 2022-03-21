using System;
using System.ComponentModel;

namespace SJP.Schematic.Reporting.Html;

/// <summary>
/// <para>Internal. Not intended to be used outside of this assembly.</para>
/// <para>Ensures that pre-rendered text is explicitly used so that rendering errors are more difficult.</para>
/// </summary>
public sealed class HtmlString
{
    public HtmlString(string encodedText)
    {
        _text = encodedText ?? throw new ArgumentNullException(nameof(encodedText));
    }

    public string ToHtmlString() => _text;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => throw new NotSupportedException("Use ToHtmlString() instead of ToString(). This is to avoid unintended use of encoded text for templating.");

    public static implicit operator HtmlString(string encodedText) => new(encodedText);

    private readonly string _text;
}