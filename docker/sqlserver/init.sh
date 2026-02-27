#!/bin/bash
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &
PID=$!

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
until /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -No -Q "SELECT 1" &>/dev/null; do
  sleep 1
done

echo "SQL Server ready. Running init script..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -No -i /init.sql
echo "Init script complete."

# Hand off to SQL Server process
wait $PID
