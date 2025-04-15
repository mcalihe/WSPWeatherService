## WSPWeatherService

This C#/.NET 9 microservice fetches weather data from the Zürich Water Police (Wasserschutzpolizei) for the stations "
Tiefenbrunnen" and "Mythenquai". It stores validated measurements in a relational database and exposes a REST/JSON API
for querying historical data.

### Features

- Fetches and persists weather data from the previous day
- Filters out invalid or incomplete entries
- Avoids duplicate records
- Provides a REST API to:
    - Retrieve max, min, and average values per measurement type
    - Count stored entries
    - List all stored measurements
- Filtering options:
    - **Required**: Time range
    - **Optional**: Station
- Dependencies
    - [Hangfire](https://www.hangfire.io/)
    - [Microsoft.Data.SqlClient](https://www.nuget.org/packages/microsoft.data.sqlclient)
    - [Microsoft.EntityFrameworkCore.Design](https://www.nuget.org/packages/microsoft.entityframeworkcore.design/)
    - [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.sqlserver/)
    - [NSwag.AspNetCore](https://github.com/RicoSuter/NSwag)
    - [Riok.Mapperly](https://mapperly.riok.app/)

## 🛠 Makefile Commands

To simplify building, running, and testing the project, the following Makefile commands are available:

| Command      | Description                                                                                         |
|--------------|-----------------------------------------------------------------------------------------------------|
| `make build` | Builds all Docker containers **from scratch** (no cache). Useful for stale dependencies.            |
| `make start` | Starts the web application (`wspweatherservice`) and the database (`sqlserver`) in detached mode.   |
| `make test`  | Runs all **unit tests** inside the `wspweatherservice.tests` container.                             |
| `make down`  | Stops and removes all running containers.                                                           |
| `make clean` | Completely removes all containers, volumes, and images created by the project. ⚠️ Use with caution. |

### Example usage

```bash
make build       # Rebuild all services
```
```bash
make start       # Start app and database
```
```bash
make test        # Run unit tests
```
```bash
make down        # Stop and remove containers
```
```bash
make clean       # Remove everything (containers, volumes, images)
```

## C# client generation for Tecdottir API

To generate a client for the [Tecdottir Weather API](https://tecdottir.metaodi.ch/docs/) (based on
the [OAS json](https://tecdottir.metaodi.ch/swagger)):

1. First install the nswag CLI tool => `dotnet tool install --global NSwag.ConsoleCore`
2. Use the CLI to generate the client as defined in `nswag.json` =>  `nswag run nswag.json`

CAREFUL: There is an issue with the OAS from the api: [Github Issue](https://github.com/metaodi/tecdottir/issues/53)

So the datatype needed to be fixed manually after generating while this issue is not fixed!

## Further steps

These would be the further steps to take if this was a real project:

- Rate-limiting or API key for public access
- Limiting the `GetAllMeasurements` endpoint so it is limited in the date-range (e.g. max 6 months) / limited in the
  max-rows (e.g. 5000)
- Integration tests (e.g. for the endpoints)
- Maybe use `MediatR` in the future if the project grows
- Add pagination support to the `GetAllMeasurements`
- Provide a generated `C#`/`Typescript`/`Angular` client via `npm`/`NuGet`