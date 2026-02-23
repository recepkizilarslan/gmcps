FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Gmcps/Gmcps.csproj src/Gmcps/
RUN dotnet restore src/Gmcps/Gmcps.csproj

COPY src/ src/
RUN dotnet publish src/Gmcps/Gmcps.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
COPY data/ data/

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Gmcps.dll"]
