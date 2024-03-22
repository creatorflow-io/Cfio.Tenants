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
ARG RunInDocker=true
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Cfio.Tenants.App/Cfio.Tenants.App.csproj", "src/Cfio.Tenants.App/"]

RUN dotnet restore "./src/Cfio.Tenants.App/Cfio.Tenants.App.csproj" --no-cache --force
COPY . .
WORKDIR "/src/src/Cfio.Tenants.App"
RUN dotnet build "./Cfio.Tenants.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Cfio.Tenants.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_URLS=http://+:80;https://+:443
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cfio.Tenants.App.dll"]
