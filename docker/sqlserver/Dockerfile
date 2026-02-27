FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

COPY init.sh /init.sh
COPY init.sql /init.sql

RUN chmod +x /init.sh

USER mssql

ENTRYPOINT ["/init.sh"]
