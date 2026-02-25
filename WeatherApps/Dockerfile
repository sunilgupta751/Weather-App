
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

RUN dotnet publish "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:TrimUnusedDependencies=true

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










#
## See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
## This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#ENV ASPNETCORE_URLS=http://0.0.0.0:5132
## This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
##Sirf project file copy hoti hai /src/WeatherApp
##Reason: Docker cache → agar code change ho, lekin dependencies same → restore fast ho
#COPY WeatherApps.csproj ./
##.csproj ke dependencies (NuGet packages) download hote hain
#RUN dotnet restore "./WeatherApps.csproj"
#
##Ab poora source code container ke /src folder me copy hota hai,
##Pehle sirf .csproj copy karke caching optimize kiya tha,
##Ab final build ke liye full code ready
#COPY . .
#
#RUN dotnet build "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "WeatherApps.dll"]


