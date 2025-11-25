# ---------- STAGE 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el repo
COPY . .

# Restaurar paquetes y publicar SOLO la WebApp
RUN dotnet restore ArtemisBanking.sln

RUN dotnet publish ArtemisBanking.WebApp/ArtemisBanking.WebApp.csproj \
    -c Release \
    -o /app/out

# ---------- STAGE 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiamos lo publicado
COPY --from=build /app/out ./

# Railway suele usar el puerto 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ArtemisBanking.WebApp.dll"]
