# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything explicitly to ensure no folder is missed
# Copy everything explicitly to ensure no folder is missed
COPY Cafe1316.slnx ./
# Copy global props to fix SDK issues
COPY Directory.Build.props ./
COPY Cafe1316.API/ Cafe1316.API/
COPY Cafe1316.Application/ Cafe1316.Application/
COPY Cafe1316.Domain/ Cafe1316.Domain/
COPY Cafe1316.Infrastructure/ Cafe1316.Infrastructure/

# Safety: Remove any bin/obj folders that might have slipped in (to avoid .NET version mismatch)
# Safety: Aggressively remove any bin/obj folders (including those with backslashes)
RUN rm -rf */bin* */obj*

# --- DIAGNOSTIC BLOCK ---
RUN dotnet --info
RUN ls -R /src/Cafe1316.Infrastructure
RUN echo "--- Infrastructure CSPROJ Content ---" && cat /src/Cafe1316.Infrastructure/Cafe1316.Infrastructure.csproj
RUN echo "--- Directory.Build.props Content ---" && cat /src/Directory.Build.props
# ------------------------

# Restore dependencies
RUN dotnet restore "Cafe1316.Infrastructure/Cafe1316.Infrastructure.csproj"
# Try building Infrastructure explicitly with DETAILED logging to see why it misses files
RUN dotnet build "Cafe1316.Infrastructure/Cafe1316.Infrastructure.csproj" -c Release -v detailed

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
