FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY MiniBox.sln ./
COPY src/Core/MiniBox.Domain/MiniBox.Domain.csproj src/Core/MiniBox.Domain/
COPY src/Core/MiniBox.Application/MiniBox.Application.csproj src/Core/MiniBox.Application/
COPY src/Infrastructure/MiniBox.Infrastructure/MiniBox.Infrastructure.csproj src/Infrastructure/MiniBox.Infrastructure/
COPY src/Presentation/MiniBox.SimulationEngine/MiniBox.SimulationEngine.csproj src/Presentation/MiniBox.SimulationEngine/
RUN dotnet restore src/Presentation/MiniBox.SimulationEngine/MiniBox.SimulationEngine.csproj
COPY src/ ./src/
RUN dotnet publish src/Presentation/MiniBox.SimulationEngine/MiniBox.SimulationEngine.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniBox.SimulationEngine.dll"]
