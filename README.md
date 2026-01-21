# Budget Manager API

## Overview

The **Budget Manager API** is a .NET 8 Web API designed to manage personal finances. It provides endpoints for tracking transactions, managing accounts, categorization, planning expenses, and handling monthly recurring items. The service is built with a Hexagonal Architecture (Ports and Adapters) approach, utilizing Azure Cosmos DB for data storage and Auth0 for secure authentication.

## Prerequisites

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **[Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator)** (for local development)
- **[Docker](https://www.docker.com/)** (optional, for containerized execution)

## Setup & Configuration

### 1. Cosmos DB Setup

This application requires an Azure Cosmos DB instance (or the local Emulator).

**Database Name:** `BudgetManager`

**Containers:**
You need to create the following containers. All containers use the same partition key.

| Container Name        | Partition Key | Description |
|-----------------------|---------------|-------------|
| `Transactions`        | `/userId`     | Stores individual income/expense records. |
| `Accounts`            | `/userId`     | Stores user bank/cash accounts. |
| `Categories`          | `/userId`     | Stores transaction categories. |
| `PlannedItems`        | `/userId`     | Stores planned expenses/budgets. |
| `MonthlyTransactions` | `/userId`     | Stores recurring monthly items. |
| `Users`               | `/userId`     | Stores user preferences and metadata. |

> **Note:** The local development configuration in `appsettings.json` points to the default Cosmos DB Emulator connection string:
> `AccountEndpoint=https://localhost:8081/;AccountKey=...`

### 2. Environment Variables / AppSettings

Configure the `appsettings.json` (or `appsettings.Development.json`) file in `BudgetManager.Service.Api`.

#### Database Settings
```json
"CosmosDb": {
  "ConnectionString": "YourConnectionString",
  "DatabaseName": "BudgetManager",
  "TransactionsContainer": "Transactions",
  "AccountsContainer": "Accounts",
  "CategoriesContainer": "Categories",
  "PlannedItemsContainer": "PlannedItems",
  "MonthlyTransactionsContainer": "MonthlyTransactions",
  "UsersContainer": "Users"
}
```

#### Authentication (Auth0)
You must configure Auth0 to secure the API.

```json
"Auth0": {
  "Domain": "your-auth0-domain.us.auth0.com",
  "Audience": "your-api-audience"
}
```
*Alternatively, you can set `AUTH0_DOMAIN` and `AUTH0_AUDIENCE` as environment variables.*

## Running the Application

### Option 1: Using .NET CLI

1. Navigate to the API project directory:
   ```bash
   cd BudgetManager.Service.Api
   ```
2. Run the application:
   ```bash
   dotnet run
   ```
   The API will start on `http://localhost:5023`

### Option 2: Using Docker Compose

A `compose.yaml` file is provided at the solution root.

1. Ensure the Cosmos DB Emulator is running locally.
2. Run:
   ```bash
   docker-compose up
   ```
   The service will run on port `8080`.

## API Documentation

The application uses **Swagger (Swashbuckle)** for API documentation.

- Access the Swagger UI at: `https://localhost:<port>/swagger` (e.g., `https://localhost:7196/swagger`).
- This interface allows you to explore and test endpoints directly.

## Project Structure

- **BudgetManager.Service.Api**: The main entry point, containing Controllers, Configuration, and Dependency Injection setup.
- **BudgetManager.Api.Domain**: Contains the core domain entities (`Transaction`, `Account`, etc.) and interfaces.
- **Infrastructure**: Contains the implementation of repositories and Cosmos DB integration.
