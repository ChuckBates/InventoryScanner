# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["InventoryScannerCore/InventoryScannerCore.csproj", "InventoryScannerCore/"]
COPY ["InventoryScanner.Messaging/InventoryScanner.Messaging.csproj", "InventoryScanner.Messaging/"]
RUN dotnet restore "InventoryScannerCore/InventoryScannerCore.csproj"

COPY . .
WORKDIR "/src/InventoryScannerCore"
RUN dotnet build "./InventoryScannerCore.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InventoryScannerCore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN apt-get update && apt-get install -y wget ca-certificates && \
    wget https://github.com/jwilder/dockerize/releases/download/v0.6.1/dockerize-linux-amd64-v0.6.1.tar.gz && \
    tar -C /usr/local/bin -xzvf dockerize-linux-amd64-v0.6.1.tar.gz && \
    rm dockerize-linux-amd64-v0.6.1.tar.gz && \
    apt-get remove -y wget && \
    rm -rf /var/lib/apt/lists/*
USER app

# Wait for RabbitMQ and Postgres to be ready before starting the app
ENTRYPOINT ["dockerize", "-wait", "tcp://rabbitmq:5672", "-wait", "tcp://postgres:5432", "-timeout", "30s", "dotnet", "InventoryScannerCore.dll"]

