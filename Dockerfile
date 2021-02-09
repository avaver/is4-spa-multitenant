FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY ./DS.Identity.sln .
COPY ./src/DS.Identity/DS.Identity.csproj ./src/DS.Identity/DS.Identity.csproj
COPY ./src/DS.Identity.AppIdentity/DS.Identity.AppIdentity.csproj ./src/DS.Identity.AppIdentity/DS.Identity.AppIdentity.csproj
COPY ./src/DS.Identity.Migrations.Sqlite/DS.Identity.Migrations.Sqlite.csproj ./src/DS.Identity.Migrations.Sqlite/DS.Identity.Migrations.Sqlite.csproj
COPY ./src/DS.Identity.Migrations.SqlServer/DS.Identity.Migrations.SqlServer.csproj ./src/DS.Identity.Migrations.SqlServer/DS.Identity.Migrations.SqlServer.csproj

RUN dotnet restore

COPY ./src/DS.Identity/. ./src/DS.Identity/
COPY ./src/DS.Identity.AppIdentity/. ./src/DS.Identity.AppIdentity/
COPY ./src/DS.Identity.Migrations.Sqlite/. ./src/DS.Identity.Migrations.Sqlite/
COPY ./src/DS.Identity.Migrations.SqlServer/. ./src/DS.Identity.Migrations.SqlServer/

WORKDIR /app/src/DS.Identity
RUN dotnet publish -c Debug -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime
WORKDIR /app
COPY --from=build /app/src/DS.Identity/out ./
ENTRYPOINT ["dotnet", "DS.Identity.dll"]