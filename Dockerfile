FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /packages
USER root

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-stage
WORKDIR /build-source

COPY ["./nuget.config", "./"]
COPY ["./common.props", "./"]
COPY ["./common.version.props", "./"]

COPY ["./src/Eds.IdentityService.Domain/Eds.IdentityService.Domain.csproj", "./src/Eds.IdentityService.Domain/"]
COPY ["./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj", "./src/Eds.IdentityService.EntityFrameworkCore/"]

# Update NuGet source with secret credentials
RUN --mount=type=secret,id=NUGET_SECRET \
    export NUGET_SECRET=$(cat /run/secrets/NUGET_SECRET) && \
    dotnet nuget update source github \
        --username hsnsh \
        --password $NUGET_SECRET \
        --store-password-in-clear-text

RUN dotnet restore "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --verbosity minimal

COPY ["./src/Eds.IdentityService.Domain/.", "./src/Eds.IdentityService.Domain/"]
COPY ["./src/Eds.IdentityService.EntityFrameworkCore/.", "./src/Eds.IdentityService.EntityFrameworkCore/"]

RUN dotnet build "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --no-restore --configuration Release --verbosity minimal

RUN dotnet test "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --no-restore --no-build --configuration Release --verbosity minimal

# Pack with version
RUN --mount=type=secret,id=VERSION_NUMBER \
    --mount=type=secret,id=ACTION_NUMBER \
    dotnet pack "./src/Eds.IdentityService.Domain/Eds.IdentityService.Domain.csproj" \
        --no-restore --no-build --configuration Release \
        --output ./packages \
        -p:PackageVersion=$(cat /run/secrets/VERSION_NUMBER).$(cat /run/secrets/ACTION_NUMBER)

RUN --mount=type=secret,id=VERSION_NUMBER \
    --mount=type=secret,id=ACTION_NUMBER \
    dotnet pack "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" \
        --no-restore --no-build --configuration Release \
        --output ./packages \
        -p:PackageVersion=$(cat /run/secrets/VERSION_NUMBER).$(cat /run/secrets/ACTION_NUMBER)

# Final stage
FROM base AS final
WORKDIR /packages
COPY --from=build-stage /build-source/packages .

# Push to private NuGet source using secrets securely
RUN --mount=type=secret,id=NUGET_SOURCE \
    --mount=type=secret,id=NUGET_SECRET \
    dotnet nuget push *.nupkg \
        --source $(cat /run/secrets/NUGET_SOURCE) \
        --api-key $(cat /run/secrets/NUGET_SECRET) \
        --skip-duplicate
