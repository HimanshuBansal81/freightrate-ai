FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY services/auth-service/auth-service.csproj services/auth-service/
RUN dotnet restore services/auth-service/auth-service.csproj

COPY services/auth-service/. services/auth-service/
RUN dotnet publish services/auth-service/auth-service.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "auth-service.dll"]
