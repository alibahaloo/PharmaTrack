# syntax=docker/dockerfile:1

############################
# 1) Build your app
############################
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /source

# Copy csproj & restore
COPY ./PharmaTrack/Drug.API/*.csproj ./Drug.API/
COPY ./PharmaTrack/PharmaTrack.Shared/*.csproj ./PharmaTrack.Shared/
RUN dotnet restore ./Drug.API

# Copy code + DataFiles and publish
COPY ./PharmaTrack/Drug.API ./Drug.API
COPY ./PharmaTrack/PharmaTrack.Shared ./PharmaTrack.Shared

WORKDIR /source/Drug.API
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

# Copy published output + DataFiles
COPY --from=build /app .  
COPY --from=build /source/Drug.API/DataFiles ./DataFiles

# Prepare mount point for your host-exported PFX
RUN mkdir -p /https && chmod 755 /https

 # Tell Kestrel where to find the PFX
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=YourP@ssw0rd! \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80 443

USER $APP_UID

ENTRYPOINT ["dotnet", "Drug.API.dll"]