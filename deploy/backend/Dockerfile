FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/src/PackControl.Api/PackControl.Api.csproj backend/src/PackControl.Api/
COPY backend/src/PackControl.Infrastructure/PackControl.Infrastructure.csproj backend/src/PackControl.Infrastructure/
COPY backend/src/PackControl.Application/PackControl.Application.csproj backend/src/PackControl.Application/
COPY backend/src/PackControl.Domain/PackControl.Domain.csproj backend/src/PackControl.Domain/
COPY shared/PackControl.Contracts/PackControl.Contracts.csproj shared/PackControl.Contracts/

RUN dotnet restore backend/src/PackControl.Api/PackControl.Api.csproj

COPY backend/src backend/src
COPY shared shared

RUN dotnet publish backend/src/PackControl.Api/PackControl.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PackControl.Api.dll"]
