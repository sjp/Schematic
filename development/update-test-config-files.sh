#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cp "${DIR}/mysql-test.config.json" "${DIR}/../src/SJP.Schematic.MySql.Tests/mysql-test.config.json"
cp "${DIR}/oracle-test.config.json" "${DIR}/../src/SJP.Schematic.Oracle.Tests/oracle-test.config.json"
cp "${DIR}/postgresql-test.config.json" "${DIR}/../src/SJP.Schematic.PostgreSql.Tests/postgresql-test.config.json"
cp "${DIR}/sqlserver-test.config.json" "${DIR}/../src/SJP.Schematic.SqlServer.Tests/sqlserver-test.config.json"