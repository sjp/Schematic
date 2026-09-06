using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The user-defined types summary payload (<c>data/userDefinedTypes.json</c>): the types declared
/// by users within the database, e.g. a PostgreSQL enum or a SQL Server table type.
/// </summary>
public sealed class UserDefinedTypes
{
    public UserDefinedTypes(IEnumerable<Main.UserDefinedType> userDefinedTypes)
    {
        if (userDefinedTypes.NullOrAnyNull())
            throw new ArgumentNullException(nameof(userDefinedTypes));

        UserDefinedTypesCount = userDefinedTypes.UCount();
        AllUserDefinedTypes = userDefinedTypes;
    }

    public uint UserDefinedTypesCount { get; }

    public IEnumerable<Main.UserDefinedType> AllUserDefinedTypes { get; }
}
