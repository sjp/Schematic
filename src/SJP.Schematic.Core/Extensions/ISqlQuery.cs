namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// A marker interface to enable strongly-typed results when pairing request parameters and results.
/// </summary>
/// <typeparam name="TResult">The result type from the SQL query.</typeparam>
#pragma warning disable S2326 // Unused type parameters should be removed
public interface ISqlQuery<TResult> where TResult : notnull
#pragma warning restore S2326 // Unused type parameters should be removed
{
}