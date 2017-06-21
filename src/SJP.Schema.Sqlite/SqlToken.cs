using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Sqlite.Parsing
{
    public enum SqlToken
    {
        None,
        Keyword,
        Identifier,
        Delimiter,
        Dot,
        Comment,
        Literal,
        Operator,
        Terminator,
        LParen,
        RParen,
        Type
    }
}
