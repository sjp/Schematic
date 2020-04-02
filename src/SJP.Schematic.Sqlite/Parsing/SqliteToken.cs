using Superpower.Display;

namespace SJP.Schematic.Sqlite.Parsing
{
    public enum SqliteToken
    {
        None,

        Identifier,

        [Token(Description = "built-in identifier")]
        BuiltInIdentifier,

        String,

        [Token(Description = "regular expression")]
        RegularExpression,

        Number,

        [Token(Description = "hexadecimal number")]
        HexNumber,

        [Token(Description = "blob literal")]
        Blob,

        [Token(Example = "&")]
        Ampersand,

        [Token(Example = "`")]
        Backtick,

        [Token(Example = ",")]
        Comma,

        [Token(Example = ".")]
        Period,

        [Token(Example = "[")]
        LBracket,

        [Token(Example = "]")]
        RBracket,

        [Token(Example = "(")]
        LParen,

        [Token(Example = ")")]
        RParen,

        [Token(Example = "?")]
        QuestionMark,

        [Token(Example = "\"")]
        DoubleQuote,

        [Token(Example = "|")]
        Pipe,

        [Token(Example = "~")]
        Tilde,

        [Token(Example = ";")]
        Semicolon,

        [Token(Category = "operator", Example = "+")]
        Plus,

        [Token(Category = "operator", Example = "-")]
        Minus,

        [Token(Example = "*")]
        Asterisk,

        [Token(Category = "operator", Example = "/")]
        ForwardSlash,

        [Token(Category = "operator", Example = "%")]
        Percent,

        [Token(Category = "operator", Example = "^")]
        Caret,

        [Token(Category = "operator", Example = "<")]
        LessThan,

        [Token(Category = "operator", Example = "<=")]
        LessThanOrEqual,

        [Token(Category = "operator", Example = ">")]
        GreaterThan,

        [Token(Category = "operator", Example = ">=")]
        GreaterThanOrEqual,

        [Token(Category = "operator", Example = "=")]
        Equal,

        [Token(Category = "operator", Example = "<>")]
        NotEqual,

        [Token(Category = "operator", Example = "<<")]
        LeftShift,

        [Token(Category = "operator", Example = ">>")]
        RightShift,

        [Token(Category = "keyword", Example = "and")]
        And,

        [Token(Category = "keyword", Example = "is")]
        Is,

        [Token(Category = "keyword", Example = "like")]
        Like,

        [Token(Category = "keyword", Example = "not")]
        Not,

        [Token(Category = "keyword", Example = "or")]
        Or,

        [Token(Category = "keyword", Example = "true")]
        True,

        [Token(Category = "keyword", Example = "false")]
        False,

        [Token(Category = "keyword", Example = "null")]
        Null,

        [Token(Category = "keyword", Example = "abort")]
        Abort,

        [Token(Category = "keyword", Example = "action")]
        Action,

        [Token(Category = "keyword", Example = "add")]
        Add,

        [Token(Category = "keyword", Example = "after")]
        After,

        [Token(Category = "keyword", Example = "all")]
        All,

        [Token(Category = "keyword", Example = "alter")]
        Alter,

        [Token(Category = "keyword", Example = "always")]
        Always,

        [Token(Category = "keyword", Example = "analyze")]
        Analyze,

        [Token(Category = "keyword", Example = "analyze")]
        As,

        [Token(Category = "keyword", Example = "asc")]
        Ascending,

        [Token(Category = "keyword", Example = "attach")]
        Attach,

        [Token(Category = "keyword", Example = "autoincrement")]
        AutoIncrement,

        [Token(Category = "keyword", Example = "before")]
        Before,

        [Token(Category = "keyword", Example = "begin")]
        Begin,

        [Token(Category = "keyword", Example = "between")]
        Between,

        [Token(Category = "keyword", Example = "by")]
        By,

        [Token(Category = "keyword", Example = "cascade")]
        Cascade,

        [Token(Category = "keyword", Example = "case")]
        Case,

        [Token(Category = "keyword", Example = "cast")]
        Cast,

        [Token(Category = "keyword", Example = "check")]
        Check,

        [Token(Category = "keyword", Example = "collate")]
        Collate,

        [Token(Category = "keyword", Example = "cplumn")]
        Column,

        [Token(Category = "keyword", Example = "commit")]
        Commit,

        [Token(Category = "keyword", Example = "conflict")]
        Conflict,

        [Token(Category = "keyword", Example = "constraint")]
        Constraint,

        [Token(Category = "keyword", Example = "create")]
        Create,

        [Token(Category = "keyword", Example = "cross")]
        Cross,

        [Token(Category = "keyword", Example = "current_date")]
        CurrentDate,

        [Token(Category = "keyword", Example = "current_time")]
        CurrentTime,

        [Token(Category = "keyword", Example = "current_timestamp")]
        CurrentTimestamp,

        [Token(Category = "keyword", Example = "database")]
        Database,

        [Token(Category = "keyword", Example = "default")]
        Default,

        [Token(Category = "keyword", Example = "deferrable")]
        Deferrable,

        [Token(Category = "keyword", Example = "deferred")]
        Deferred,

        [Token(Category = "keyword", Example = "delete")]
        Delete,

        [Token(Category = "keyword", Example = "desc")]
        Descending,

        [Token(Category = "keyword", Example = "detach")]
        Detach,

        [Token(Category = "keyword", Example = "distinct")]
        Distinct,

        [Token(Category = "keyword", Example = "drop")]
        Drop,

        [Token(Category = "keyword", Example = "each")]
        Each,

