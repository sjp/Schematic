using Superpower.Display;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A token used to capture information from SQL as known to SQLite.
/// </summary>
public enum SqliteToken
{
    /// <summary>
    /// A none/unknown token value.
    /// </summary>
    None,

    /// <summary>
    /// An identifier.
    /// </summary>
    Identifier,

    /// <summary>
    /// A built-in identifier.
    /// </summary>
    [Token(Description = "built-in identifier")]
    BuiltInIdentifier,

    /// <summary>
    /// A string.
    /// </summary>
    String,

    /// <summary>
    /// A regular expression.
    /// </summary>
    [Token(Description = "regular expression")]
    RegularExpression,

    /// <summary>
    /// A decimal number.
    /// </summary>
    Number,

    /// <summary>
    /// A hexadecimal number.
    /// </summary>
    [Token(Description = "hexadecimal number")]
    HexNumber,

    /// <summary>
    /// A blob literal, representing a sequence of bytes.
    /// </summary>
    [Token(Description = "blob literal")]
    Blob,

    /// <summary>
    /// The ampersand symbol.
    /// </summary>
    [Token(Example = "&")]
    Ampersand,

    /// <summary>
    /// The backtick symbol.
    /// </summary>
    [Token(Example = "`")]
    Backtick,

    /// <summary>
    /// The comma symbol.
    /// </summary>
    [Token(Example = ",")]
    Comma,

    /// <summary>
    /// The dot/period symbol.
    /// </summary>
    [Token(Example = ".")]
    Period,

    /// <summary>
    /// A left square bracket symbol.
    /// </summary>
    [Token(Example = "[")]
    LBracket,

    /// <summary>
    /// A right square bracket symbol.
    /// </summary>
    [Token(Example = "]")]
    RBracket,

    /// <summary>
    /// A left parenthesis.
    /// </summary>
    [Token(Example = "(")]
    LParen,

    /// <summary>
    /// A right parenthesis.
    /// </summary>
    [Token(Example = ")")]
    RParen,

    /// <summary>
    /// The question mark symbol.
    /// </summary>
    [Token(Example = "?")]
    QuestionMark,

    /// <summary>
    /// The double quote symbol.
    /// </summary>
    [Token(Example = "\"")]
    DoubleQuote,

    /// <summary>
    /// The pipe or vertical bar symbol.
    /// </summary>
    [Token(Example = "|")]
    Pipe,

    /// <summary>
    /// The tilde symbol.
    /// </summary>
    [Token(Example = "~")]
    Tilde,

    /// <summary>
    /// The semicolon symbol.
    /// </summary>
    [Token(Example = ";")]
    Semicolon,

    /// <summary>
    /// The <c>+</c> (plus/addition) operator.
    /// </summary>
    [Token(Category = "operator", Example = "+")]
    Plus,

    /// <summary>
    /// The <c>-</c> (minus/subtraction) operator.
    /// </summary>
    [Token(Category = "operator", Example = "-")]
    Minus,

    /// <summary>
    /// The asterisk symbol.
    /// </summary>
    [Token(Example = "*")]
    Asterisk,

    /// <summary>
    /// The <c>/</c> operator.
    /// </summary>
    [Token(Category = "operator", Example = "/")]
    ForwardSlash,

    /// <summary>
    /// The <c>%</c> operator.
    /// </summary>
    [Token(Category = "operator", Example = "%")]
    Percent,

    /// <summary>
    /// The <c>^</c> operator.
    /// </summary>
    [Token(Category = "operator", Example = "^")]
    Caret,

    /// <summary>
    /// The <c>&lt;</c> (less than) operator.
    /// </summary>
    [Token(Category = "operator", Example = "<")]
    LessThan,

    /// <summary>
    /// The <c>&lt;=</c> (less than or equal) operator.
    /// </summary>
    [Token(Category = "operator", Example = "<=")]
    LessThanOrEqual,

    /// <summary>
    /// The <c>&gt;</c> (greater than) operator.
    /// </summary>
    [Token(Category = "operator", Example = ">")]
    GreaterThan,

    /// <summary>
    /// The <c>&gt;=</c> (greater than or equal) operator.
    /// </summary>
    [Token(Category = "operator", Example = ">=")]
    GreaterThanOrEqual,

    /// <summary>
    /// The <c>=</c> (equals) operator.
    /// </summary>
    [Token(Category = "operator", Example = "=")]
    Equal,

    /// <summary>
    /// The <c>&lt;&gt;</c> (not equals) operator.
    /// </summary>
    [Token(Category = "operator", Example = "<>")]
    NotEqual,

    /// <summary>
    /// The <c>&lt;&lt;</c> (left shift) operator.
    /// </summary>
    [Token(Category = "operator", Example = "<<")]
    LeftShift,

    /// <summary>
    /// The <c>&gt;&gt;</c> (right shift) operator.
    /// </summary>
    [Token(Category = "operator", Example = ">>")]
    RightShift,

