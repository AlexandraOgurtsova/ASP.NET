# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and project files, then restore dependencies
COPY Pcf.Common/*.csproj Pcf.Common/
COPY Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/*.csproj Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/
COPY Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/*.csproj Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/
COPY Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/*.csproj Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/
COPY Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/*.csproj Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/
COPY Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.UnitTests/*.csproj Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.UnitTests/
RUN dotnet restore Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/Pcf.ReceivingFromPartner.WebHost.csproj

# Copy the remaining files and publish the application
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet publish -c Release -o out

# Use the .NET 8 runtime for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.ReceivingFromPartner.WebHost.dll"]