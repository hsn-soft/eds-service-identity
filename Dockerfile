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

RUN --mount=type=secret,id=NUGET_SECRET \
    export NUGET_SECRET=$(cat /run/secrets/NUGET_SECRET) && \
    echo ${NUGET_SECRET} > ./nuget_secret
    
RUN dotnet nuget update source github --username hsnsh --password $(cat ./nuget_secret) --store-password-in-clear-text

RUN dotnet restore "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --verbosity minimal

COPY ["./src/Eds.IdentityService.Domain/.", "./src/Eds.IdentityService.Domain/"]
COPY ["./src/Eds.IdentityService.EntityFrameworkCore/.", "./src/Eds.IdentityService.EntityFrameworkCore/"]

RUN dotnet build "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --no-restore --configuration Release --verbosity minimal

RUN dotnet test "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --no-restore --no-build --configuration Release --verbosity minimal

RUN --mount=type=secret,id=VERSION_NUMBER \
    export VERSION_NUMBER=$(cat /run/secrets/VERSION_NUMBER) && \
    echo ${VERSION_NUMBER} > ./version_number

RUN --mount=type=secret,id=ACTION_NUMBER \
    export ACTION_NUMBER=$(cat /run/secrets/ACTION_NUMBER) && \
    echo ${ACTION_NUMBER} > ./action_number

RUN dotnet pack "./src/Eds.IdentityService.Domain/Eds.IdentityService.Domain.csproj" --no-restore --no-build --configuration Release --output ./packages -p:PackageVersion=$(cat ./version_number).$(cat ./action_number)
RUN dotnet pack "./src/Eds.IdentityService.EntityFrameworkCore/Eds.IdentityService.EntityFrameworkCore.csproj" --no-restore --no-build --configuration Release --output ./packages -p:PackageVersion=$(cat ./version_number).$(cat ./action_number)

FROM base AS final
WORKDIR /packages
COPY --from=build-stage /build-source/packages .

RUN --mount=type=secret,id=NUGET_SOURCE \
    export NUGET_SOURCE=$(cat /run/secrets/NUGET_SOURCE) && \
    echo ${NUGET_SOURCE} > ./nuget_source

RUN --mount=type=secret,id=NUGET_SECRET \
    export NUGET_SECRET=$(cat /run/secrets/NUGET_SECRET) && \
    echo ${NUGET_SECRET} > ./nuget_secret

RUN dotnet nuget push *.nupkg --source $(cat ./nuget_source) --api-key $(cat ./nuget_secret) --skip-duplicate
