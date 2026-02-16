# Etapa 1: BUILD (Usa una imagen con SDK para compilar)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y restauramos dependencias
COPY ["GC.Account.API.csproj", "./"]
RUN dotnet restore "GC.Account.API.csproj"

# Copiamos el resto del código y publicamos la aplicación
COPY . .
RUN dotnet publish "GC.Account.API.csproj" -c Release -o /app/publish

# Etapa 2: FINAL (Usa una imagen más ligera para ejecutar la aplicación)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponemos el puerto en el que la aplicación escuchará
EXPOSE 8080

# Definimos el punto de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "GC.Account.API.dll"]