FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY ["MiniERP/MiniERP.csproj", "MiniERP/"]
COPY ["MiniERP.Application/MiniERP.Application.csproj", "MiniERP.Application/"]
COPY ["MiniERP.Domain/MiniERP.Domain.csproj", "MiniERP.Domain/"]
COPY ["MiniERP.Infrastructure/MiniERP.Infrastructure.csproj", "MiniERP.Infrastructure/"]


RUN dotnet restore "MiniERP/MiniERP.csproj"
RUN dotnet restore "MiniERP.Application/MiniERP.Application.csproj"
RUN dotnet restore "MiniERP.Domain/MiniERP.Domain.csproj"
RUN dotnet restore "MiniERP.Infrastructure/MiniERP.Infrastructure.csproj"

COPY . .

RUN dotnet publish "MiniERP/MiniERP.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish . 
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniERP.dll"]

