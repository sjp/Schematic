# SJP.Schema

![Project icon](https://cdn.rawgit.com/sjp/SJP.Schema/master/database.svg)

This is a work in progress.

The aim of this project is to create an easier way to declare and manage schemas of medium-level complexity.

Goals:

* Simple and strongly-typed declarations of schema in C\#.
* Automatic schema updates to match schema defined in C\#.
* Customisable schema i.e. have schema declared in C\# but overridden (at least partially) in JSON, XML, etc.
* Given a declared schema, generate code/classes that can be used in Entity Framework, Dapper, ServiceStack.OrmLite, etc.
* Vendor agnostic migrations, ensuring that we can easily switch between vendors without changing declarations. Note, this does not mean ignoring vendor-specific behaviour, only that such behaviour may not be migratable.
* Manual/explicit database migrations (e.g. create table, object renames, etc). Can occur before or after automatic migrations.

Non-goals:

* User administration/management. Access and permissions are not in scope.
* Advanced database features such as partitioning, file storage, advanced index configuration.

Original icon made by [Madebyoliver](https://dribbble.com/olivers) from [www.flaticon.com](http://www.flaticon.com). Modifications created by myself.
