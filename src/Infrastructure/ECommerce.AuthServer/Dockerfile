FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN apt-get update && apt-get install -y unzip && rm -rf /var/lib/apt/lists/*
RUN curl -fsSL https://bun.sh/install | bash
ENV PATH="/root/.bun/bin:$PATH"

COPY ["Directory.Packages.props", "./"]
COPY ["src/Core/ECommerce.SharedKernel/ECommerce.SharedKernel.csproj", "src/Core/ECommerce.SharedKernel/"]
COPY ["src/Infrastructure/ECommerce.AuthServer/ECommerce.AuthServer.csproj", "src/Infrastructure/ECommerce.AuthServer/"]
COPY ["src/Infrastructure/ECommerce.Persistence/ECommerce.Persistence.csproj", "src/Infrastructure/ECommerce.Persistence/"]
COPY ["src/Core/ECommerce.Application/ECommerce.Application.csproj", "src/Core/ECommerce.Application/"]
COPY ["src/Core/ECommerce.Domain/ECommerce.Domain.csproj", "src/Core/ECommerce.Domain/"]
COPY ["src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/Infrastructure/ECommerce.Infrastructure/"]
RUN dotnet restore "src/Infrastructure/ECommerce.AuthServer/ECommerce.AuthServer.csproj"
COPY . .
WORKDIR "/src/src/Infrastructure/ECommerce.AuthServer"

RUN bun install
RUN bun run build-css-prod

RUN dotnet build "ECommerce.AuthServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECommerce.AuthServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && \
    apt-get install -y openssl ca-certificates

RUN echo '#!/bin/bash\n\
if [ ! -f /app/auth.pfx ]; then\n\
    # Create a config file for the certificate with SAN\n\
    cat > /tmp/cert.conf <<EOF\n\
[req]\n\
distinguished_name = req_distinguished_name\n\
req_extensions = v3_req\n\
prompt = no\n\
\n\
[req_distinguished_name]\n\
CN = ecommerce.authserver\n\
\n\
[v3_req]\n\
keyUsage = critical, digitalSignature, keyEncipherment, keyAgreement\n\
extendedKeyUsage = critical, serverAuth, clientAuth\n\
subjectAltName = @alt_names\n\
basicConstraints = critical, CA:false\n\
\n\
[alt_names]\n\
DNS.1 = ecommerce.authserver\n\
DNS.2 = localhost\n\
IP.1 = 127.0.0.1\n\
EOF\n\
    openssl req -x509 -newkey rsa:4096 -keyout /tmp/key.pem -out /tmp/cert.pem -days 365 -nodes -config /tmp/cert.conf -extensions v3_req\n\
    openssl pkcs12 -export -out /app/auth.pfx -inkey /tmp/key.pem -in /tmp/cert.pem -password pass:YourSecurePassword\n\
    rm /tmp/key.pem /tmp/cert.pem /tmp/cert.conf\n\
fi\n\
dotnet ECommerce.AuthServer.dll' > /app/startup.sh && \
chmod +x /app/startup.sh

ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_HTTPS_PORT=8081
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/auth.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword

ENTRYPOINT ["/app/startup.sh"]
