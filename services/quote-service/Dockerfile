FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY services/quote-service/quote-service.csproj services/quote-service/
RUN dotnet restore services/quote-service/quote-service.csproj

COPY services/quote-service/. services/quote-service/
RUN dotnet publish services/quote-service/quote-service.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "quote-service.dll"]
