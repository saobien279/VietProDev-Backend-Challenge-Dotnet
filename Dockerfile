# Stage 1: Build env
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to cache dependencies
COPY ["MiniERP.sln", "./"]
COPY ["MiniERP/MiniERP.csproj", "MiniERP/"]
COPY ["MiniERP.Application/MiniERP.Application.csproj", "MiniERP.Application/"]
COPY ["MiniERP.Domain/MiniERP.Domain.csproj", "MiniERP.Domain/"]
COPY ["MiniERP.Infrastructure/MiniERP.Infrastructure.csproj", "MiniERP.Infrastructure/"]

# Restore packages
RUN dotnet restore "MiniERP/MiniERP.csproj"

# Copy all source files and build the release output
COPY . .
WORKDIR "/src/MiniERP"
RUN dotnet publish "MiniERP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime env
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniERP.dll"]
