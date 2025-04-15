# 🌦 WSPWeatherService

This C#/.NET 9 microservice fetches weather data from the Zürich Water Police (*Wasserschutzpolizei*) for the stations
**Tiefenbrunnen** and **Mythenquai**. It stores validated measurements in a relational database and exposes a RESTful
JSON API for querying historical data.

---

## Prerequisites

- **Docker & Docker Compose**
- **.NET SDK 9.0**
- Optional: `make`

---

## ✨ Features

- ✅ Fetches and persists weather data from the **previous day**
- ✅ Filters out **invalid or incomplete** entries
- ✅ Avoids **duplicate records**
- ✅ Provides a REST API to:
    - Retrieve **max**, **min**, and **average** values per measurement type
    - Count stored entries per measurement type
    - List all stored measurements per measurement type
- ✅ Filtering options:
    - **Required**: Time range (`start`, `end`)
    - **Optional**: Station (`station` query param)

---

## 🚀 Getting Started

To run the project locally using Docker:

```bash
make build     # Build all containers (API, DB, Tests)
```

```bash
make start     # Start API and database
```

```bash
make test      # Run all unit tests
```

```bash
make down      # Stop containers
```

Make sure `Docker` and `make` is installed and running on your system.

***Getting Started without `make`***

App:

```bash
docker compose build --no-cache && docker compose up -d wspweatherservice sqlserver
```

*Tests*:

```bash
docker compose run --rm wspweatherservice.tests
```

Once running, the following services are available:

| Name                                                                      | Link                                          | Description                   |
|---------------------------------------------------------------------------|-----------------------------------------------|-------------------------------|
| [Swagger UI](http://localhost:8080/swagger)                               | http://localhost:8080/swagger                 | Interactive API documentation |
| [OAS Definition (OpenAPI)](http://localhost:8080/swagger/v1/swagger.json) | http://localhost:8080/swagger/v1/swagger.json | Raw OpenAPI JSON schema       |
| [Hangfire Dashboard](http://localhost:8080/hangfire)                      | http://localhost:8080/hangfire                | Background job monitoring     |

---

## ⚙️ How it works

- A background job is scheduled via **Hangfire** to run daily at **00:30**, fetching weather data for the previous
  day. (The job can also be runned manually using the [Hangfire Dashboard](http://localhost:8080/hangfire))
- Additionally, the fetch job runs **once on application startup** to ensure fresh data is available even before the
  first scheduled run.
- The fetched and validated data is stored in a SQL Server database and made accessible via a RESTful API.
- The API is documented and available to consumers via [Swagger UI](http://localhost:8080/swagger)
  and [OAS Definition (OpenAPI)](http://localhost:8080/swagger/v1/swagger.json).

---

## 🧩 Dependencies

- [Hangfire](https://www.hangfire.io/)
- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/microsoft.data.sqlclient)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.sqlserver/)
- [Microsoft.EntityFrameworkCore.Design](https://www.nuget.org/packages/microsoft.entityframeworkcore.design/)
- [NSwag.AspNetCore](https://github.com/RicoSuter/NSwag)
- [Riok.Mapperly](https://mapperly.riok.app/)

---

## 🛠 Makefile Commands

The project includes a `Makefile` to simplify building, running, and testing.

| Command      | Description                                                                                   |
|--------------|-----------------------------------------------------------------------------------------------|
| `make build` | Builds all Docker containers **from scratch** (no cache). Useful for stale dependencies.      |
| `make start` | Starts the web application (`wspweatherservice`) and database (`sqlserver`) in detached mode. |
| `make test`  | Runs all **unit tests** inside the `wspweatherservice.tests` container.                       |
| `make down`  | Stops and removes all running containers.                                                     |
| `make clean` | Removes all containers, volumes, and images created by the project. ⚠️ Use with caution.      |

### 💡 Example usage

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

---

## 🔧 Generating the C# Client for the Tecdottir API

To generate a C# client for the [Tecdottir Weather API](https://tecdottir.metaodi.ch/docs/):

1. Install the NSwag CLI:
   ```bash
   dotnet tool install --global NSwag.ConsoleCore
   ```

2. Run the generator:
   ```bash
   nswag run nswag.json
   ```

⚠️ **Important:**  
There is a known issue with the API's OpenAPI schema —
see [GitHub Issue #53](https://github.com/metaodi/tecdottir/issues/53).  
You'll need to **manually fix the datatype** in the generated code until it's resolved upstream.

---

## 📌 Potential Next Steps

If this were a real production system, here are some ideas for further improvement:

- 🔒 Add **rate-limiting** or **API key** support for public access
- 📉 Limit `GetAllMeasurements`:
    - By **date range** (e.g. max 6 months)
    - By **row count** (e.g. max 5000 entries)
- 🧪 Add **integration tests** (e.g. for endpoints)
- 🧭 Consider **MediatR** for better architecture as the project scales
- 📦 Add **pagination support** to `GetAllMeasurements`
- 📤 Provide generated clients (`C#`, `TypeScript`, `Angular`) via **NuGet** / **npm**
