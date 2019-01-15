using Superpower.Display;

namespace SJP.Schematic.Oracle.Parsing
{
    internal enum OracleToken
    {
        None,

        Identifier,

        [Token(Description = "built-in identifier")]
        BuiltInIdentifier,

        String,

        Number,

        [Token(Description = "money literal")]
        Money,

        [Token(Description = "binary literal")]
        Blob,

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

        [Token(Example = ";")]
        Semicolon,

        [Token(Example = ")")]
        RParen,

        [Token(Example = "?")]
        QuestionMark,

        [Token(Example = "$")]
        Dollar,

        [Token(Example = "\"")]
        DoubleQuote,

        [Token(Category = "operator", Example = "+")]
        Plus,

        [Token(Category = "operator", Example = "+=")]
        PlusEquals,

        [Token(Category = "operator", Example = "-")]
        Minus,

        [Token(Category = "operator", Example = "-=")]
        MinusEquals,

        [Token(Example = "*")]
        Asterisk,

        [Token(Category = "operator", Example = "||")]
        StringConcat,

        [Token(Category = "operator", Example = "*=")]
        MultiplyEquals,

        [Token(Category = "operator", Example = "/")]
        Divide,

        [Token(Category = "operator", Example = "/=")]
        DivideEquals,

        [Token(Category = "operator", Example = "%")]
        Modulo,

        [Token(Category = "operator", Example = "%")]
        ModuloEquals,

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

        [Token(Category = "operator", Example = "!=")]
        NonStandardNotEqual,

        [Token(Category = "operator", Example = "!<")]
        NonStandardNotLessThan,

        [Token(Category = "operator", Example = "!>")]
        NonStandardNotGreaterThan,

        [Token(Category = "operator", Example = "&")]
        BitwiseAnd,

        [Token(Category = "operator", Example = "&=")]
        BitwiseAndEqual,

        [Token(Category = "operator", Example = "|")]
        BitwiseOr,

        [Token(Category = "operator", Example = "|=")]
        BitwiseOrEqual,

        [Token(Category = "operator", Example = "^")]
        BitwiseXor,

        [Token(Category = "operator", Example = "^=")]
        BitwiseXorEqual,

        [Token(Category = "operator", Example = "::")]
        Scope,

        [Token(Category = "keyword", Example = "add")]
        Add,

        [Token(Category = "keyword", Example = "all")]
        All,

        [Token(Category = "keyword", Example = "alter")]
        Alter,

        [Token(Category = "keyword", Example = "and")]
        And,

        [Token(Category = "keyword", Example = "any")]
        Any,

        [Token(Category = "keyword", Example = "as")]
        As,

        [Token(Category = "keyword", Example = "asc")]
        Ascending,

        [Token(Category = "keyword", Example = "authorization")]
        Authorization,

        [Token(Category = "keyword", Example = "backup")]
        Backup,

        [Token(Category = "keyword", Example = "begin")]
        Begin,

        [Token(Category = "keyword", Example = "between")]
        Between,

        [Token(Category = "keyword", Example = "body")]
        Body,

        [Token(Category = "keyword", Example = "break")]
        Break,

        [Token(Category = "keyword", Example = "browse")]
        Browse,

        [Token(Category = "keyword", Example = "bulk")]
        Bulk,

        [Token(Category = "keyword", Example = "by")]
        By,

        [Token(Category = "keyword", Example = "cascade")]
        Cascade,

        [Token(Category = "keyword", Example = "case")]
        Case,

        [Token(Category = "keyword", Example = "check")]
        Check,

        [Token(Category = "keyword", Example = "checkpoint")]
        Checkpoint,

        [Token(Category = "keyword", Example = "close")]
        Close,

        [Token(Category = "keyword", Example = "clustered")]
        Clustered,

        [Token(Category = "keyword", Example = "coalesce")]
        Coalesce,

        [Token(Category = "keyword", Example = "collate")]
        Collate,

