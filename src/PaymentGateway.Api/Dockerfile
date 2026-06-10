FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["src/PaymentGateway.Api/PaymentGateway.Api.csproj", "PaymentGateway.Api/"]
RUN dotnet restore "PaymentGateway.Api/PaymentGateway.Api.csproj"

COPY ["src/", "."]

WORKDIR /src/PaymentGateway.Api
RUN dotnet publish "PaymentGateway.Api.csproj" --no-restore -c Release \
    /p:UseAppHost=false -o /app/publish \
    /p:DebugType=None

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /home/app

COPY --from=build /app/publish .

USER app

HEALTHCHECK --interval=10s --timeout=3s \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"]