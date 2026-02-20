# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["UserService.API/UserService.API.csproj", "UserService.API/"]
COPY ["UserService.Application/UserService.Application.csproj", "UserService.Application/"]
COPY ["UserService.Domain/UserService.Domain.csproj", "UserService.Domain/"]
COPY ["UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "UserService.API/UserService.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/UserService.API"
RUN dotnet build "UserService.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5000

# Copy published app
COPY --from=publish /app/publish .

# Run as non-root user
RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "UserService.API.dll"]
