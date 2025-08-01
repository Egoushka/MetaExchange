using MetaExchange.Domain;

namespace MetaExchange.Console.UI;

/// <summary>
/// Parses raw user input strings into structured request objects.
/// </summary>
public class CommandParser
{
    public ExecutionInput Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be empty.");
        }

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            throw new FormatException("Invalid command format. Please use: [buy|sell] [amount]");
        }

        if (!Enum.TryParse<OrderType>(parts[0], true, out var orderType))
        {
            throw new FormatException($"Error: Invalid order type '{parts[0]}'. Please use 'buy' or 'sell'.");
        }

        if (!decimal.TryParse(parts[1], out decimal btcAmount) || btcAmount <= 0)
        {
            throw new FormatException($"Error: Invalid amount '{parts[1]}'. Please provide a positive number.");
        }

        return new ExecutionInput(orderType, btcAmount);
    }
}