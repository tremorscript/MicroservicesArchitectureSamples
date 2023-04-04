#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY ["InitialDevSetup.sln", "InitialDevSetup.sln"]
COPY ["SampleApi1/SampleApi1.csproj", "SampleApi1/"]
COPY ["SampleApi2/SampleApi2.csproj", "SampleApi2/"]
COPY ["SampleWebApp/SampleWebApp.csproj", "SampleWebApp/"]
COPY ["docker-compose.dcproj", "docker-compose.dcproj"]
COPY "NuGet.config" "NuGet.config"

RUN dotnet restore "InitialDevSetup.sln"

COPY . .
WORKDIR "/src/SampleApi1"
RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SampleApi1.dll"]