namespace SJP.Schematic.Core;

/// <summary>
/// Describes the physical structure used to implement an index.
/// </summary>
public enum IndexType
{
    /// <summary>
    /// The index structure is not known, either because the database does not report it,
    /// or because it is a structure that this enumeration does not describe.
    /// </summary>
    Unknown,

    /// <summary>
    /// A balanced-tree index. This is the default index structure in every supported database,
    /// and corresponds to a non-clustered rowstore index in SQL Server.
    /// </summary>
    BTree,

    /// <summary>
    /// An index whose leaf level contains the table's rows, i.e. a SQL Server clustered rowstore
    /// index or an Oracle index-organized table.
    /// </summary>
    Clustered,

    /// <summary>
    /// A hash index, supporting equality lookups only.
    /// </summary>
    Hash,

    /// <summary>
    /// A bitmap index, as used by Oracle for low-cardinality columns.
    /// </summary>
    Bitmap,

    /// <summary>
    /// A columnstore index, i.e. one storing data column-wise instead of row-wise.
    /// </summary>
    ColumnStore,

    /// <summary>
    /// A full-text index, supporting word and phrase searching within text.
    /// </summary>
    FullText,

    /// <summary>
    /// A spatial index, supporting queries over geometric or geographic values.
    /// </summary>
    Spatial,

    /// <summary>
    /// An index over the structure of XML documents.
    /// </summary>
    Xml,

    /// <summary>
    /// A PostgreSQL generalised inverted index, typically used to index the elements
    /// of composite values such as arrays or documents.
    /// </summary>
    Gin,

    /// <summary>
    /// A PostgreSQL generalised search tree index, a framework for building index structures
    /// over arbitrary data types.
    /// </summary>
    Gist,

    /// <summary>
    /// A PostgreSQL block range index, storing summaries of the values in physically adjacent
    /// ranges of table blocks.
    /// </summary>
    Brin,

    /// <summary>
    /// An index structure that the database reports and that this enumeration does not describe,
    /// e.g. an Oracle domain index or a PostgreSQL index provided by an extension.
    /// </summary>
    Other,
}
