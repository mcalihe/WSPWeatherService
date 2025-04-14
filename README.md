## ZRHWeatherService

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