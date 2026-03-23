# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and shared library
COPY ["Services/ProductService/ProductService.API/ProductService.API.csproj", "Services/ProductService/ProductService.API/"]
COPY ["Services/ProductService/ProductService.Application/ProductService.Application.csproj", "Services/ProductService/ProductService.Application/"]
COPY ["Services/ProductService/ProductService.Domain/ProductService.Domain.csproj", "Services/ProductService/ProductService.Domain/"]
COPY ["Services/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "Services/ProductService/ProductService.Infrastructure/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]

# Restore dependencies
RUN dotnet restore "Services/ProductService/ProductService.API/ProductService.API.csproj"

# Copy source code
COPY ["Services/ProductService/ProductService.API/", "Services/ProductService/ProductService.API/"]
COPY ["Services/ProductService/ProductService.Application/", "Services/ProductService/ProductService.Application/"]
COPY ["Services/ProductService/ProductService.Domain/", "Services/ProductService/ProductService.Domain/"]
COPY ["Services/ProductService/ProductService.Infrastructure/", "Services/ProductService/ProductService.Infrastructure/"]
COPY ["Shared/Shared.Kernel/", "Shared/Shared.Kernel/"]

# Build
WORKDIR "/src/Services/ProductService/ProductService.API"
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

