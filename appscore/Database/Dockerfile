FROM mcr.microsoft.com/mssql/server:2017-latest
ENV SA_PASSWORD=Pass@word
ENV ACCEPT_EULA=Y
COPY entrypoint.sh entrypoint.sh
COPY SqlCmdStartup.sh SqlCmdStartup.sh
COPY SqlCmdScript.sql SqlCmdScript.sql
COPY init.sql init.sql
RUN chmod +x ./SqlCmdStartup.sh

# Correction pour exécuter SQL Server en tant qu'utilisateur non-root
RUN groupadd -r mssqlgroup && useradd -r -g mssqlgroup mssqluser && mkdir /home/mssqluser && chown -R mssqluser:mssqlgroup /var/opt/mssql
USER mssqluser

CMD /bin/bash ./entrypoint.sh