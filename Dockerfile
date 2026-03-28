# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Cafe1316.slnx ./
COPY Cafe1316.API/ Cafe1316.API/
COPY Cafe1316.Application/ Cafe1316.Application/
COPY Cafe1316.Domain/ Cafe1316.Domain/
COPY Cafe1316.Infrastructure/ Cafe1316.Infrastructure/

# Safety: Remove ALL bin/obj artifacts including backslash-named directories
RUN find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true && \
    find . -maxdepth 4 -type d -name "bin*" -exec rm -rf {} + 2>/dev/null || true

# Restore NuGet packages
RUN dotnet restore "Cafe1316.API/Cafe1316.API.csproj"

# Publish directly (build is implicit in publish, skipping separate build step)
WORKDIR "/src/Cafe1316.API"
RUN dotnet publish "Cafe1316.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Cafe1316.API.dll"]
