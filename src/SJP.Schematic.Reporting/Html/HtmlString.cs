using System;

namespace SJP.Schematic.Reporting.Html
{
    public sealed class HtmlString
    {
        public HtmlString(string encodedText)
        {
            _text = encodedText ?? throw new ArgumentNullException(nameof(encodedText));
        }

        public string ToHtmlString() => _text;

        public override string ToString() => throw new NotSupportedException("Use ToHtmlString() instead of ToString(). This is to avoid unintended use of encoded text for templating.");

        public static implicit operator HtmlString(string encodedText) => new HtmlString(encodedText);

        private readonly string _text;
    }
}