        [Token(Category = "keyword", Example = "column")]
        Column,

        [Token(Category = "keyword", Example = "commit")]
        Commit,

        [Token(Category = "keyword", Example = "compute")]
        Compute,

        [Token(Category = "keyword", Example = "constraint")]
        Constraint,

        [Token(Category = "keyword", Example = "contains")]
        Contains,

        [Token(Category = "keyword", Example = "containstable")]
        ContainsTable,

        [Token(Category = "keyword", Example = "continue")]
        Continue,

        [Token(Category = "keyword", Example = "convert")]
        Convert,

        [Token(Category = "keyword", Example = "create")]
        Create,

        [Token(Category = "keyword", Example = "cross")]
        Cross,

        [Token(Category = "keyword", Example = "current")]
        Current,

        [Token(Category = "keyword", Example = "current_date")]
        CurrentDate,

        [Token(Category = "keyword", Example = "current_time")]
        CurrentTime,

        [Token(Category = "keyword", Example = "current_timestamp")]
        CurrentTimestamp,

        [Token(Category = "keyword", Example = "current_user")]
        CurrentUser,

        [Token(Category = "keyword", Example = "cursor")]
        Cursor,

        [Token(Category = "keyword", Example = "database")]
        Database,

        [Token(Category = "keyword", Example = "dbcc")]
        Dbcc,

        [Token(Category = "keyword", Example = "deallocate")]
        Deallocate,

        [Token(Category = "keyword", Example = "declare")]
        Declare,

        [Token(Category = "keyword", Example = "default")]
        Default,

        [Token(Category = "keyword", Example = "delete")]
        Delete,

        [Token(Category = "keyword", Example = "deny")]
        Deny,

        [Token(Category = "keyword", Example = "desc")]
        Descending,

        [Token(Category = "keyword", Example = "disk")]
        Disk,

        [Token(Category = "keyword", Example = "distinct")]
        Distinct,

        [Token(Category = "keyword", Example = "distributed")]
        Distributed,

        [Token(Category = "keyword", Example = "double")]
        Double,

        [Token(Category = "keyword", Example = "drop")]
        Drop,

        [Token(Category = "keyword", Example = "dump")]
        Dump,

        [Token(Category = "keyword", Example = "else")]
        Else,

        [Token(Category = "keyword", Example = "end")]
        End,

        [Token(Category = "keyword", Example = "errlvl")]
        ErrorLevel,

        [Token(Category = "keyword", Example = "escape")]
        Escape,

        [Token(Category = "keyword", Example = "except")]
        Except,

        [Token(Category = "keyword", Example = "exec")]
        Exec,

        [Token(Category = "keyword", Example = "execute")]
        Execute,

        [Token(Category = "keyword", Example = "exists")]
        Exists,

        [Token(Category = "keyword", Example = "exit")]
        Exit,

        [Token(Category = "keyword", Example = "external")]
        External,

        [Token(Category = "keyword", Example = "fetch")]
        Fetch,

        [Token(Category = "keyword", Example = "file")]
        File,

        [Token(Category = "keyword", Example = "fillfactor")]
        FillFactor,

        [Token(Category = "keyword", Example = "for")]
        For,

        [Token(Category = "keyword", Example = "foreign")]
        Foreign,

        [Token(Category = "keyword", Example = "freetext")]
        FreeText,

        [Token(Category = "keyword", Example = "freetexttable")]
        FreeTextTable,

        [Token(Category = "keyword", Example = "from")]
        From,

        [Token(Category = "keyword", Example = "full")]
        Full,

        [Token(Category = "keyword", Example = "function")]
        Function,

        [Token(Category = "keyword", Example = "goto")]
        Goto,

        [Token(Category = "keyword", Example = "grant")]
        Grant,

        [Token(Category = "keyword", Example = "group")]
        Group,

        [Token(Category = "keyword", Example = "having")]
        Having,

        [Token(Category = "keyword", Example = "holdlock")]
        HoldLock,

