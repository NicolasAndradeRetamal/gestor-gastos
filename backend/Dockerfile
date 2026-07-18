# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/GestorGastos.Domain/*.csproj src/GestorGastos.Domain/
COPY src/GestorGastos.Infrastructure/*.csproj src/GestorGastos.Infrastructure/
COPY src/GestorGastos.Api/*.csproj src/GestorGastos.Api/
RUN dotnet restore src/GestorGastos.Api/GestorGastos.Api.csproj

COPY src/ src/
RUN dotnet publish src/GestorGastos.Api/GestorGastos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl backs the container HEALTHCHECK against /health.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GestorGastos.Api.dll"]
