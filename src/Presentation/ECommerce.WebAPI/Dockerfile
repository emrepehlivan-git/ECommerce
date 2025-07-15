FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy central package management files first
COPY ["Directory.Packages.props", "./"]
COPY ["src/Core/ECommerce.SharedKernel/ECommerce.SharedKernel.csproj", "src/Core/ECommerce.SharedKernel/"]
COPY ["src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj", "src/Presentation/ECommerce.WebAPI/"]
COPY ["src/Core/ECommerce.Application/ECommerce.Application.csproj", "src/Core/ECommerce.Application/"]
COPY ["src/Core/ECommerce.Domain/ECommerce.Domain.csproj", "src/Core/ECommerce.Domain/"]
COPY ["src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/Infrastructure/ECommerce.Infrastructure/"]
COPY ["src/Infrastructure/ECommerce.Persistence/ECommerce.Persistence.csproj", "src/Infrastructure/ECommerce.Persistence/"]
RUN dotnet restore "src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/Presentation/ECommerce.WebAPI"
RUN dotnet build "ECommerce.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECommerce.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/src/Infrastructure/ECommerce.Persistence/SeedImages ./SeedImages

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ECommerce.WebAPI.dll"]
