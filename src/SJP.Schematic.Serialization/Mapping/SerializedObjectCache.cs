using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// The serialized objects already produced during one mapping pass, keyed by the instance they were
/// mapped from.
/// </summary>
/// <remarks>
/// Database providers hand out the same column, type, key and index instances wherever an object is
/// referenced: a table's primary key holds the table's own columns, and every foreign key to that
/// table holds that same primary key. Reusing the serialized object for an instance mapped earlier
/// keeps the serialized graph as small as the source one. The written document is unchanged, because
/// the serializer writes a shared object out in full at every reference. The cache is not thread-safe.
/// </remarks>
internal sealed class SerializedObjectCache
{
    public Dictionary<IDatabaseColumn, Dto.DatabaseColumn> Columns { get; } = new(ReferenceEqualityComparer.Instance);

    public Dictionary<IDbType, Dto.DbType> Types { get; } = new(ReferenceEqualityComparer.Instance);

    public Dictionary<IDatabaseKey, Dto.DatabaseKey> Keys { get; } = new(ReferenceEqualityComparer.Instance);

    public Dictionary<IDatabaseIndex, Dto.DatabaseIndex> Indexes { get; } = new(ReferenceEqualityComparer.Instance);

    public Dictionary<IDatabaseRelationalKey, Dto.DatabaseRelationalKey> RelationalKeys { get; } = new(ReferenceEqualityComparer.Instance);
}
