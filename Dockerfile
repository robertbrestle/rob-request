# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["RobRequest.Server/RobRequest.Server.csproj", "RobRequest.Server/"]
COPY ["RobRequest.Shared/RobRequest.Shared.csproj", "RobRequest.Shared/"]
RUN dotnet restore "RobRequest.Server/RobRequest.Server.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/RobRequest.Server"

# Build the application
RUN dotnet build "RobRequest.Server.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RobRequest.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Create a directory for the SQLite database and set the connection string
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data
VOLUME /app/data
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/robrequest.db"

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --chown=$APP_UID:$APP_UID --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RobRequest.Server.dll"]
