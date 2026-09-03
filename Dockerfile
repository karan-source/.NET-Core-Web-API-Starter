FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy manifests first so the restore layer is cached until dependencies change.
COPY Directory.Build.props ProductionApi.slnx ./
COPY src/ProductionApi.Domain/*.csproj src/ProductionApi.Domain/
COPY src/ProductionApi.Application/*.csproj src/ProductionApi.Application/
COPY src/ProductionApi.Infrastructure/*.csproj src/ProductionApi.Infrastructure/
COPY src/ProductionApi.Api/*.csproj src/ProductionApi.Api/
COPY tests/ProductionApi.Application.UnitTests/*.csproj tests/ProductionApi.Application.UnitTests/
COPY tests/ProductionApi.Api.IntegrationTests/*.csproj tests/ProductionApi.Api.IntegrationTests/
RUN dotnet restore ProductionApi.slnx

COPY . .
RUN dotnet publish src/ProductionApi.Api/ProductionApi.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# APP_UID is defined by the base image; running as non-root is the default here.
USER $APP_UID
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "ProductionApi.Api.dll"]
