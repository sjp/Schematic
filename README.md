# Schematic

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![GitHub Actions](https://github.com/sjp/Schematic/actions/workflows/ci.yml/badge.svg)](https://github.com/sjp/Schematic/actions/workflows/ci.yml) [![Code coverage](https://img.shields.io/codecov/c/gh/sjp/Schematic/master?logo=codecov)](https://codecov.io/gh/sjp/Schematic)

![Project icon](database.png)

This is a work in progress.

The aim of this project is to create an easier way to declare and manage schemas of medium-level complexity. An additional constraint is that schemas should be largely transferable between different SQL implementations.

## Features

* Easy querying of database schema in a vendor-independent manner. Vendor-specific information may also be available (e.g. `PRAGMA` for SQLite).
* Given a declared schema, generate code/classes that can be used in Entity Framework, Dapper, ServiceStack.OrmLite, etc.
* Detects potential schema issues. For example, missing indexes or foreign key constraints.
* Generates a schema report that includes details on tables, views, columns, indexes, etc. Also includes relationship diagrams.

## Currently supported databases

* MySQL
* Oracle
* PostgreSQL
* SQLite
* SQL Server

## Command-line tool

Schematic ships a command-line tool, packaged as the [`dotnet-schematic`](https://www.nuget.org/packages/dotnet-schematic) .NET tool:

```sh
dotnet tool install --global dotnet-schematic
```

The `schematic` command exposes the following sub-commands:

* `init` — interactively create a configuration file (see below).
* `orm efcore` / `orm ormlite` / `orm poco` — generate ORM projects for a database.
* `lint` — analyse a database schema for potential issues.
* `report` — generate an HTML report of a database schema.
* `test` — test a database connection to see whether it is available.
* `completion` — generate shell completion scripts (see below).

### Configuring a connection

Every database command needs to know two things: which **dialect** to use (one of `mysql`, `oracle`,
`postgresql`, `sqlserver`, `sqlite`) and a **connection string**. There are three ways to supply them.

**1. Inline, no file needed.** Pass `--dialect` and `--connection-string` directly. This is the quickest
way to run a one-off report or lint:

```sh
schematic report --dialect sqlite --connection-string "Data Source=app.db" --output ./report
schematic lint   --dialect sqlite --connection-string "Data Source=app.db"
```

**2. Scaffold a configuration file with `init`.** For a repeatable setup, run the interactive wizard.
It prompts for the dialect and connection details (entering them field-by-field, or pasting a full
connection string), optionally tests the connection, and writes a `schematic.json`:

```sh
schematic init                       # writes ./schematic.json
schematic init --output db.json      # or a path of your choosing
```

**3. Use a configuration file.** A configuration file is JSON with a `Dialect` and a connection string
named `Schematic`:

```json
{
  "Dialect": "sqlserver",
  "ConnectionStrings": {
    "Schematic": "Server=localhost,1433;Database=MyDb;User Id=sa;Password=<password>"
  }
}
```

Point any command at it with `--config` (or `-c`). If you omit `--config`, a `schematic.json` in the
current directory is picked up automatically:

```sh
schematic report --config schematic.json --output ./report
schematic lint                       # uses ./schematic.json if present
```

Values can also be overridden via environment variables (for example, keeping secrets out of the file):
set `Dialect` and `ConnectionStrings__Schematic`. Inline `--dialect` / `--connection-string` options take
precedence over both the file and environment variables.

Values inside the configuration file (and inline `--dialect` / `--connection-string` options) can also
reference environment variables with `${NAME}` placeholders, so the file itself can be committed to
source control without leaking secrets:

```json
{
  "Dialect": "sqlserver",
  "ConnectionStrings": {
    "Schematic": "Server=localhost,1433;Database=MyDb;User Id=sa;Password=${DB_PASSWORD}"
  }
}
```

`${NAME}` is the only supported syntax — there's no nesting or default-value fallback. If a referenced
variable isn't set, the command fails immediately with a clear error rather than silently using a blank
value. `schematic init` can offer to write a placeholder like this for you instead of the literal
password when you choose the guided setup.

### Shell completions

The `completion` command prints a tab-completion script for a given shell to standard output. Supported shells are `bash`, `zsh`, `fish`, and `powershell`.

```sh
# bash — current shell
source <(schematic completion bash)
# bash — permanent
schematic completion bash > /etc/bash_completion.d/schematic

# zsh — save to a directory on your $fpath
schematic completion zsh > "${fpath[1]}/_schematic"

# fish
schematic completion fish > ~/.config/fish/completions/schematic.fish

# PowerShell — append to your profile
schematic completion powershell >> $PROFILE
```

Each generated script starts with a comment describing how to install it.

## Status

At the moment, all supported vendors are able to provide a significant amount of information about the structure of the schema. In other words, obtaining schema information is largely complete.

There is an aspirational goal of generating scripts for migration purposes. This is still in a work-in-progress phase.

## Icon

Original icon made by [Madebyoliver](https://dribbble.com/olivers) from [www.flaticon.com](https://www.flaticon.com). Modifications created by myself.
