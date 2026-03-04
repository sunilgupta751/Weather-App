# =============================
# Stage 1: Build (SDK Image - Badi hoti hai)
# =============================
<<<<<<< HEAD

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build  

WORKDIR /src

COPY WeatherApps.csproj ./

RUN dotnet restore "./WeatherApps.csproj"

COPY . .

ARG BUILD_CONFIGURATION=Release

RUN dotnet build "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build
=======
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Layer caching: Pehle sirf csproj restore karo
COPY ["WeatherApps.csproj", "."]
RUN dotnet restore "./WeatherApps.csproj"
>>>>>>> df671290d41d4d071f38a92f9d3d3683e1cfc0d6

# Ab baaki code copy karke build karo
COPY . .
RUN dotnet build "WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =============================
# Stage 2: Publish
# =============================
<<<<<<< HEAD

FROM build AS publish

RUN dotnet publish "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:TrimUnusedDependencies=true
=======
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
>>>>>>> df671290d41d4d071f38a92f9d3d3683e1cfc0d6

# =============================
# Stage 3: Final (Slim Runtime - Chhoti aur fast) (Best Balance of Size & Stability)
# =============================
<<<<<<< HEAD

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
=======
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5132
# Production p niche waliline hata dena maine ye line sirf swagger ke liye likhi hai kyoki development main swaaggr ui ko dikhana thik hai
ENV ASPNETCORE_ENVIRONMENT=Development 
# Best Practice: .NET 9 ki images mein 'app' user default hota hai.
# Isse root access ki vulnerability khatam ho jati hai.
USER app

# Publish stage se sirf zaroori files uthao
COPY --from=publish /app/publish .
>>>>>>> df671290d41d4d071f38a92f9d3d3683e1cfc0d6

# Standard ports for .NET 8/9
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "WeatherApps.dll"]
