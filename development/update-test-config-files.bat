echo off
copy /-Y mysql-test.config.json ..\src\SJP.Schematic.MySql.Tests\mysql-test.config.json
copy /-Y postgresql-test.config.json ..\src\SJP.Schematic.PostgreSql.Tests\postgresql-test.config.json
copy /-Y sqlserver-test.config.json ..\src\SJP.Schematic.SqlServer.Tests\sqlserver-test.config.json