# BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build


WORKDIR /source

COPY VetControl.slnx .
COPY ./Api/*.csproj ./Api/
COPY ./Api/packages.lock.json ./Api
RUN dotnet restore --locked-mode

COPY ./Api/. ./Api/
RUN dotnet build --no-restore

WORKDIR /source/Api
RUN dotnet tool install --global dotnet-ef

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




# RUN

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Api.dll"]