using MetaExchange.Domain;
using MetaExchange.Infrastructure;

namespace MetaExchange.Api.Services;

public class MetaExchangeService
{
    private readonly List<Exchange> _exchanges;

    public MetaExchangeService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("MetaExchangeSettings");
        var filePath = settings.GetValue<string>("OrderBooksFilePath");

        if (string.IsNullOrEmpty(filePath))
        {
            throw new InvalidOperationException("OrderBooksFilePath is not configured in appsettings.json.");
        }

        _exchanges = OrderBookLoader.Load(filePath);
    }

    public List<Exchange> GetExchanges()
    {
        return _exchanges;
    }
}