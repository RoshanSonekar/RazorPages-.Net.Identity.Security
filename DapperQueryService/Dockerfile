# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /BuildingBlocks 
COPY ["/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]
RUN dotnet build "BuildingBlocks.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

WORKDIR /DapperSqlService 
COPY ["DapperSqlService.csproj", "DapperQueryService/"]
RUN dotnet restore "DapperQueryService/DapperSqlService.csproj"

COPY . .
#COPY BuildingBlocks/ ./BuildingBlocks/
#WORKDIR . .
RUN dotnet build "DapperSqlService.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DapperSqlService.csproj" -c ${BUILD_CONFIGURATION} --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DapperSqlService.dll"]