    /// <summary>
    /// The <c>and</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "and")]
    And,

    /// <summary>
    /// The <c>is</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "is")]
    Is,

    /// <summary>
    /// The <c>like</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "like")]
    Like,

    /// <summary>
    /// The <c>not</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "not")]
    Not,

    /// <summary>
    /// The <c>or</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "or")]
    Or,

    /// <summary>
    /// The <c>true</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "true")]
    True,

    /// <summary>
    /// The <c>false</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "false")]
    False,

    /// <summary>
    /// The <c>null</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "null")]
    Null,

    /// <summary>
    /// The <c>abort</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "abort")]
    Abort,

    /// <summary>
    /// The <c>action</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "action")]
    Action,

    /// <summary>
    /// The <c>add</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "add")]
    Add,

    /// <summary>
    /// The <c>after</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "after")]
    After,

    /// <summary>
    /// The <c>all</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "all")]
    All,

    /// <summary>
    /// The <c>alter</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "alter")]
    Alter,

    /// <summary>
    /// The <c>always</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "always")]
    Always,

    /// <summary>
    /// The <c>analyze</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "analyze")]
    Analyze,

    /// <summary>
    /// The <c>as</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "as")]
    As,

    /// <summary>
    /// The <c>asc</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "asc")]
    Ascending,

    /// <summary>
    /// The <c>attach</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "attach")]
    Attach,

    /// <summary>
    /// The <c>autoincrement</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "autoincrement")]
    AutoIncrement,

    /// <summary>
    /// The <c>before</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "before")]
    Before,

    /// <summary>
    /// The <c>begin</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "begin")]
    Begin,

    /// <summary>
    /// The <c>between</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "between")]
    Between,

    /// <summary>
    /// The <c>by</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "by")]
    By,

    /// <summary>
    /// The <c>cascade</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "cascade")]
    Cascade,

    /// <summary>
    /// The <c>case</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "case")]
    Case,

    /// <summary>
    /// The <c>cast</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "cast")]
    Cast,

    /// <summary>
    /// The <c>check</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "check")]
    Check,

    /// <summary>
    /// The <c>collate</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "collate")]
    Collate,

    /// <summary>
    /// The <c>column</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "column")]
    Column,

    /// <summary>
    /// The <c>commit</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "commit")]
    Commit,

    /// <summary>
    /// The <c>conflict</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "conflict")]
    Conflict,

    /// <summary>
    /// The <c>constraint</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "constraint")]
    Constraint,

    /// <summary>
    /// The <c>create</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "create")]
    Create,

    /// <summary>
    /// The <c>cross</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "cross")]
    Cross,

    /// <summary>
    /// The <c>current_date</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "current_date")]
    CurrentDate,

    /// <summary>
    /// The <c>current_time</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "current_time")]
    CurrentTime,

    /// <summary>
    /// The <c>current_timestamp</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "current_timestamp")]
    CurrentTimestamp,

    /// <summary>
    /// The <c>database</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "database")]
    Database,

    /// <summary>
    /// The <c>default</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "default")]
    Default,

    /// <summary>
    /// The <c>deferrable</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "deferrable")]
    Deferrable,

    /// <summary>
    /// The <c>deferred</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "deferred")]
    Deferred,

    /// <summary>
    /// The <c>delete</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "delete")]
    Delete,

    /// <summary>
    /// The <c>desc</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "desc")]
    Descending,

    /// <summary>
    /// The <c>detach</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "detach")]
    Detach,

    /// <summary>
    /// The <c>distinct</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "distinct")]
    Distinct,

    /// <summary>
    /// The <c>drop</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "drop")]
    Drop,

    /// <summary>
    /// The <c>each</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "each")]
    Each,

    /// <summary>
    /// The <c>else</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "else")]
    Else,

    /// <summary>
    /// The <c>end</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "end")]
    End,

    /// <summary>
    /// The <c>escape</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "escape")]
    Escape,

    /// <summary>
    /// The <c>except</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "except")]
    Except,

    /// <summary>
    /// The <c>exclusive</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "exclusive")]
    Exclusive,

    /// <summary>
    /// The <c>exists</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "exists")]
    Exists,

    /// <summary>
    /// The <c>explain</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "explain")]
    Explain,

    /// <summary>
    /// The <c>fail</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "fail")]
    Fail,

    /// <summary>
    /// The <c>for</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "for")]
    For,

    /// <summary>
    /// The <c>foreign</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "foreign")]
    Foreign,

    /// <summary>
    /// The <c>from</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "from")]
    From,

    /// <summary>
    /// The <c>full</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "full")]
    Full,

    /// <summary>
    /// The <c>generated</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "generated")]
    Generated,

    /// <summary>
    /// The <c>glob</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "glob")]
    Glob,

    /// <summary>
    /// The <c>group</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "group")]
    Group,

    /// <summary>
    /// The <c>having</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "having")]
    Having,

