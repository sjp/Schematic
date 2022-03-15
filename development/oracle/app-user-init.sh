#!/bin/bash

# Exit on errors
# Great explanation on https://vaneyckt.io/posts/safer_bash_scripts_with_set_euxo_pipefail/
set -Eeuo pipefail

# Grant permissions for the app user
function grant_app_user {
    # Check whether the user needs to be in a PDB or not
    ALTER_SESSION_CMD="ALTER SESSION SET CONTAINER=XEPDB1;"
    if [[ "${ORACLE_VERSION}" = "11.2"* ]]; then
        ALTER_SESSION_CMD="";
    fi;

    echo "CONTAINER: Adding permissions to application user."

    sqlplus -s / as sysdba <<EOF
     -- Exit on any errors
     WHENEVER SQLERROR EXIT SQL.SQLCODE
     ${ALTER_SESSION_CMD}
     GRANT CREATE SYNONYM TO ${APP_USER};
     exit;
EOF

    # If ORACLE_DATABASE is specified, create user also in app PDB (only applicable >=18c)
    if [ -n "${ORACLE_DATABASE:-}" ]; then
        sqlplus -s / as sysdba <<EOF
       -- Exit on any errors
       WHENEVER SQLERROR EXIT SQL.SQLCODE
       ALTER SESSION SET CONTAINER=${ORACLE_DATABASE};
       GRANT CREATE SYNONYM TO ${APP_USER};
       exit;
EOF
    fi;
}

# Check whether app user should have permissions granted
if [ -n "${APP_USER:-}" ]; then
    grant_app_user
fi;