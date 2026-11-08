
# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["InsightFlow.Core/InsightFlow.Core.csproj", "InsightFlow.Core/"]
COPY ["InsightFlow.Api/InsightFlow.Api.csproj", "InsightFlow.Api/"]

# Restore dependencies
RUN dotnet restore "InsightFlow.Api/InsightFlow.Api.csproj"

# Copy remaining source code
COPY . .

# Publish API
WORKDIR /src/InsightFlow.Api
RUN dotnet publish "InsightFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InsightFlow.Api.dll"]
