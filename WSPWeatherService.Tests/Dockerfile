FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WSPWeatherService/WSPWeatherService.csproj", "WSPWeatherService/"]
COPY ["WSPWeatherService.Tests/WSPWeatherService.Tests.csproj", "WSPWeatherService.Tests/"]
RUN dotnet restore "WSPWeatherService/WSPWeatherService.csproj"
RUN dotnet restore "WSPWeatherService.Tests/WSPWeatherService.Tests.csproj"
COPY . .
WORKDIR "/src/WSPWeatherService.Tests"
ENTRYPOINT ["dotnet", "test", "--logger:trx;LogFileName=testresults.trx"]
