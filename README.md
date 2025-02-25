# Wallet Service Application

## Description
This application provides a wallet service that allows you to:
- Create wallets.
- Retrieve wallet balances with optional currency conversion.
- Adjust wallet balances using various strategies (Add Funds, Subtract Funds, or Force Subtract Funds).

Additionally, a background service fetches the latest exchange rates from the European Central Bank (ECB) every minute and updates a SQL Server database.

## Requirements
- .NET 8.0 SDK
- SQL Server LocalDB (or another SQL Server instance)
- An IDE or command-line interface for .NET (e.g., Visual Studio, Rider, or VS Code)

## Setup

### 1. Clone the Repository
Clone the repository to your local machine:
```sh
git clone https://github.com/SteliosBotonakis/novicode
```

### 2. Update the Database Connection String
Update the connection string in the `appsettings.json` file to point to your SQL Server instance:
```json
{
  "ConnectionStrings": {
    "MyDb": "Server=(localdb)\\mssqllocaldb;Database=NoviCodeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```
Verify that your LocalDB instance is running:
```sh
sqllocaldb start MSSQLLocalDB
```

### 3. Build the Project
```sh
dotnet build
```

### 4. Run the Project
```sh
dotnet run --project NoviCode.WalletService
```

### 5. Test the API
Open your browser and navigate to `http://localhost:5044/swagger/index.html` to test the API using Swagger.
