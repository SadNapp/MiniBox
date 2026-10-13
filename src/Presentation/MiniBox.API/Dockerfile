FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY MiniBox.sln ./
COPY src/Core/MiniBox.Domain/MiniBox.Domain.csproj src/Core/MiniBox.Domain/
COPY src/Core/MiniBox.Application/MiniBox.Application.csproj src/Core/MiniBox.Application/
COPY src/Infrastructure/MiniBox.Infrastructure/MiniBox.Infrastructure.csproj src/Infrastructure/MiniBox.Infrastructure/
COPY src/Presentation/MiniBox.API/MiniBox.API.csproj src/Presentation/MiniBox.API/
RUN dotnet restore src/Presentation/MiniBox.API/MiniBox.API.csproj
COPY src/ ./src/
RUN dotnet publish src/Presentation/MiniBox.API/MiniBox.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=10 CMD dotnet /app/MiniBox.API.dll --health-check || exit 1
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MiniBox.API.dll"]