        [Token(Category = "keyword", Example = "identity")]
        Identity,

        [Token(Category = "keyword", Example = "identity_insert")]
        IdentityInsert,

        [Token(Category = "keyword", Example = "identitycol")]
        IdentityCol,

        [Token(Category = "keyword", Example = "if")]
        If,

        [Token(Category = "keyword", Example = "in")]
        In,

        [Token(Category = "keyword", Example = "index")]
        Index,

        [Token(Category = "keyword", Example = "inner")]
        Inner,

        [Token(Category = "keyword", Example = "insert")]
        Insert,

        [Token(Category = "keyword", Example = "intersect")]
        Intersect,

        [Token(Category = "keyword", Example = "into")]
        Into,

        [Token(Category = "keyword", Example = "is")]
        Is,

        [Token(Category = "keyword", Example = "join")]
        Join,

        [Token(Category = "keyword", Example = "key")]
        Key,

        [Token(Category = "keyword", Example = "kill")]
        Kill,

        [Token(Category = "keyword", Example = "left")]
        Left,

        [Token(Category = "keyword", Example = "like")]
        Like,

        [Token(Category = "keyword", Example = "lineno")]
        LineNumber,

        [Token(Category = "keyword", Example = "load")]
        Load,

        [Token(Category = "keyword", Example = "merge")]
        Merge,

        [Token(Category = "keyword", Example = "national")]
        National,

        [Token(Category = "keyword", Example = "nocheck")]
        NoCheck,

        [Token(Category = "keyword", Example = "nonclustered")]
        NonClustered,

        [Token(Category = "keyword", Example = "not")]
        Not,

        [Token(Category = "keyword", Example = "null")]
        Null,

        [Token(Category = "keyword", Example = "nullif")]
        Nullif,

        [Token(Category = "keyword", Example = "of")]
        Of,

        [Token(Category = "keyword", Example = "off")]
        Off,

        [Token(Category = "keyword", Example = "offsets")]
        Offsets,

        [Token(Category = "keyword", Example = "on")]
        On,

        [Token(Category = "keyword", Example = "open")]
        Open,

        [Token(Category = "keyword", Example = "opendatasource")]
        OpenDataSource,

        [Token(Category = "keyword", Example = "openquery")]
        OpenQuery,

        [Token(Category = "keyword", Example = "openrowset")]
        OpenRowSet,

        [Token(Category = "keyword", Example = "openxml")]
        OpenXml,

        [Token(Category = "keyword", Example = "option")]
        Option,

        [Token(Category = "keyword", Example = "or")]
        Or,

        [Token(Category = "keyword", Example = "order")]
        Order,

        [Token(Category = "keyword", Example = "outer")]
        Outer,

        [Token(Category = "keyword", Example = "over")]
        Over,

        [Token(Category = "keyword", Example = "package")]
        Package,

        [Token(Category = "keyword", Example = "percent")]
        Percent,

        [Token(Category = "keyword", Example = "pivot")]
        Pivot,

        [Token(Category = "keyword", Example = "plan")]
        Plan,

        [Token(Category = "keyword", Example = "precision")]
        Precision,

        [Token(Category = "keyword", Example = "primary")]
        Primary,

        [Token(Category = "keyword", Example = "print")]
        Print,

        [Token(Category = "keyword", Example = "proc")]
        Proc,

        [Token(Category = "keyword", Example = "procedure")]
        Procedure,

        [Token(Category = "keyword", Example = "public")]
        Public,

        [Token(Category = "keyword", Example = "raiserror")]
        Raiserror,

        [Token(Category = "keyword", Example = "read")]
        Read,

        [Token(Category = "keyword", Example = "readtext")]
        ReadText,

        [Token(Category = "keyword", Example = "reconfigure")]
        Reconfigure,

        [Token(Category = "keyword", Example = "references")]
        References,

        [Token(Category = "keyword", Example = "replace")]
        Replace,

        [Token(Category = "keyword", Example = "replication")]
        Replication,

