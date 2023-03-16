#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "SampleApplication.sln" "SampleApplication.sln"
COPY ["Services/SampleApi1/SampleApi1.csproj", "Services/SampleApi1/"]
COPY ["Services/SampleApi2/SampleApi2.csproj", "Services/SampleApi2/"]
COPY ["Web/SampleWebApp/SampleWebApp.csproj", "Web/SampleWebApp/"]
COPY ["Services/IdentityApi/IdentityApi.csproj", "Services/IdentityApi/"]
COPY ["ApiGateways/Web.Bff.Sample/Web.Bff.Sample.csproj", "ApiGateways/Web.Bff.Sample/"]
COPY "docker-compose.dcproj" "docker-compose.dcproj"
COPY "NuGet.config" "NuGet.config"
RUN dotnet restore "SampleApplication.sln"

COPY . .
WORKDIR "/src/Services/IdentityApi"
RUN dotnet build "IdentityApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]