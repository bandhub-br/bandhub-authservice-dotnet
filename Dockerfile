FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BandHub.AuthService/BandHub.AuthService.csproj", "BandHub.AuthService/"]
RUN dotnet restore "BandHub.AuthService/BandHub.AuthService.csproj"
COPY . .
WORKDIR "/src/BandHub.AuthService"
RUN dotnet build "BandHub.AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BandHub.AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BandHub.AuthService.dll"]
