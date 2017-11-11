using EnumsNET;
using System;
using SysTextEncoding = System.Text.Encoding;

namespace SJP.Schematic.Sqlite.Pragma
{
    public static class EncodingExtensions
    {
        public static SysTextEncoding AsTextEncoding(this Encoding encoding)
        {
            if (!encoding.IsValid())
                throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(encoding));

            switch (encoding)
            {
                case Encoding.Utf8:
                    return SysTextEncoding.UTF8;
                case Encoding.Utf16:
                case Encoding.Utf16le:
                    return SysTextEncoding.Unicode;
                case Encoding.Utf16be:
                    return SysTextEncoding.BigEndianUnicode;
                default:
                    throw new InvalidOperationException("Unknown and unsupported encoding found: " + encoding.ToString());
            }
        }
    }
}