        [Token(Category = "keyword", Example = "restore")]
        Restore,

        [Token(Category = "keyword", Example = "restrict")]
        Restrict,

        [Token(Category = "keyword", Example = "return")]
        Return,

        [Token(Category = "keyword", Example = "revert")]
        Revert,

        [Token(Category = "keyword", Example = "revoke")]
        Revoke,

        [Token(Category = "keyword", Example = "right")]
        Right,

        [Token(Category = "keyword", Example = "rollback")]
        Rollback,

        [Token(Category = "keyword", Example = "rowcount")]
        RowCount,

        [Token(Category = "keyword", Example = "rowguidcol")]
        RowGuidCol,

        [Token(Category = "keyword", Example = "rule")]
        Rule,

        [Token(Category = "keyword", Example = "save")]
        Save,

        [Token(Category = "keyword", Example = "schema")]
        Schema,

        [Token(Category = "keyword", Example = "securityaudit")]
        SecurityAudit,

        [Token(Category = "keyword", Example = "select")]
        Select,

        [Token(Category = "keyword", Example = "semantickeyphrasetable")]
        SemanticKeyPhraseTable,

        [Token(Category = "keyword", Example = "semanticsimilaritydetailstable")]
        SemanticSimilarityDetailsTable,

        [Token(Category = "keyword", Example = "semanticsimilaritytable")]
        SemanticSimilarityTable,

        [Token(Category = "keyword", Example = "session_user")]
        SessionUser,

        [Token(Category = "keyword", Example = "set")]
        Set,

        [Token(Category = "keyword", Example = "setuser")]
        SetUser,

        [Token(Category = "keyword", Example = "shutdown")]
        Shutdown,

        [Token(Category = "keyword", Example = "some")]
        Some,

        [Token(Category = "keyword", Example = "statistics")]
        Statistics,

        [Token(Category = "keyword", Example = "system_user")]
        SystemUser,

        [Token(Category = "keyword", Example = "table")]
        Table,

        [Token(Category = "keyword", Example = "tablesample")]
        TableSample,

        [Token(Category = "keyword", Example = "textsize")]
        TextSize,

        [Token(Category = "keyword", Example = "then")]
        Then,

        [Token(Category = "keyword", Example = "to")]
        To,

        [Token(Category = "keyword", Example = "top")]
        Top,

        [Token(Category = "keyword", Example = "tran")]
        Tran,

        [Token(Category = "keyword", Example = "transaction")]
        Transaction,

        [Token(Category = "keyword", Example = "trigger")]
        Trigger,

        [Token(Category = "keyword", Example = "truncate")]
        Truncate,

        [Token(Category = "keyword", Example = "try_convert")]
        TryConvert,

        [Token(Category = "keyword", Example = "tsequal")]
        Tsequal,

        [Token(Category = "keyword", Example = "union")]
        Union,

        [Token(Category = "keyword", Example = "unique")]
        Unique,

        [Token(Category = "keyword", Example = "unpivot")]
        Unpivot,

        [Token(Category = "keyword", Example = "update")]
        Update,

        [Token(Category = "keyword", Example = "updatetext")]
        UpdateText,

        [Token(Category = "keyword", Example = "use")]
        Use,

        [Token(Category = "keyword", Example = "user")]
        User,

        [Token(Category = "keyword", Example = "values")]
        Values,

        [Token(Category = "keyword", Example = "varying")]
        Varying,

        [Token(Category = "keyword", Example = "view")]
        View,

        [Token(Category = "keyword", Example = "waitfor")]
        WaitFor,

        [Token(Category = "keyword", Example = "when")]
        When,

        [Token(Category = "keyword", Example = "where")]
        Where,

        [Token(Category = "keyword", Example = "while")]
        While,

        [Token(Category = "keyword", Example = "with")]
        With,

        [Token(Category = "keyword", Example = "within")]
        Within,

        [Token(Category = "keyword", Example = "writetext")]
        WriteText
    }
}
