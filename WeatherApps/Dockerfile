# =============================
# Stage 1: Build (SDK Image - Badi hoti hai)
# =============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Layer caching: Pehle sirf csproj restore karo
COPY ["WeatherApps.csproj", "."]
RUN dotnet restore "./WeatherApps.csproj"

# Ab baaki code copy karke build karo
COPY . .
RUN dotnet build "WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =============================
# Stage 2: Publish
# =============================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WeatherApps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =============================
# Stage 3: Final (Slim Runtime - Chhoti aur fast) (Best Balance of Size & Stability)
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5132
ENV ASPNETCORE_ENVIRONMENT=Development # Production p y line hata dena maine ye line sirf swagger ke liye likhi hai 
# Best Practice: .NET 9 ki images mein 'app' user default hota hai.
# Isse root access ki vulnerability khatam ho jati hai.
USER app

# Publish stage se sirf zaroori files uthao
COPY --from=publish /app/publish .

# Standard ports for .NET 8/9
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "WeatherApps.dll"]
