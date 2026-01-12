FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Install curl for health checks
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ARG APP_UID=1654
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoreLedger.Worker/CoreLedger.Worker.csproj", "CoreLedger.Worker/"]
COPY ["CoreLedger.Application/CoreLedger.Application.csproj", "CoreLedger.Application/"]
COPY ["CoreLedger.Domain/CoreLedger.Domain.csproj", "CoreLedger.Domain/"]
COPY ["CoreLedger.Infrastructure/CoreLedger.Infrastructure.csproj", "CoreLedger.Infrastructure/"]
RUN dotnet restore "CoreLedger.Worker/CoreLedger.Worker.csproj"
COPY . .
WORKDIR "/src/CoreLedger.Worker"
RUN dotnet build "./CoreLedger.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreLedger.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreLedger.Worker.dll"]
