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

* `orm efcore` / `orm ormlite` / `orm poco` — generate ORM projects for a database.
* `lint` — analyse a database schema for potential issues.
* `report` — generate an HTML report of a database schema.
* `test` — test a database connection to see whether it is available.
* `completion` — generate shell completion scripts (see below).

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
