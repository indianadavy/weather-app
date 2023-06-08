# Base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY ./WeatherApp/WeatherApp.csproj .
RUN dotnet restore

# Copy the entire project and build
COPY ./WeatherApp/ .
RUN dotnet publish -c Release -o out

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the required port
EXPOSE 5102

# Start the application
ENTRYPOINT ["dotnet", "WeatherApp.dll"]
