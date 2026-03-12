FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia a solução e os arquivos de projeto primeiro (para aproveitar o cache do Docker)
COPY ["TCC-USP.slnx", "./"]
COPY ["Zeladoria.Domain/Zeladoria.Domain.csproj", "Zeladoria.Domain/"]
COPY ["Zeladoria.Application/Zeladoria.Application.csproj", "Zeladoria.Application/"]
COPY ["Zeladoria.Infrastructure/Zeladoria.Infrastructure.csproj", "Zeladoria.Infrastructure/"]
COPY ["Zeladoria.API/Zeladoria.API.csproj", "Zeladoria.API/"]

# Restaura as dependências lendo o arquivo da solução
RUN dotnet restore "TCC-USP.slnx"

# Copia todo o resto do código
COPY . .
WORKDIR "/src/Zeladoria.API"

# Compila o projeto da API (que já vai puxar as outras camadas por referência)
RUN dotnet publish "Zeladoria.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Zeladoria.API.dll"]