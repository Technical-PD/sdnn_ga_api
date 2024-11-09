# Use official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./

COPY SdnnGa.Api/*.csproj ./SdnnGa.Api/
COPY SdnnGa.Core/*.csproj ./SdnnGa.Core/
COPY SdnnGa.Database/*.csproj ./SdnnGa.Database/
COPY SdnnGa.Infrastructure/*.csproj ./SdnnGa.Infrastructure/
COPY SdnnGa.Model/*.csproj ./SdnnGa.Model/
COPY SdnnGa.Services/*.csproj ./SdnnGa.Services/

RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /publish

# Use runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /publish
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "SdnnGa.Api.dll"]