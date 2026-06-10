# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and project files, then restore dependencies
COPY Pcf.Common/*.csproj Pcf.Common/
COPY Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/*.csproj Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/
COPY Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/*.csproj Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/
COPY Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/*.csproj Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/
COPY Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/*.csproj Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/
COPY Pcf.GivingToCustomer/Pcf.GivingToCustomer.IntegrationTests/*.csproj Pcf.GivingToCustomer/Pcf.GivingToCustomer.IntegrationTests/
RUN dotnet restore Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/Pcf.GivingToCustomer.WebHost.csproj

# Copy the remaining files and publish the application
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet publish -c Release -o out

# Use the .NET 8 runtime for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.GivingToCustomer.WebHost.dll"]