        [Token(Category = "keyword", Example = "else")]
        Else,

        [Token(Category = "keyword", Example = "end")]
        End,

        [Token(Category = "keyword", Example = "escape")]
        Escape,

        [Token(Category = "keyword", Example = "except")]
        Except,

        [Token(Category = "keyword", Example = "exclusive")]
        Exclusive,

        [Token(Category = "keyword", Example = "exists")]
        Exists,

        [Token(Category = "keyword", Example = "explain")]
        Explain,

        [Token(Category = "keyword", Example = "fail")]
        Fail,

        [Token(Category = "keyword", Example = "for")]
        For,

        [Token(Category = "keyword", Example = "foreign")]
        Foreign,

        [Token(Category = "keyword", Example = "from")]
        From,

        [Token(Category = "keyword", Example = "full")]
        Full,

        [Token(Category = "keyword", Example = "generated")]
        Generated,

        [Token(Category = "keyword", Example = "glob")]
        Glob,

        [Token(Category = "keyword", Example = "group")]
        Group,

        [Token(Category = "keyword", Example = "having")]
        Having,

        [Token(Category = "keyword", Example = "if")]
        If,

        [Token(Category = "keyword", Example = "ignore")]
        Ignore,

        [Token(Category = "keyword", Example = "immediate")]
        Immediate,

        [Token(Category = "keyword", Example = "in")]
        In,

        [Token(Category = "keyword", Example = "index")]
        Index,

        [Token(Category = "keyword", Example = "indexed")]
        Indexed,

        [Token(Category = "keyword", Example = "initially")]
        Initially,

        [Token(Category = "keyword", Example = "inner")]
        Inner,

        [Token(Category = "keyword", Example = "insert")]
        Insert,

        [Token(Category = "keyword", Example = "instead")]
        Instead,

        [Token(Category = "keyword", Example = "intersect")]
        Intersect,

        [Token(Category = "keyword", Example = "into")]
        Into,

        [Token(Category = "keyword", Example = "isnull")]
        IsNull,

        [Token(Category = "keyword", Example = "join")]
        Join,

        [Token(Category = "keyword", Example = "key")]
        Key,

        [Token(Category = "keyword", Example = "left")]
        Left,

        [Token(Category = "keyword", Example = "limit")]
        Limit,

        [Token(Category = "keyword", Example = "match")]
        Match,

        [Token(Category = "keyword", Example = "natural")]
        Natural,

        [Token(Category = "keyword", Example = "no")]
        No,

        [Token(Category = "keyword", Example = "notnull")]
        NotNull,

        [Token(Category = "keyword", Example = "of")]
        Of,

        [Token(Category = "keyword", Example = "offset")]
        Offset,

        [Token(Category = "keyword", Example = "on")]
        On,

        [Token(Category = "keyword", Example = "order")]
        Order,

        [Token(Category = "keyword", Example = "outer")]
        Outer,

        [Token(Category = "keyword", Example = "plan")]
        Plan,

        [Token(Category = "keyword", Example = "pragma")]
        Pragma,

        [Token(Category = "keyword", Example = "primary")]
        Primary,

        [Token(Category = "keyword", Example = "query")]
        Query,

        [Token(Category = "keyword", Example = "raise")]
        Raise,

        [Token(Category = "keyword", Example = "recursive")]
        Recursive,

        [Token(Category = "keyword", Example = "references")]
        References,

        [Token(Category = "keyword", Example = "regexp")]
        Regexp,

        [Token(Category = "keyword", Example = "reindex")]
        ReIndex,

        [Token(Category = "keyword", Example = "release")]
        Release,

        [Token(Category = "keyword", Example = "rename")]
        Rename,

        [Token(Category = "keyword", Example = "replace")]
        Replace,

        [Token(Category = "keyword", Example = "restrict")]
        Restrict,

        [Token(Category = "keyword", Example = "right")]
        Right,

        [Token(Category = "keyword", Example = "rollback")]
        Rollback,

        [Token(Category = "keyword", Example = "row")]
        Row,

        [Token(Category = "keyword", Example = "savepoint")]
        Savepoint,

        [Token(Category = "keyword", Example = "select")]
        Select,

        [Token(Category = "keyword", Example = "set")]
        Set,

        [Token(Category = "keyword", Example = "stored")]
        Stored,

        [Token(Category = "keyword", Example = "table")]
        Table,

        [Token(Category = "keyword", Example = "temp")]
        Temporary,

        [Token(Category = "keyword", Example = "then")]
        Then,

        [Token(Category = "keyword", Example = "to")]
        To,

        [Token(Category = "keyword", Example = "transaction")]
        Transaction,

        [Token(Category = "keyword", Example = "trigger")]
        Trigger,

        [Token(Category = "keyword", Example = "union")]
        Union,

        [Token(Category = "keyword", Example = "unique")]
        Unique,

        [Token(Category = "keyword", Example = "update")]
        Update,

        [Token(Category = "keyword", Example = "using")]
        Using,

        [Token(Category = "keyword", Example = "vacuum")]
        Vacuum,

        [Token(Category = "keyword", Example = "values")]
        Values,

        [Token(Category = "keyword", Example = "view")]
        View,

        [Token(Category = "keyword", Example = "virtual")]
        Virtual,

        [Token(Category = "keyword", Example = "when")]
        When,

        [Token(Category = "keyword", Example = "where")]
        Where,

        [Token(Category = "keyword", Example = "with")]
        With,

        [Token(Category = "keyword", Example = "without")]
        Without
    }
}
