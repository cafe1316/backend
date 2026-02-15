# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything immediately to ensure source files are present
COPY . .

# Restore dependencies
RUN dotnet restore "Cafe1316.API/Cafe1316.API.csproj"

# Build the project
WORKDIR "/src/Cafe1316.API"
RUN dotnet build "Cafe1316.API.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "Cafe1316.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cafe1316.API.dll"]
