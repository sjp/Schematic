namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// A marker interface to enable strongly-typed results when pairing request parameters and results.
/// </summary>
/// <typeparam name="TResult">The result type from the SQL query.</typeparam>
public interface ISqlQuery<TResult> where TResult : notnull
{
}