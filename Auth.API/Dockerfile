# syntax=docker/dockerfile:1

############################
# 1) Build your app
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /source

# Copy and restore
COPY ./PharmaTrack/Auth.API/*.csproj ./Auth.API/
COPY ./PharmaTrack/PharmaTrack.Shared/*.csproj ./PharmaTrack.Shared/
COPY ./PharmaTrack/PharmaTrack.Core/*.csproj ./PharmaTrack.Core/
RUN dotnet restore ./Auth.API

# Copy the rest and publish
COPY ./PharmaTrack/Auth.API ./Auth.API
COPY ./PharmaTrack/PharmaTrack.Shared ./PharmaTrack.Shared
COPY ./PharmaTrack/PharmaTrack.Core ./PharmaTrack.Core

WORKDIR /source/Auth.API
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release \
      --self-contained false \
      -o /app

############################
# 2) Generate PFX + run
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

WORKDIR /app

# Install globalization support, CA certs
RUN apk add --no-cache icu-libs ca-certificates && update-ca-certificates

# Copy published output
COPY --from=build /app .

# Prepare mount point for your host-exported PFX
RUN mkdir -p /https && chmod 755 /https

 # Tell Kestrel where to find the PFX
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=YourP@ssw0rd! \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80 443

USER $APP_UID

ENTRYPOINT ["dotnet", "Auth.API.dll"]