FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /src

COPY ["PromoCodeFactory.Core/PromoCodeFactory.Core.csproj", "PromoCodeFactory.Core/"]
COPY ["PromoCodeFactory.DataAccess/PromoCodeFactory.DataAccess.csproj", "PromoCodeFactory.DataAccess/"]
COPY ["PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj", "PromoCodeFactory.WebHost/"]
COPY ["PromoCodeFactory.UnitTests/PromoCodeFactory.UnitTests.csproj", "PromoCodeFactory.UnitTests/"]
RUN dotnet restore 

COPY src/ .
RUN dotnet build -c Release -o --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=base /app/publish .
EXPOSE 5000

ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]