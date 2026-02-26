# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ProductService.API/ProductService.API.csproj", "ProductService.API/"]
COPY ["ProductService.Application/ProductService.Application.csproj", "ProductService.Application/"]
COPY ["ProductService.Domain/ProductService.Domain.csproj", "ProductService.Domain/"]
COPY ["ProductService.Infrastructure/ProductService.Infrastructure.csproj", "ProductService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ProductService.API/ProductService.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/ProductService.API"
RUN dotnet build "ProductService.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ProductService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy published app
COPY --from=publish /app/publish .

# Run as non-root user
RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "ProductService.API.dll"]

