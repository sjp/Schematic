# Schematic

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![Build Status](https://travis-ci.com/sjp/Schematic.svg?branch=master)](https://travis-ci.com/sjp/Schematic) [![Windows Build Status](https://ci.appveyor.com/api/projects/status/4tojgp8j8hp8vyd3?svg=true)](https://ci.appveyor.com/project/sjp/sjp-schema) [![Code coverage](https://codecov.io/gh/sjp/Schematic/branch/master/graph/badge.svg)](https://codecov.io/gh/sjp/Schematic)

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

## Status

At the moment, all supported vendors are able to provide a significant amount of information about the structure of the schema. In other words, obtaining schema information is largely complete.

There is an aspirational goal of generating scripts for migration purposes. This is still in a work-in-progress phase.

## Icon

Original icon made by [Madebyoliver](https://dribbble.com/olivers) from [www.flaticon.com](https://www.flaticon.com). Modifications created by myself.
