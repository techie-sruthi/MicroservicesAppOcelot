# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY ["Services/ApiGateway/ApiGateway/ApiGateway.csproj", "Services/ApiGateway/ApiGateway/"]
COPY ["Shared/Shared.Kernel/Shared.Kernel.csproj", "Shared/Shared.Kernel/"]

# Restore dependencies
RUN dotnet restore "Services/ApiGateway/ApiGateway/ApiGateway.csproj"

# Copy only the application source
COPY . .

# Build
WORKDIR "/src/Services/ApiGateway/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy published output
COPY --from=publish /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
