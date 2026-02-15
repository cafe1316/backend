# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything explicitly to ensure no folder is missed
COPY Cafe1316.slnx ./
COPY Cafe1316.API/ Cafe1316.API/
COPY Cafe1316.Application/ Cafe1316.Application/
COPY Cafe1316.Domain/ Cafe1316.Domain/
COPY Cafe1316.Infrastructure/ Cafe1316.Infrastructure/

# Safety: Remove any bin/obj folders that might have slipped in (to avoid .NET version mismatch)
RUN find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} +

# Restore dependencies
RUN dotnet restore "Cafe1316.API/Cafe1316.API.csproj"

# Build the project
WORKDIR "/src/Cafe1316.API"
RUN dotnet build "Cafe1316.API.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "Cafe1316.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cafe1316.API.dll"]
