# syntax=docker/dockerfile:1

############################
# 1) Build your app
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /source

# Copy and restore
COPY ./PharmaTrack/Auth.API/*.csproj ./Auth.API/
COPY ./PharmaTrack/PharmaTrack.Shared/*.csproj ./PharmaTrack.Shared/
RUN dotnet restore ./Auth.API

# Copy the rest and publish
COPY ./PharmaTrack/Auth.API ./Auth.API
COPY ./PharmaTrack/PharmaTrack.Shared ./PharmaTrack.Shared

WORKDIR /source/Auth.API
ARG TARGETARCH
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release \
      --self-contained false \
      -o /app

############################
# 2) Generate PFX + run
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

WORKDIR /app

# Install globalization support, CA certs, and openssl
RUN apk add --no-cache icu-libs ca-certificates openssl \
    && update-ca-certificates

# Copy published output
COPY --from=build /app .

# Build-time PFX password (override with --build-arg if desired)
ARG PFX_PASSWORD=SuperSecretP@ss

# Generate a self-signed cert and package into PFX
RUN mkdir -p /https \
 && openssl req -x509 -nodes -days 365 \
      -subj "/CN=localhost" \
      -newkey rsa:2048 -keyout /https/key.pem \
      -out /https/cert.pem \
 && openssl pkcs12 -export \
      -in /https/cert.pem \
      -inkey /https/key.pem \
      -passout pass:${PFX_PASSWORD} \
      -out /https/aspnetapp.pfx

# allow non-root users to read the PFX
RUN chmod 644 /https/aspnetapp.pfx

# Tell Kestrel where to find the generated PFX
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=${PFX_PASSWORD} \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80 443

USER $APP_UID

ENTRYPOINT ["dotnet", "Auth.API.dll"]
