FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Posts.Api/Posts.Api.csproj", "src/Posts.Api/"]
COPY ["src/Posts.Application/Posts.Application.csproj", "src/Posts.Application/"]
COPY ["src/Posts.Contract/Posts.Contract.csproj", "src/Posts.Contract/"]
COPY ["src/Posts.Domain/Posts.Domain.csproj", "src/Posts.Domain/"]
COPY ["src/Posts.Infrastructure/Posts.Infrastructure.csproj", "src/Posts.Infrastructure/"]
RUN dotnet restore "./src/Posts.Api/Posts.Api.csproj"
COPY . .
WORKDIR "/src/src/Posts.Api"
RUN dotnet build "./Posts.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Posts.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/Posts.Migrations ./Migrations
ENTRYPOINT ["dotnet", "Posts.Api.dll"]