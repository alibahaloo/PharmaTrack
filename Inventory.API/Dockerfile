# syntax=docker/dockerfile:1

############################
# 1) Build your app
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /source

# Copy project files and restore
COPY ./PharmaTrack/Inventory.API/*.csproj ./Inventory.API/
COPY ./PharmaTrack/PharmaTrack.Shared/*.csproj ./PharmaTrack.Shared/
COPY ./PharmaTrack/PharmaTrack.Core/*.csproj ./PharmaTrack.Core/
RUN dotnet restore ./Inventory.API

# Copy the rest of the code and publish
COPY ./PharmaTrack/Inventory.API ./Inventory.API
COPY ./PharmaTrack/PharmaTrack.Shared ./PharmaTrack.Shared
COPY ./PharmaTrack/PharmaTrack.Core ./PharmaTrack.Core

WORKDIR /source/Inventory.API
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release \
      --self-contained false \
      -o /app

############################
# 2) Generate PFX + run
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

WORKDIR /app

# Install globalization support
RUN apk add --no-cache icu-libs

# Copy published output
COPY --from=build /app .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80

USER $APP_UID

ENTRYPOINT ["dotnet", "Inventory.API.dll"]