    /// <summary>
    /// The <c>if</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "if")]
    If,

    /// <summary>
    /// The <c>ignore</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "ignore")]
    Ignore,

    /// <summary>
    /// The <c>immediate</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "immediate")]
    Immediate,

    /// <summary>
    /// The <c>in</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "in")]
    In,

    /// <summary>
    /// The <c>index</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "index")]
    Index,

    /// <summary>
    /// The <c>indexed</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "indexed")]
    Indexed,

    /// <summary>
    /// The <c>initially</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "initially")]
    Initially,

    /// <summary>
    /// The <c>inner</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "inner")]
    Inner,

    /// <summary>
    /// The <c>insert</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "insert")]
    Insert,

    /// <summary>
    /// The <c>instead</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "instead")]
    Instead,

    /// <summary>
    /// The <c>intersect</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "intersect")]
    Intersect,

    /// <summary>
    /// The <c>into</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "into")]
    Into,

    /// <summary>
    /// The <c>isnull</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "isnull")]
    IsNull,

    /// <summary>
    /// The <c>join</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "join")]
    Join,

    /// <summary>
    /// The <c>key</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "key")]
    Key,

    /// <summary>
    /// The <c>left</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "left")]
    Left,

    /// <summary>
    /// The <c>limit</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "limit")]
    Limit,

    /// <summary>
    /// The <c>match</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "match")]
    Match,

    /// <summary>
    /// The <c>natural</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "natural")]
    Natural,

    /// <summary>
    /// The <c>no</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "no")]
    No,

    /// <summary>
    /// The <c>notnull</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "notnull")]
    NotNull,

    /// <summary>
    /// The <c>of</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "of")]
    Of,

    /// <summary>
    /// The <c>offset</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "offset")]
    Offset,

    /// <summary>
    /// The <c>on</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "on")]
    On,

    /// <summary>
    /// The <c>order</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "order")]
    Order,

    /// <summary>
    /// The <c>outer</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "outer")]
    Outer,

    /// <summary>
    /// The <c>plan</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "plan")]
    Plan,

    /// <summary>
    /// The <c>pragma</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "pragma")]
    Pragma,

    /// <summary>
    /// The <c>primary</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "primary")]
    Primary,

    /// <summary>
    /// The <c>query</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "query")]
    Query,

    /// <summary>
    /// The <c>raise</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "raise")]
    Raise,

    /// <summary>
    /// The <c>recursive</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "recursive")]
    Recursive,

    /// <summary>
    /// The <c>references</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "references")]
    References,

    /// <summary>
    /// The <c>regexp</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "regexp")]
    Regexp,

    /// <summary>
    /// The <c>reindex</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "reindex")]
    ReIndex,

    /// <summary>
    /// The <c>release</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "release")]
    Release,

    /// <summary>
    /// The <c>rename</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "rename")]
    Rename,

    /// <summary>
    /// The <c>replace</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "replace")]
    Replace,

    /// <summary>
    /// The <c>restrict</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "restrict")]
    Restrict,

    /// <summary>
    /// The <c>right</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "right")]
    Right,

    /// <summary>
    /// The <c>rollback</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "rollback")]
    Rollback,

    /// <summary>
    /// The <c>row</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "row")]
    Row,

    /// <summary>
    /// The <c>savepoint</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "savepoint")]
    Savepoint,

    /// <summary>
    /// The <c>select</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "select")]
    Select,

    /// <summary>
    /// The <c>set</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "set")]
    Set,

    /// <summary>
    /// The <c>stored</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "stored")]
    Stored,

    /// <summary>
    /// The <c>table</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "table")]
    Table,

    /// <summary>
    /// The <c>temp</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "temp")]
    Temporary,

    /// <summary>
    /// The <c>then</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "then")]
    Then,

    /// <summary>
    /// The <c>to</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "to")]
    To,

    /// <summary>
    /// The <c>transaction</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "transaction")]
    Transaction,

    /// <summary>
    /// The <c>trigger</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "trigger")]
    Trigger,

    /// <summary>
    /// The <c>union</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "union")]
    Union,

    /// <summary>
    /// The <c>unique</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "unique")]
    Unique,

    /// <summary>
    /// The <c>update</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "update")]
    Update,

    /// <summary>
    /// The <c>using</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "using")]
    Using,

    /// <summary>
    /// The <c>vacuum</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "vacuum")]
    Vacuum,

    /// <summary>
    /// The <c>values</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "values")]
    Values,

    /// <summary>
    /// The <c>view</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "view")]
    View,

    /// <summary>
    /// The <c>virtual</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "virtual")]
    Virtual,

    /// <summary>
    /// The <c>when</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "when")]
    When,

    /// <summary>
    /// The <c>where</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "where")]
    Where,

    /// <summary>
    /// The <c>with</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "with")]
    With,

    /// <summary>
    /// The <c>without</c> keyword.
    /// </summary>
    [Token(Category = "keyword", Example = "without")]
    Without
}