#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "SampleApplication.sln" "SampleApplication.sln"
COPY ["Services/SampleApi1/SampleApi1.csproj", "Services/SampleApi1/"]
COPY ["Services/SampleApi2/SampleApi2.csproj", "Services/SampleApi2/"]
COPY ["Web/SampleWebApp/SampleWebApp.csproj", "Web/SampleWebApp/"]
COPY ["Web/DuendeIdentityServer6/DuendeIdentityServer6.csproj", "Web/DuendeIdentityServer6/"]
COPY ["BuildingBlocks/Sample.Contracts/Sample.Contracts.csproj", "BuildingBlocks/Sample.Contracts/"]
COPY ["BuildingBlocks/WebHost.Customization/WebHost.Customization.csproj", "BuildingBlocks/WebHost.Customization/"]
COPY ["ApiGateways/Web.Bff.Sample/Web.Bff.Sample.csproj", "ApiGateways/Web.Bff.Sample/"]
COPY "Directory.Packages.props" "Directory.Packages.props"
COPY "docker-compose.dcproj" "docker-compose.dcproj"
COPY "NuGet.config" "NuGet.config"
RUN dotnet restore "SampleApplication.sln"

COPY . .
WORKDIR "/src/Web/DuendeIdentityServer6"
RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DuendeIdentityServer6.dll"]