using MetaExchange.Core;
using MetaExchange.Console.UI;
using MetaExchange.Domain;
using MetaExchange.Infrastructure;

namespace MetaExchange.Console;

/// <summary>
/// Manages the entire application lifecycle, including data loading and user interaction.
/// </summary>
public class ConsoleRunner
{
    private readonly PlanPresenter _presenter = new();
    private readonly CommandParser _parser = new();
    private readonly OrderExecutor _executor = new();

    private List<Exchange> _exchanges = [];
    private const string OrderBookFilePath = "order_books_data.json";

    /// <summary>
    /// The main entry point for the console application. It loads data and starts the command loop.
    /// </summary>
    public void Run()
    {
        if (!TryLoadExchanges())
        {
            return;
        }
        
        _presenter.DisplayWelcomeMessage(_exchanges.Count);

        while (true)
        {
            _presenter.DisplayPrompt();
            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {
                var executionInput = _parser.Parse(input);
                System.Console.WriteLine($"\nAttempting to {executionInput.Type} {executionInput.Amount} BTC...");
                
                var plan = _executor.GetBestExecutionPlan(_exchanges, executionInput.Type, executionInput.Amount);
                
                _presenter.DisplayExecutionPlan(plan);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is InvalidOperationException)
            {
                _presenter.DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                _presenter.DisplayError($"An unexpected error occurred: {ex.Message}");
            }
        }
        System.Console.WriteLine("Exiting application. Goodbye!");
    }
    
    /// <summary>
    /// Attempts to load exchange data from the file system.
    /// </summary>
    /// <returns>True if loading was successful, otherwise false.</returns>
    private bool TryLoadExchanges()
    {
        try
        {
            System.Console.WriteLine("Loading order books from exchanges...");
            _exchanges = OrderBookLoader.Load(OrderBookFilePath);
            return true;
        }
        catch (FileNotFoundException)
        {
            _presenter.DisplayError($"Error: The data file was not found. Ensure '{OrderBookFilePath}' is in the project's output directory.");
            return false;
        }
        catch (Exception ex)
        {
            _presenter.DisplayError($"An unexpected error occurred during data load: {ex.Message}");
            return false;
        }
    }
}