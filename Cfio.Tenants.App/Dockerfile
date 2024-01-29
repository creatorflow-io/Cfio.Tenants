#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN set -uex \
    && apt-get update \
    && apt-get install -y ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key \
     | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && NODE_MAJOR=18 \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" \
     | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update \
    && apt-get install nodejs -y;

ARG JuiceRefVersion=7.0.2
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Cfio.Tenants.App/Cfio.Tenants.App.csproj", "Cfio.Tenants.App/"]

RUN dotnet restore "Cfio.Tenants.App/Cfio.Tenants.App.csproj" --ignore-failed-sources --no-cache --force --configfile nuget.config
COPY . .
WORKDIR "/src/Cfio.Tenants.App"
RUN dotnet build "Cfio.Tenants.App.csproj" -c Release -o /app/build /p:SpaProxyServerUrl={Scheme}://{ServiceHost}:{ServicePort};SpaRoot=/ClientApp/

FROM build AS publish
RUN dotnet publish "Cfio.Tenants.App.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Cfio.Tenants.App.dll"]
