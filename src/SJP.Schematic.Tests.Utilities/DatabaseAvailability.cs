using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// Availability probing for the integration fixtures that require a live database.
/// </summary>
public static class DatabaseAvailability
{
    /// <summary>
    /// Ignores the enclosing suite when a connection to the database cannot be opened.
    /// </summary>
    /// <param name="connectionFactoryProvider">Retrieves the connection factory to probe. Returning <see langword="null"/>, or throwing, is treated the same way as a failed connection.</param>
    /// <param name="ignoreMessage">The message to report against the skipped tests.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactoryProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="ignoreMessage"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <remarks>
    /// Intended to be called from the <see cref="OneTimeSetUpAttribute"/> method of a namespace
    /// <see cref="SetUpFixtureAttribute"/>: NUnit propagates the ignore to every test beneath that
    /// namespace, and the single connection is opened once per run at execution time rather than
    /// once per fixture at discovery time.
    /// </remarks>
    public static void EnsureAvailable(Func<IDbConnectionFactory?> connectionFactoryProvider, string ignoreMessage)
    {
        ArgumentNullException.ThrowIfNull(connectionFactoryProvider);
        ArgumentException.ThrowIfNullOrWhiteSpace(ignoreMessage);

        IDbConnectionFactory? connectionFactory = null;
        string? failureMessage = null;

        try
        {
            connectionFactory = connectionFactoryProvider();
            if (connectionFactory != null)
            {
                using var connection = connectionFactory.OpenConnection();
            }
        }
        catch (Exception ex)
        {
            failureMessage = ex.Message;
        }

        // Assert.Ignore() throws, so it must stay outside of the try block above.
        if (failureMessage != null)
            Assert.Ignore(ignoreMessage + ": " + failureMessage);
        else if (connectionFactory == null)
            Assert.Ignore(ignoreMessage);
    }
}
