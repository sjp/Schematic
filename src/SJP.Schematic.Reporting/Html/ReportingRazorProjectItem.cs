using System;
using System.IO;
using System.Text;
using RazorLight.Razor;

namespace SJP.Schematic.Reporting.Html;

internal sealed class ReportingRazorProjectItem : RazorLightProjectItem
{
    public ReportingRazorProjectItem(string key, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Key = key;
        _content = content;
    }

    public override string Key { get; }

    public override bool Exists => _content != null;

    public override Stream Read() => new MemoryStream(Encoding.UTF8.GetBytes(_content));

    private readonly string _content;
}