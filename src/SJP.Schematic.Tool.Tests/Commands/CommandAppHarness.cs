#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

// Shared helpers for exercising commands in-process through a CommandApp.
internal static class CommandAppHarness
{
    // Creates a throwaway SQLite database whose 'book' table has a foreign key without a
    // supporting index, which triggers a well-known lint rule and gives the report/ORM
    // generators a small but non-trivial schema to work with.
    public static string CreateSampleSqliteDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), $"schematic-test-{Guid.NewGuid():N}.db");

        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE author (author_id INTEGER PRIMARY KEY, name TEXT NOT NULL);
            CREATE TABLE book (
                book_id INTEGER PRIMARY KEY,
                title TEXT NOT NULL,
                author_id INTEGER REFERENCES author(author_id)
            );
            """;
        command.ExecuteNonQuery();

        return path;
    }

    // Releases pooled SQLite handles and deletes the database file created for a test.
    public static void DeleteSqliteDatabase(string path)
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(path))
            File.Delete(path);
    }

    // A minimal type registrar that constructs commands from a set of pre-registered
    // instances, avoiding a dependency on a full DI container for these tests.
    public sealed class InstanceRegistrar : ITypeRegistrar, ITypeResolver
    {
        private readonly Dictionary<Type, object> _instances = [];

        public void Register(Type service, Type implementation)
        {
        }

        // First registration wins: this keeps the console (and other test doubles) we register up
        // front from being overwritten when Spectre seeds the registrar with its own defaults.
        public void RegisterInstance(Type service, object implementation) => _instances.TryAdd(service, implementation);

        public void RegisterLazy(Type service, Func<object> factory) => _instances.TryAdd(service, factory());

        public ITypeResolver Build() => this;

        public object? Resolve(Type? type)
        {
            if (type == null)
                return null;
            if (_instances.TryGetValue(type, out var instance))
                return instance;

            // Spectre resolves collections (e.g. IEnumerable<IHelpProvider>) and expects an empty
            // sequence rather than null when nothing is registered.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return Array.CreateInstance(type.GetGenericArguments()[0], 0);

            if (type.IsInterface || type.IsAbstract)
                return null;

            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
                return Activator.CreateInstance(type);

            var args = constructor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();
            return Activator.CreateInstance(type, args);
        }
    }

    public static (IAnsiConsole console, StringWriter writer) CreateCapturingConsole()
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No,
            Out = new AnsiConsoleOutput(writer),
        });
        return (console, writer);
    }
}
