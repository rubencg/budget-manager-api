FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BudgetManager.Service.Api/BudgetManager.Service.Api.csproj", "BudgetManager.Service.Api/"]
RUN dotnet restore "BudgetManager.Service.Api/BudgetManager.Service.Api.csproj"
COPY . .
WORKDIR "/src/BudgetManager.Service.Api"
RUN dotnet build "BudgetManager.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BudgetManager.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BudgetManager.Service.Api.dll"]
