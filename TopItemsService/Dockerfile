# Use the official .NET SDK image for .NET 8.0 as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for .NET 8.0 as a runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the build output
COPY --from=build /app/out .

# Set the ASP.NET Core URL binding to ensure the application listens on port 5000
ENV ASPNETCORE_URLS=http://*:5000

# Expose the port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "TopItemsService.dll"]
