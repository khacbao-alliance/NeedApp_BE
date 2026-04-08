# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files for layer caching
COPY src/NeedApp.Domain/NeedApp.Domain.csproj src/NeedApp.Domain/
COPY src/NeedApp.Application/NeedApp.Application.csproj src/NeedApp.Application/
COPY src/NeedApp.Infrastructure/NeedApp.Infrastructure.csproj src/NeedApp.Infrastructure/
COPY src/NeedApp.API/NeedApp.API.csproj src/NeedApp.API/

# Restore dependencies
RUN dotnet restore src/NeedApp.API/NeedApp.API.csproj

# Copy all source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/NeedApp.API/NeedApp.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "NeedApp.API.dll"]
