FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RealEase.API/RealEase.API.csproj", "RealEase.API/"]
COPY ["RealEase.Application/RealEase.Application.csproj", "RealEase.Application/"]
COPY ["RealEase.Domain/RealEase.Domain.csproj", "RealEase.Domain/"]
COPY ["RealEase.Persistence/RealEase.Persistence.csproj", "RealEase.Persistence/"]
RUN dotnet restore "RealEase.API/RealEase.API.csproj"

COPY . .
WORKDIR "/src/RealEase.API"
RUN dotnet build "RealEase.API.csproj" -c Release -o /app/build
RUN dotnet publish "RealEase.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RealEase.API.dll"]
