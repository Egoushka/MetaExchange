using MetaExchange.Domain;

namespace MetaExchange.Console.UI;

/// <summary>
/// Handles all console output for displaying information to the user.
/// </summary>
public class PlanPresenter
{
    public void DisplayWelcomeMessage(int exchangeCount)
    {
        System.Console.WriteLine($"Successfully loaded data for {exchangeCount} exchanges.\n");
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Welcome to the MetaExchange Console.");
        System.Console.ResetColor();
        System.Console.WriteLine("Enter a command in the format '[buy|sell] [amount]' (e.g., 'buy 10,5').");
        System.Console.WriteLine("Type 'exit' to quit the application.\n");
    }

    public void DisplayExecutionPlan(List<ExecutionOrder> plan)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("\n--- Optimal Execution Plan Found ---");
        System.Console.ResetColor();

        foreach (var order in plan)
        {
            System.Console.WriteLine($"  - {order.Type} {order.BtcAmount:F8} BTC on {order.ExchangeName} at {order.PricePerBtc} EUR per BTC");
        }

        DisplaySummary(plan);
    }

    public void DisplayError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"{message}\n");
        System.Console.ResetColor();
    }
    
    public void DisplayPrompt()
    {
        System.Console.Write("> ");
    }

    private void DisplaySummary(List<ExecutionOrder> plan)
    {
        if (!plan.Any()) return;
        
        var orderType = plan.First().Type;
        decimal totalBtcExecuted = plan.Sum(o => o.BtcAmount);
        decimal totalEurCost = plan.Sum(o => o.BtcAmount * o.PricePerBtc);
        decimal effectivePrice = totalBtcExecuted > 0 ? totalEurCost / totalBtcExecuted : 0;

        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("\n--- Summary ---");
        System.Console.ResetColor();
        System.Console.WriteLine($"Total BTC to {orderType}: {totalBtcExecuted:F8}");
        System.Console.WriteLine($"{(orderType == OrderType.Buy ? "Total Cost" : "Total Revenue")}: {totalEurCost} EUR");
        System.Console.WriteLine($"Effective Price: {effectivePrice} EUR per BTC\n");
    }
}