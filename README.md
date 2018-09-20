# Schematic

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![Build status](https://ci.appveyor.com/api/projects/status/4tojgp8j8hp8vyd3?svg=true)](https://ci.appveyor.com/project/sjp/sjp-schema) [![Code coverage](https://codecov.io/gh/sjp/Schematic/branch/master/graph/badge.svg)](https://codecov.io/gh/sjp/Schematic)

![Project icon](database.png)

This is a work in progress.

The aim of this project is to create an easier way to declare and manage schemas of medium-level complexity. An additional constraint is that schemas should be largely transferable between different SQL implementations.

## Goals

* Easy querying of database schema in a vendor-independent manner. Vendor-specific information should also be available (e.g. `PRAGMA` for SQLite).
* Simple and strongly-typed declarations of schema in C\#.
* Automatic schema updates to match schema defined in C\#.
* Customisable schema i.e. have schema declared in C\# but overridden (at least partially) in JSON, XML, etc.
* Given a declared schema, generate code/classes that can be used in Entity Framework, Dapper, ServiceStack.OrmLite, etc.
* Vendor agnostic migrations, ensuring that we can easily switch between vendors without changing declarations. Note, this does not mean ignoring vendor-specific behaviour, only that such behaviour may not be migratable.
* Manual/explicit database migrations (e.g. create table, object renames, etc). Can occur before or after automatic migrations.

## Non-goals (at the moment)

* User administration/management. Access and permissions are not in scope.
* Advanced database features such as partitioning, file storage, advanced index configuration.

## Currently supported databases

* MySQL
* Oracle
* PostgreSQL
* SQLite
* SQL Server

## Status

At the moment, all supported vendors are able to provide a significant amount of information about the structure of the schema. In other words, obtaining schema information is largely complete.

We are also able to generate code to access a database using commonly available ORMs. So far Dapper-like mappers, ServiceStack.OrmLite and Entity Framework Core are implemented. For the latter two, the generated code also includes the ability for schema to be managed entirely in ServiceStack.OrmLite or Entity Framework Core. This means that `Schematic` can be used to bootstrap an ORM project from an existing database.

## Icon

Original icon made by [Madebyoliver](https://dribbble.com/olivers) from [www.flaticon.com](http://www.flaticon.com). Modifications created by myself.