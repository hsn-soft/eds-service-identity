FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Metadata labels
LABEL org.opencontainers.image.title="HsnSoft Eds Identity Service" \
      org.opencontainers.image.description="HsnSoft Eds Identity Service" \
      org.opencontainers.image.authors="HsnSoft <admin@hsnsoft.com>" \
      org.opencontainers.image.source="https://github.com/hsn-soft/eds-service-identity"

USER root

WORKDIR /app
EXPOSE 8080

ENV DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_URLS='http://+:8080;' \
    ASPNETCORE_ENVIRONMENT='docker'

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Health check endpoint (varsayılan: /health)
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/LivenessCheck || exit 1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-stage
WORKDIR /build-source

# Non-root user setup
RUN groupadd -r -g 1001 servicegroup && \
    useradd -r -u 1001 serviceuser -g servicegroup

# Configuration files
COPY ["./nuget.config", "./"]
COPY ["./common.props", "./"]
COPY ["./common.version.props", "./"]

COPY ["./src/Eds.IdentityService.Application/Eds.IdentityService.Application.csproj", "./src/Eds.IdentityService.Application/"]
COPY ["./src/Eds.IdentityService.Domain/Eds.IdentityService.Domain.csproj", "./src/Eds.IdentityService.Domain/"]
COPY ["./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj", "./src/Eds.IdentityService.EntityFrameworkCore/"]
COPY ["./src/Eds.IdentityService.Http.Host/Eds.IdentityService.Http.Host.csproj", "./src/Eds.IdentityService.Http.Host/"]

ENV NUGET_CERT_REVOCATION_MODE=offline
RUN dotnet restore "./src/Eds.IdentityService.Http.Host/Eds.IdentityService.Http.Host.csproj" \
    --configfile ./nuget.config \
    --verbosity minimal

# Copy source code
COPY --chown=1001:1001 ["./src/Eds.IdentityService.Application/.", "./src/Eds.IdentityService.Application/"]
COPY --chown=1001:1001 ["./src/Eds.IdentityService.Domain/.", "./src/Eds.IdentityService.Domain/"]
COPY --chown=1001:1001 ["./src/Eds.IdentityService.EntityFrameworkCore/.", "./src/Eds.IdentityService.EntityFrameworkCore/"]
COPY --chown=1001:1001 ["./src/Eds.IdentityService.Http.Host/.", "./src/Eds.IdentityService.Http.Host/"]

# Build
RUN dotnet build "./src/Eds.IdentityService.Http.Host/Eds.IdentityService.Http.Host.csproj" \
    --no-restore \
    --configuration Release \
    --verbosity minimal

# (Opsiyonel) Test
RUN dotnet test "./src/Eds.IdentityService.Http.Host/Eds.IdentityService.Http.Host.csproj" \
    --no-restore \
    --no-build \
    --configuration Release \
    --verbosity minimal

# Publish
RUN dotnet publish "./src/Eds.IdentityService.Http.Host/Eds.IdentityService.Http.Host.csproj" \
    --no-restore \
    --no-build \
    --configuration Release \
    --output /build-source/publish \
    /p:UseAppHost=false

# Final Runtime Image
FROM base AS final
WORKDIR /app
COPY --chown=1001:1001 --from=build-stage /build-source/publish .

# Runtime User
USER 1001

ENTRYPOINT ["dotnet", "Eds.IdentityService.Http.Host.dll"]
CMD []