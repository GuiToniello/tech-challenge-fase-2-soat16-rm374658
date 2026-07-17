# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia primeiro os csproj para maximizar o cache do restore.
# Se só o código mudar, essa etapa tende a ser reaproveitada.
COPY ["TechChallenge.Oficina.API/TechChallenge.Oficina.API.csproj", "TechChallenge.Oficina.API/"]
COPY ["TechChallenge.Oficina.Application/TechChallenge.Oficina.Application.csproj", "TechChallenge.Oficina.Application/"]
COPY ["TechChallenge.Oficina.DB/TechChallenge.Oficina.DB.csproj", "TechChallenge.Oficina.DB/"]
COPY ["TechChallenge.Oficina.Infra/TechChallenge.Oficina.Infra.csproj", "TechChallenge.Oficina.Infra/"]
COPY ["TechChallenge.Oficina.Domain/TechChallenge.Oficina.Domain.csproj", "TechChallenge.Oficina.Domain/"]

RUN dotnet restore "TechChallenge.Oficina.API/TechChallenge.Oficina.API.csproj"

# Depois do restore, copia apenas os projetos necessários para compilar a solução.
# Isso evita trazer arquivos extras do contexto de build para a imagem.
COPY ["TechChallenge.Oficina.API/", "TechChallenge.Oficina.API/"]
COPY ["TechChallenge.Oficina.Application/", "TechChallenge.Oficina.Application/"]
COPY ["TechChallenge.Oficina.DB/", "TechChallenge.Oficina.DB/"]
COPY ["TechChallenge.Oficina.Infra/", "TechChallenge.Oficina.Infra/"]
COPY ["TechChallenge.Oficina.Domain/", "TechChallenge.Oficina.Domain/"]
WORKDIR /src/TechChallenge.Oficina.API
RUN dotnet publish "TechChallenge.Oficina.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instala uma ferramenta leve de HTTP para o HEALTHCHECK do container.
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

# Usa apenas o runtime para manter a imagem final menor e mais segura.
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

# A imagem oficial já expõe um usuário não-root via APP_UID.
USER $APP_UID
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=20s --retries=3 \
	CMD curl --fail --silent --show-error http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TechChallenge.Oficina.API.dll"]
