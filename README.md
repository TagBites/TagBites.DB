# TagBites.DB

[![Nuget](https://img.shields.io/nuget/v/TagBites.DB.svg)](https://www.nuget.org/packages/TagBites.DB/)
[![License](http://img.shields.io/github/license/TagBites/TagBites.DB)](https://github.com/TagBites/TagBites.DB/blob/master/LICENSE)

TagBites.DB is a library that simplifies query execution in .NET applications. It efficiently manages database connections, executes queries, and handles transactions. The library supports multiple database providers, including PostgreSQL and SQLite.

> This project is not recommended for general use as there are better solutions available.
This library was specifically designed for a very complex ERP system where a single connection and transaction pass through multiple libraries and functions.
It allows tracking the start and end of connections and transactions, and hooking into various events such as connection opened, connection closed, transaction beginning, transaction committed, and more.

## Features

- **Database Abstraction**: Provides a unified API for interacting with different database systems.
- **Connection Management**: Efficiently manages database connections and transactions.
- **Cursor Management** (PostgreSQL): Supports cursor-based data retrieval for large datasets.
- **Extensible**: Easily extendable to support additional database providers.

## Supported Database Providers

- PostgreSQL (supports all API features)
- SQLite
- SqlServer

## Installation

You can install the TagBites.DB packages via NuGet:
```
dotnet add package TagBites.DB
dotnet add package TagBites.DB.Postgres
dotnet add package TagBites.DB.Sqlite
dotnet add package TagBites.DB.SqlServer
```
    
## Usage

- **Initialize the Database Provider**
```csharp
var provider = new PgSqlLinkProvider("your_connection_string");
```

- **Execute Queries**
```csharp
using (var link = provider.CreateLink())
{
    var result = link.Execute(SELECT * FROM your_table");
    
    foreach (var row in result)
    {
        // ...
    }
}
```

- **Execute Scalar**
```csharp
using (var link = provider.CreateLink())
{
    var value = link.ExecuteScalar<int>("SELECT 1");
}

```

- **Batch execute**
```csharp
using (var link = provider.CreateLink())
{
    var results = link.BatchExecute("SELECT 1; SELECT 2, 3");

    foreach (var result in results)
        foreach (var row in result)
        {
            // ...
        }    
}

```

- **Delay Batch Execute**
```csharp
using (var link = provider.CreateLink())
{
    var r1 = link.DelayedBatchExecute("SELECT 1");
    // ...
    var r2 = link.DelayedBatchExecute("SELECT 2");
    // ..

    // First call to Result executes all delay queries at once.
    foreach (var result in r1.Result)
    {
        // ...
    }
}

```
