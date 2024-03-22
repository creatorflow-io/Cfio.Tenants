#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG JuiceRefVersion=7.0.3-alpha.*
ARG GITHUB_PACKAGE_USERNAME
ARG GITHUB_PACKAGE_TOKEN
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Cfio.Tenants.InternalHost/Cfio.Tenants.InternalHost.csproj", "src/Cfio.Tenants.InternalHost/"]

RUN dotnet restore "./src/Cfio.Tenants.InternalHost/Cfio.Tenants.InternalHost.csproj"
COPY . .
WORKDIR "/src/src/Cfio.Tenants.InternalHost"
RUN dotnet build "./Cfio.Tenants.InternalHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cfio.Tenants.InternalHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_URLS=http://+:80;https://+:443
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cfio.Tenants.InternalHost.dll"]