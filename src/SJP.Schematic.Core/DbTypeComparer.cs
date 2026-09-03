using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Compares column data types for equality by the information they carry, rather than by reference
/// or by the definition text a database happens to print.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IDbType.Definition"/> is formatted by the dialect that created the type, so two types
/// describing the same storage can differ only in casing or spacing, e.g. <c>VARCHAR(50)</c> and
/// <c>varchar(50)</c>. <see cref="IDbType.ClrType"/> is likewise a mapping decision rather than part
/// of the type. Neither takes part in a comparison; type names and collations are compared
/// case-insensitively, as identifiers are elsewhere.
/// </para>
/// <para>
/// Types are only comparable when they come from the same database, or from databases of the same
/// dialect. Comparing types across dialects is what <see cref="IDbTypeProvider.GetComparableColumnType"/>
/// is for: translate one type into the other's dialect first, then compare the results.
/// </para>
/// </remarks>
/// <seealso cref="IDbType"/>
public sealed class DbTypeComparer : IEqualityComparer<IDbType>
{
    private DbTypeComparer(bool compareShape, bool compareCollation)
    {
        _compareShape = compareShape;
        _compareCollation = compareCollation;
    }

    /// <summary>
    /// Gets a comparer that treats two types as equal when they have the same name, shape and collation.
    /// </summary>
    /// <value>A <see cref="DbTypeComparer"/> object.</value>
    public static DbTypeComparer Structural { get; } = new DbTypeComparer(compareShape: true, compareCollation: true);

    /// <summary>
    /// Gets a comparer that treats two types as equal when they have the same name and shape, whatever
    /// their collations.
    /// </summary>
    /// <value>A <see cref="DbTypeComparer"/> object.</value>
    /// <remarks>
    /// A collation determines how stored values sort and compare, not what can be stored, so a
    /// difference in collation is usually worth reporting on its own rather than as a difference of type.
    /// </remarks>
    public static DbTypeComparer StructuralIgnoringCollation { get; } = new DbTypeComparer(compareShape: true, compareCollation: false);

    /// <summary>
    /// Gets a comparer that treats two types as equal when they have the same name, whatever their
    /// lengths, precisions or collations.
    /// </summary>
    /// <value>A <see cref="DbTypeComparer"/> object.</value>
    public static DbTypeComparer NameOnly { get; } = new DbTypeComparer(compareShape: false, compareCollation: false);

    /// <summary>
    /// Determines whether two column data types are equal.
    /// </summary>
    /// <param name="x">The first column data type to compare.</param>
    /// <param name="y">The second column data type to compare.</param>
    /// <returns><see langword="true" /> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals(IDbType? x, IDbType? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;

        if (!IdentifierComparer.OrdinalIgnoreCase.Equals(x.TypeName, y.TypeName))
            return false;

        if (_compareCollation && !IdentifierComparer.OrdinalIgnoreCase.Equals(GetCollation(x), GetCollation(y)))
            return false;

        if (!_compareShape)
            return true;

        return x.DataType == y.DataType
            && x.MaxLength == y.MaxLength
            && x.IsFixedLength == y.IsFixedLength
            && x.IsUnsigned == y.IsUnsigned
            && PrecisionsEqual(x.NumericPrecision, y.NumericPrecision)
            && x.EnumValues.SequenceEqual(y.EnumValues, StringComparer.Ordinal)
            && Equals(GetInnerType(x.ElementType), GetInnerType(y.ElementType))
            && Equals(GetInnerType(x.BaseType), GetInnerType(y.BaseType));
    }

    /// <summary>
    /// Returns a hash code for a column data type.
    /// </summary>
    /// <param name="obj">A column data type.</param>
    /// <returns>A hash code for a column data type, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(IDbType obj)
    {
        if (obj == null)
            return 0;

        var hashCode = new HashCode();
        hashCode.Add(obj.TypeName, IdentifierComparer.OrdinalIgnoreCase);

        if (_compareCollation)
        {
            var collation = GetCollation(obj);
            hashCode.Add(collation != null ? IdentifierComparer.OrdinalIgnoreCase.GetHashCode(collation) : 0);
        }

        if (_compareShape)
        {
            // element and base types take no part: the types that have one are rare enough that
            // hashing it would cost every other type more than it saves them
            hashCode.Add(obj.DataType);
            hashCode.Add(obj.MaxLength);
            hashCode.Add(obj.IsFixedLength);
            hashCode.Add(obj.IsUnsigned);
            hashCode.Add(obj.EnumValues.Count);
            obj.NumericPrecision.IfSome(np =>
            {
                hashCode.Add(np.Precision);
                hashCode.Add(np.Scale);
            });
        }

        return hashCode.ToHashCode();
    }

    private static bool PrecisionsEqual(Option<INumericPrecision> x, Option<INumericPrecision> y)
    {
        var xPrecision = x.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);
        var yPrecision = y.MatchUnsafe(static np => np, static () => (INumericPrecision?)null);

        if (xPrecision == null || yPrecision == null)
            return xPrecision == null && yPrecision == null;

        return xPrecision.Precision == yPrecision.Precision
            && xPrecision.Scale == yPrecision.Scale;
    }

    private static Identifier? GetCollation(IDbType dbType) => dbType.Collation.MatchUnsafe(static c => c, static () => (Identifier?)null);

    private static IDbType? GetInnerType(Option<IDbType> dbType) => dbType.MatchUnsafe(static t => t, static () => (IDbType?)null);

    private readonly bool _compareShape;
    private readonly bool _compareCollation;
}
