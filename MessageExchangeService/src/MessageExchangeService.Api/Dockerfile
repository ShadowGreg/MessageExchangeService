FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# Переключаемся на пользователя root для установки пакетов
USER root

# Устанавливаем curl и wget
RUN apt-get update && apt-get install -y curl wget

# Возвращаемся к пользователю, используемому для выполнения приложения (например, app)
USER app

EXPOSE 5252

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MessageExchangeService/src/MessageExchangeService.Api/MessageExchangeService.Api.csproj", "MessageExchangeService/src/MessageExchangeService.Api/"]
COPY ["MessageExchangeService/src/MessageExchangeService.infrastructure/MessageExchangeService.infrastructure.csproj", "MessageExchangeService/src/MessageExchangeService.infrastructure/"]
COPY ["MessageExchangeService/src/MessageExchangeService.Domain/MessageExchangeService.Domain.csproj", "MessageExchangeService/src/MessageExchangeService.Domain/"]
RUN dotnet restore "MessageExchangeService/src/MessageExchangeService.Api/MessageExchangeService.Api.csproj"
COPY . .
WORKDIR "/src/MessageExchangeService/src/MessageExchangeService.Api"
RUN dotnet build "MessageExchangeService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MessageExchangeService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageExchangeService.Api.dll"]
