# MetaExchange

MetaExchange is a service designed to find the optimal execution plan for buying or selling a specified amount of Bitcoin (BTC) for Euro (EUR) across multiple cryptocurrency exchanges. It aggregates order book data and available balances from various exchanges to calculate the most cost-effective way to fulfill an order, minimizing costs for buyers and maximizing revenue for sellers.

The project is available as both a REST API and an interactive command-line application.

## Features

-   **Optimal Order Routing**: Calculates the best execution plan by selecting the cheapest offers for buys and the most expensive bids for sells across all available exchanges.
-   **Dual Interfaces**:
    -   A **REST API** for programmatic integration.
    -   An **interactive Console UI** for manual queries.

## Project Structure

The solution is divided into several projects, each with a distinct responsibility:

-   `MetaExchange.Api`: An ASP.NET Core project that exposes the functionality via a RESTful API.
-   `MetaExchange.Console`: A .NET console application that provides an interactive command-line interface.
-   `MetaExchange.Core`: The core project containing the main business logic.
-   `MetaExchange.Domain`: Contains the core domain models and entities.
-   `MetaExchange.Infrastructure`: Responsible for data-access concerns, primarily loading the order book data from a file.
-   `MetaExchange.Tests`: A xUnit project containing unit tests for the core business logic, ensuring its correctness and reliability.

## Prerequisites

-   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Configuration

The application requires a JSON file containing the order book data and balances for each exchange.

  **File Location**:
    -   For the **API**, the path is configured in `MetaExchange.Api/appsettings.Development.json`. Make sure the file is placed where the path indicates.
      ```json
      "MetaExchangeSettings": {
        "OrderBooksFilePath": "order_books_data.json"
      }
      ```
    -   For the **Console App**, place the `order_books_data.json` file in the project's output directory (e.g., `MetaExchange.Console/bin/Debug/net8.0/`).

## How to Run

### Running the API

1.  Navigate to the API project directory:
```bash
cd MetaExchange/MetaExchange.Api
```
2.  Run the application:
```bash
dotnet run
```
3.  The API will be available at `http://localhost:5230`. You can access the interactive Swagger UI documentation at `http://localhost:5230/swagger`.

### Running the Console App

1.  Make sure `order_books_data.json` is correctly placed in the output directory.
2.  Navigate to the console project directory:
```bash
cd MetaExchange/MetaExchange.Console
```
3.  Run the application:
 ```bash
 dotnet run
 ```

## How to Use

### Console Usage
After starting the console app, enter commands in the format [buy|sell] [amount].

Example Session:
```bash
Loading order books from exchanges...
Successfully loaded data for 3097 exchanges.

Welcome to the MetaExchange Console.
Enter a command in the format '[buy|sell] [amount]' (e.g., 'buy 10,5').
Type 'exit' to quit the application.

> buy 10,5

Attempting to Buy 10,5 BTC...

--- Optimal Execution Plan Found ---
  - Buy 1,18438000 BTC on Exchange3012 at 2955,03 EUR per BTC
  - Buy 0,40600000 BTC on Exchange2943 at 2957,96 EUR per BTC
  - Buy 0,40600000 BTC on Exchange2944 at 2957,96 EUR per BTC
  - Buy 0,17302283 BTC on Exchange2786 at 2958,0 EUR per BTC
...
--- Summary ---
Total BTC to Buy: 10,50000000
Total Cost: 31 056,89 EUR
Effective Price: 2957,7986501315428571428571429 EUR per BTC

> sell 10,6

Attempting to Sell 10,6 BTC...

--- Optimal Execution Plan Found ---
  - Sell 0,01000000 BTC on Exchange204 at 2966,95 EUR per BTC
  - Sell 0,01000000 BTC on Exchange205 at 2966,95 EUR per BTC
  - Sell 0,01000000 BTC on Exchange206 at 2966,95 EUR per BTC
...
--- Summary ---
Total BTC to Sell: 10,60000000
Total Revenue: 31 439,81 EUR
Effective Price: 2966,0198625815849056603773585 EUR per BTC

> exit
Exiting application. Goodbye!

```

## How to Run Tests
To ensure the core logic is working correctly, you can run the suite of unit tests.

1. Navigate to the solution's root directory or the tests project directory:
```bash
cd /path/to/MetaExchange/
```

2. Run the tests using the .NET CLI:
```bash
dotnet test
```
