# Build em duas etapas: o SDK compila e a imagem final leva só o runtime (menor e sem compilador).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura dependências primeiro, para aproveitar o cache de camadas do Docker
COPY Directory.Build.props .editorconfig Vacinacao.slnx ./
COPY src/Vacinacao.Domain/Vacinacao.Domain.csproj src/Vacinacao.Domain/
COPY src/Vacinacao.Application/Vacinacao.Application.csproj src/Vacinacao.Application/
COPY src/Vacinacao.Infrastructure/Vacinacao.Infrastructure.csproj src/Vacinacao.Infrastructure/
COPY src/Vacinacao.Api/Vacinacao.Api.csproj src/Vacinacao.Api/
RUN dotnet restore src/Vacinacao.Api/Vacinacao.Api.csproj

COPY src/ src/
RUN dotnet publish src/Vacinacao.Api/Vacinacao.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Roda com o usuário sem privilégios que já vem na imagem oficial
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Vacinacao.Api.dll"]
