
# =============================
# Stage 1: Build & Restore
# =============================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build  

WORKDIR /src

COPY WeatherApps.csproj ./

RUN dotnet restore "./WeatherApps.csproj"

COPY . .

ARG BUILD_CONFIGURATION=Release

RUN dotnet build "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build


# =============================
# Stage 2: Publish
# =============================

FROM build AS publish

RUN dotnet publish "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =============================
# Stage 3: Final
# =============================

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

WORKDIR /app

USER $APP_UID

# Expose ports
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherApps.dll"]
