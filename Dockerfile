# ======== Build stage ========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el repo
COPY . .

# Restauramos y publicamos la WebApp
RUN dotnet restore ArtemisBanking.sln
RUN dotnet publish ArtemisBanking.WebApp/ArtemisBanking.WebApp.csproj -c Release -o /app/out

# ======== Runtime stage ========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://0.0.0.0:3000
EXPOSE 3000

ENTRYPOINT ["dotnet", "ArtemisBanking.WebApp.dll"]
