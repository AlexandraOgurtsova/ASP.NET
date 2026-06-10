# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and project files, then restore dependencies
COPY Pcf.Common/*.csproj Pcf.Common/
COPY Pcf.Administration/Pcf.Administration.Core/*.csproj Pcf.Administration/Pcf.Administration.Core/
COPY Pcf.Administration/Pcf.Administration.DataAccess/*.csproj Pcf.Administration/Pcf.Administration.DataAccess/
COPY Pcf.Administration/Pcf.Administration.WebHost/*.csproj Pcf.Administration/Pcf.Administration.WebHost/
COPY Pcf.Administration/Pcf.Administration.IntegrationTests/*.csproj Pcf.Administration/Pcf.Administration.IntegrationTests/
RUN dotnet restore Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj

# Copy the remaining files and publish the application
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet publish -c Release -o out

# Use the .NET 8 runtime for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.Administration.WebHost.dll"]