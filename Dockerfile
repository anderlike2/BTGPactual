# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["BTGPactual.API/BTGPactual.API.csproj", "BTGPactual.API/"]
COPY ["BTGPactual.Application/BTGPactual.Application.csproj", "BTGPactual.Application/"]
COPY ["BTGPactual.Domain/BTGPactual.Domain.csproj", "BTGPactual.Domain/"]
COPY ["BTGPactual.Infrastructure/BTGPactual.Infrastructure.csproj", "BTGPactual.Infrastructure/"]
COPY ["BTGPactual.Shared/BTGPactual.Shared.csproj", "BTGPactual.Shared/"]

# Restore dependencies
RUN dotnet restore "BTGPactual.API/BTGPactual.API.csproj"

# Copiar todo el código
COPY . .

# Build
WORKDIR "/src/BTGPactual.API"
RUN dotnet build "BTGPactual.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "BTGPactual.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar certificado TLS para DocumentDB
COPY global-bundle.pem /app/global-bundle.pem

# Copiar aplicación publicada
COPY --from=publish /app/publish .

# Exponer puerto
EXPOSE 8080

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "BTGPactual.API.dll"]