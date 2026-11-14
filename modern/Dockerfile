FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/CqrsEsTodo.Api/CqrsEsTodo.Api.csproj src/CqrsEsTodo.Api/
COPY src/CqrsEsTodo.Infrastructure/CqrsEsTodo.Infrastructure.csproj src/CqrsEsTodo.Infrastructure/
COPY src/CqrsEsTodo.Domain/CqrsEsTodo.Domain.csproj src/CqrsEsTodo.Domain/
RUN dotnet restore src/CqrsEsTodo.Api/CqrsEsTodo.Api.csproj

COPY . .
RUN dotnet publish src/CqrsEsTodo.Api/CqrsEsTodo.Api.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CqrsEsTodo.Api.dll"]