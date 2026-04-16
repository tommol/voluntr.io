FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY apps/api-dotnet/Voluntrio.slnx apps/api-dotnet/Voluntrio.slnx
COPY apps/api-dotnet/src/Voluntrio.Api/Voluntrio.Api.csproj apps/api-dotnet/src/Voluntrio.Api/
RUN dotnet restore apps/api-dotnet/src/Voluntrio.Api/Voluntrio.Api.csproj

COPY apps/api-dotnet/. apps/api-dotnet/
RUN dotnet publish apps/api-dotnet/src/Voluntrio.Api/Voluntrio.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Voluntrio.Api.dll"]
