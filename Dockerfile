# BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build


WORKDIR /source

COPY  VetControl.slnx .
COPY ./Api/*.csproj ./Api/
COPY ./Api/packages.lock.json ./Api
RUN dotnet restore Api/Api.csproj --locked-mode

COPY ./Api/. ./Api/
COPY ./Shared/. ./Shared/
RUN dotnet build Api/Api.csproj --no-restore

WORKDIR /source/Api
RUN dotnet tool install --global dotnet-ef --version 10.0.10

ENV PATH="$PATH:/root/.dotnet/tools"


RUN dotnet-ef migrations bundle \
    --self-contained \
    -r linux-x64 \
    -o /out/efbundle


RUN dotnet publish -c release -o /app --no-restore



# Migration
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrations

WORKDIR /app

COPY --from=build /out/efbundle .

ENTRYPOINT ["./efbundle"]



FROM node:24-alpine AS front
WORKDIR /app
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
ARG VITE_API_URL=""
ARG VITE_IDENTITY_URL=""
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_IDENTITY_URL=$VITE_IDENTITY_URL
RUN npm run build


# RUN

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM runtime AS api

RUN mkdir -p /var/lib/vet-control/keys && chown -R 1111:1111 /var/lib/vet-control
USER 1111

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=build --chown=1111:1111 /app ./
COPY --from=front --chown=1111:1111 /app/build ./wwwroot
ENTRYPOINT ["dotnet", "Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS auth-build
WORKDIR /source
COPY ./Auth/*.csproj ./Auth/
COPY ./Auth/packages.lock.json ./Auth/
RUN dotnet restore Auth/Auth.csproj --locked-mode
COPY ./Shared/. ./Shared/
COPY ./Auth/. ./Auth/
RUN dotnet publish Auth/Auth.csproj -c Release -o /auth-out --no-restore
RUN dotnet tool install --global dotnet-ef --version 10.0.10
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /source/Auth
RUN dotnet-ef migrations bundle --self-contained -r linux-x64 -o /auth-bundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS auth-migrations
WORKDIR /app
COPY --from=auth-build /auth-bundle ./efbundle
ENTRYPOINT ["./efbundle"]

FROM runtime AS auth
WORKDIR /app
ENV HOME=/var/lib/vet-control
RUN mkdir -p /var/lib/vet-control/keys /var/lib/vet-control/.dotnet/corefx/cryptography/x509stores \
    && chown -R 1111:1111 /var/lib/vet-control
USER 1111
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081
COPY --from=auth-build --chown=1111:1111 /auth-out ./
ENTRYPOINT ["dotnet", "Auth.dll"]

# Preserve the clinical application as the default image when no target is specified.
FROM api AS default
