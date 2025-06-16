# Byggfas
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiera projektfiler och återställ beroenden
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Körfas
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Exponera porten som din app använder (oftast 80)
EXPOSE 80

ENTRYPOINT ["dotnet", "BookApi.dll"]
