# Этап 1: сборка
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/BookLibrary.Api/BookLibrary.Api.csproj src/BookLibrary.Api/
RUN dotnet restore src/BookLibrary.Api/BookLibrary.Api.csproj
COPY src/BookLibrary.Api/ src/BookLibrary.Api/
RUN dotnet publish src/BookLibrary.Api/BookLibrary.Api.csproj -c Release -o /app/publish --no-restore

# Этап 2: рантайм без SDK
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookLibrary.Api.dll"]
