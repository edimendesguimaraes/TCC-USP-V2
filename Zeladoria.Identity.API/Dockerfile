FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copia toda a solução para garantir que acha o Application, Domain e Infrastructure
COPY . .
WORKDIR "/src/Zeladoria.Identity.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Zeladoria.Identity.API.dll"]