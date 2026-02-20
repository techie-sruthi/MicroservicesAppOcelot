# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ApiGateway.csproj", "."]
RUN dotnet restore "ApiGateway.csproj"

COPY . .
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5200

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
