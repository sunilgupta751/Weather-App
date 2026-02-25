
# =============================
# Stage 1: Build & Restore
# =============================
#Use: Ye line kehti hai container ke andar full .NET SDK image use karo jisse code compile ho sake.Runtime image  compile nahi kar sakta”
#Internal / Layer perspective: Docker ye image pull karta hai → ek base layer create hoti hai. Ye layer immutable hai.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build  
#:Use: Container ke andar working folder set kiya /src.
#Internal: Har next COPY / RUN command /src ke relative path me execute hoti hai. Ye temporary workspace ke jaisa hai.
WORKDIR /src
#Use: Sirf project file copy ki → taaki dependencies restore ho sake.
#Internal: Docker temporarily ye file /src/WeatherApps.csproj me copy karta hai, abhi final image me
COPY WeatherApps.csproj ./
#Use: NuGet packages download karna.
#Internal: Docker ye restore layer me store karta hai. Agar .csproj same hai → layer reuse hoti hai → fast rebuild.
RUN dotnet restore "./WeatherApps.csproj"
#Use: Poora source code container me copy karna taaki build ho sake.
#Ye temporary copy hai /src me, abhi final image me nahi. Har file ek new layer me store hoti hai.
COPY . .
#Use: Release build mode set karna.
#Internal: Docker me ek variable define hota hai, jo build command me use hota hai.
ARG BUILD_CONFIGURATION=Release
#Use: Build binaries /app/build me generate karna.
#Internal: Ye ek new Docker layer create karta hai → ab /app/build available hai publish stage ke liye.
RUN dotnet build "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build


# =============================
# Stage 2: Publish
# =============================
#Use: Build stage se binaries le ke publish stage start karna.
#Internal: Docker copy-on-write ka use karta hai → build stage ka snapshot base hai.
FROM build AS publish
#Use:1 Ready-to-run binaries banaye /app/publish me.
#    2 UseAppHost=false → OS-specific host file create nahi hoti → cross-platform friendly.
#    3 TrimUnusedDependencies=true → unused DLLs remove → final image small


#Internal: 1 /app/publish temporary layer me store hoti hai.
#          2 Source code /src ab bhi publish stage me hai, lekin final runtime stage me copy nahi hoga.
 #here c means configuration
 #here o means ouput folder
 #UseAppHost means OS-specific host file create nahi hota → cross-platform.
 #TrimUnusedDependencies means unused DLLs remove
RUN dotnet publish "./WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:TrimUnusedDependencies=true



# =============================
# Stage 3: Final
# =============================
#Use: Lightweight runtime image for production.
#Internal: SDK aur build artifacts nahi → image small.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
#Use: App runtime folder.
#Internal: COPY aur ENTRYPOINT commands ka relative path /app hai.
WORKDIR /app
#Use: Non-root user → security.
#Internal: Linux filesystem permissions /app folder me set ho jaate → container secure.
ARG APP_UID=1000
USER $APP_UID
#Use: Networking / orchestrator (Docker / Kubernetes) ko ports dikhana.
#Internal: Ports container network stack me open hote hain.
# Expose ports
EXPOSE 8080
EXPOSE 8081
#Use: Sirf publish binaries copy karo final image me.
#Internal: Docker copy-on-write mechanism use karta hai → source code aur build layers final image me include nahi.
# Copy published binaries from previous stage
COPY --from=publish /app/publish .
#Use: Container start hote hi app run ho.
#Internal: Docker sets container process to run this command.
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


