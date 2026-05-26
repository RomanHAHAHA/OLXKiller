FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OLXKiller.Tests/OLXKiller.Tests.csproj", "OLXKiller.Tests/"]
COPY ["UsersService/UsersService.csproj", "UsersService/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["EmailService/EmailService.csproj", "EmailService/"]
COPY ["LogsService/LogsService.csproj", "LogsService/"]
COPY ["CartsService/CartsService.csproj", "CartsService/"]
COPY ["ProductsService/ProductsService.csproj", "ProductsService/"]
COPY ["ReviewsService/ReviewsService.csproj", "ReviewsService/"]
COPY ["OrdersService/OrdersService.csproj", "OrdersService/"]
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["ChatsService/ChatsService.csproj", "ChatsService/"]
RUN dotnet restore "OLXKiller.Tests/OLXKiller.Tests.csproj"
COPY . .
WORKDIR "/src/OLXKiller.Tests"
RUN dotnet build "./OLXKiller.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OLXKiller.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OLXKiller.Tests.dll"]
