# syntax=docker/dockerfile:1

############################
# 1) Build & publish the Blazor PWA
############################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS pwa-build
WORKDIR /src

# Install Python (needed by the Wasm native compile steps)
RUN apt-get update \
 && apt-get install -y --no-install-recommends python3 \
 && ln -s /usr/bin/python3 /usr/bin/python \
 && rm -rf /var/lib/apt/lists/*

# Install the Blazor WebAssembly workload
RUN dotnet workload install wasm-tools

# Copy only the PWA + its dependent project files for restore
COPY PharmaTrack/PharmaTrack.PWA/PharmaTrack.PWA.csproj    PharmaTrack.PWA/
COPY PharmaTrack/PharmaTrack.Core/PharmaTrack.Core.csproj  PharmaTrack.Core/
COPY PharmaTrack/PharmaTrack.Shared/PharmaTrack.Shared.csproj PharmaTrack.Shared/

# Restore the PWA (this brings in Core & Shared via ProjectReference)
RUN dotnet restore PharmaTrack.PWA/PharmaTrack.PWA.csproj

# Copy the rest of the source
COPY PharmaTrack/PharmaTrack.PWA    PharmaTrack.PWA
COPY PharmaTrack/PharmaTrack.Core   PharmaTrack.Core
COPY PharmaTrack/PharmaTrack.Shared PharmaTrack.Shared

# Publish the PWA’s static assets
WORKDIR /src/PharmaTrack.PWA
RUN dotnet publish -c Release -o /pwa

############################
# 2) Build & publish the Host
############################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS host-build
WORKDIR /src

# Copy & restore Host project
COPY PharmaTrack/PharmaTrack.Host/PharmaTrack.Host.csproj PharmaTrack.Host/
RUN dotnet restore PharmaTrack.Host/PharmaTrack.Host.csproj

# Copy & publish Host
COPY PharmaTrack/PharmaTrack.Host PharmaTrack.Host
WORKDIR /src/PharmaTrack.Host
RUN dotnet publish -c Release -o /host

############################
# 3) Final runtime image
############################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 3a) Copy Host binaries
COPY --from=host-build /host ./

# 3b) Copy the PWA static files (including _framework) into wwwroot
COPY --from=pwa-build /pwa/wwwroot ./wwwroot

# Prepare HTTPS cert mount point
RUN mkdir -p /https && chmod 755 /https

# Configure Kestrel & URLs
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=YourP@ssw0rd! \
    ASPNETCORE_URLS="https://+:443;http://+:80" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 80 443

ENTRYPOINT ["dotnet", "PharmaTrack.Host.dll"]
