# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and shared library
COPY ["Services/UserService/UserService.API/UserService.API.csproj", "Services/UserService/UserService.API/"]
COPY ["Services/UserService/UserService.Application/UserService.Application.csproj", "Services/UserService/UserService.Application/"]
COPY ["Services/UserService/UserService.Domain/UserService.Domain.csproj", "Services/UserService/UserService.Domain/"]
COPY ["Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "Services/UserService/UserService.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]

# Restore dependencies
RUN dotnet restore "Services/UserService/UserService.API/UserService.API.csproj"

# Copy source code
COPY ["Services/UserService/UserService.API/", "Services/UserService/UserService.API/"]
COPY ["Services/UserService/UserService.Application/", "Services/UserService/UserService.Application/"]
COPY ["Services/UserService/UserService.Domain/", "Services/UserService/UserService.Domain/"]
COPY ["Services/UserService/UserService.Infrastructure/", "Services/UserService/UserService.Infrastructure/"]
COPY ["Shared/Shared.Kernel/", "Shared/Shared.Kernel/"]

# Build
WORKDIR "/src/Services/UserService/UserService.API"
RUN dotnet build "UserService.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5100

# Copy published app
COPY --from=publish /app/publish .

# Run as non-root user
RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "UserService.API.dll"]
