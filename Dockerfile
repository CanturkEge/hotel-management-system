FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json ./
COPY src/HotelManagement.Domain/HotelManagement.Domain.csproj src/HotelManagement.Domain/
COPY src/HotelManagement.Application/HotelManagement.Application.csproj src/HotelManagement.Application/
COPY src/HotelManagement.Infrastructure/HotelManagement.Infrastructure.csproj src/HotelManagement.Infrastructure/
COPY src/HotelManagement.Web/HotelManagement.Web.csproj src/HotelManagement.Web/
RUN dotnet restore src/HotelManagement.Web/HotelManagement.Web.csproj

COPY src/ src/
RUN dotnet publish src/HotelManagement.Web/HotelManagement.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "HotelManagement.Web.dll"]
