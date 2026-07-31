FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/AutoSale.Api/AutoSale.Api.csproj", "src/AutoSale.Api/"]
COPY ["src/AutoSale.Application/AutoSale.Application.csproj", "src/AutoSale.Application/"]
COPY ["src/AutoSale.Domain/AutoSale.Domain.csproj", "src/AutoSale.Domain/"]
COPY ["src/AutoSale.Infrastructure/AutoSale.Infrastructure.csproj", "src/AutoSale.Infrastructure/"]
COPY ["src/BuildingBlocks/AutoSale.SharedKernel/AutoSale.SharedKernel.csproj", "src/BuildingBlocks/AutoSale.SharedKernel/"]
RUN dotnet restore "src/AutoSale.Api/AutoSale.Api.csproj"

COPY . .
RUN dotnet publish "src/AutoSale.Api/AutoSale.Api.csproj" --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "AutoSale.Api.dll"]
