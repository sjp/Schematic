#nullable enable
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class TestCommandTests
{
    private static async Task<(int exitCode, string output)> RunTestCommandAsync(IDbConnectionFactory connectionFactory)
    {
        var (console, writer) = CommandAppHarness.CreateCapturingConsole();

        var provider = new Mock<IDatabaseCommandDependencyProvider>(MockBehavior.Strict);
        provider.Setup(p => p.GetConnectionFactory()).Returns(connectionFactory);

        var factory = new Mock<IDatabaseCommandDependencyProviderFactory>(MockBehavior.Strict);
        factory.Setup(f => f.GetDbDependencies(It.IsAny<CommonSettings>())).Returns(provider.Object);

        var registrar = new CommandAppHarness.InstanceRegistrar();
        registrar.RegisterInstance(typeof(IAnsiConsole), console);
        registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), factory.Object);

        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<TestCommand>("test"));

        var exitCode = await app.RunAsync(["test", "--dialect", "sqlite", "--connection-string", "Data Source=:memory:"]);
        return (exitCode, writer.ToString());
    }

    [Test]
    public static async Task ExecuteAsync_GivenOpenableConnection_ReportsSuccess()
    {
        var connectionFactory = new Mock<IDbConnectionFactory>(MockBehavior.Loose);
        connectionFactory
            .Setup(f => f.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<DbConnection>());

        var (exitCode, output) = await RunTestCommandAsync(connectionFactory.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exitCode, Is.Zero);
            Assert.That(output, Does.Contain("Successfully connected"));
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenConnectionThatThrows_ReportsFailure()
    {
        var connectionFactory = new Mock<IDbConnectionFactory>(MockBehavior.Loose);
        connectionFactory
            .Setup(f => f.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("no database here"));

        var (exitCode, output) = await RunTestCommandAsync(connectionFactory.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exitCode, Is.Not.Zero);
            Assert.That(output, Does.Contain("Failed to connect"));
            Assert.That(output, Does.Contain("no database here"));
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenConnectionThatTimesOut_ReportsTimeout()
    {
        var connectionFactory = new Mock<IDbConnectionFactory>(MockBehavior.Loose);
        connectionFactory
            .Setup(f => f.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var (exitCode, output) = await RunTestCommandAsync(connectionFactory.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exitCode, Is.Not.Zero);
            Assert.That(output, Does.Contain("timed out"));
        }
    }
}
