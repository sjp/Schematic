using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A connection factory decorator whose connection opens fail while <see cref="IsFailing"/> is set, so tests can
/// make a database unreachable and then bring it back. The failure is not one that any retry policy handles, so
/// a query attempted while failing fails at once instead of backing off.
/// </summary>
public sealed class SwitchableFailureConnectionFactory : IDbConnectionFactory
{
    public SwitchableFailureConnectionFactory(IDbConnectionFactory innerFactory)
    {
        InnerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
    }

    private IDbConnectionFactory InnerFactory { get; }

    /// <summary>
    /// Whether opening a connection currently fails.
    /// </summary>
    public bool IsFailing
    {
        get => Volatile.Read(ref _isFailing);
        set => Volatile.Write(ref _isFailing, value);
    }

    public DbConnection CreateConnection() => InnerFactory.CreateConnection();

    public DbConnection OpenConnection()
    {
        ThrowWhenFailing();
        return InnerFactory.OpenConnection();
    }

    public Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowWhenFailing();
        return InnerFactory.OpenConnectionAsync(cancellationToken);
    }

    public bool DisposeConnection => InnerFactory.DisposeConnection;

    public PolicyBuilder RetryPolicy => InnerFactory.RetryPolicy;

    public int MaxConcurrentQueries => InnerFactory.MaxConcurrentQueries;

    private void ThrowWhenFailing()
    {
        if (IsFailing)
            throw new SimulatedConnectionFailureException();
    }

    private bool _isFailing;
}

/// <summary>
/// The error raised by <see cref="SwitchableFailureConnectionFactory"/> when a connection cannot be opened.
/// </summary>
public sealed class SimulatedConnectionFailureException : Exception
{
    public SimulatedConnectionFailureException()
        : base("The connection could not be opened.")
    {
    }

    public SimulatedConnectionFailureException(string message)
        : base(message)
    {
    }

    public SimulatedConnectionFailureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
