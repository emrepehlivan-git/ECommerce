FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj", "src/Presentation/ECommerce.WebAPI/"]
COPY ["src/Core/ECommerce.Application/ECommerce.Application.csproj", "src/Core/ECommerce.Application/"]
COPY ["src/Core/ECommerce.Domain/ECommerce.Domain.csproj", "src/Core/ECommerce.Domain/"]
COPY ["src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/Infrastructure/ECommerce.Infrastructure/"]
COPY ["src/Infrastructure/ECommerce.Persistence/ECommerce.Persistence.csproj", "src/Infrastructure/ECommerce.Persistence/"]
RUN dotnet restore "src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/Presentation/ECommerce.WebAPI"
RUN dotnet build "ECommerce.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECommerce.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
# Install OpenSSL
RUN apt-get update && \
    apt-get install -y openssl ca-certificates

# Create a script to generate the certificate
RUN echo '#!/bin/bash\n\
if [ ! -f /app/webapi.pfx ]; then\n\
    openssl req -x509 -newkey rsa:4096 -keyout /tmp/key.pem -out /tmp/cert.pem -days 365 -nodes -subj "/CN=localhost"\n\
    openssl pkcs12 -export -out /app/webapi.pfx -inkey /tmp/key.pem -in /tmp/cert.pem -password pass:YourSecurePassword\n\
    rm /tmp/key.pem /tmp/cert.pem\n\
fi\n\
dotnet ECommerce.WebAPI.dll' > /app/startup.sh && \
chmod +x /app/startup.sh

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_HTTPS_PORT=8081
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/webapi.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword

ENTRYPOINT ["/app/startup.sh"]
