using MetaExchange.Domain;
using MetaExchange.Infrastructure;

namespace MetaExchange.Api.Services;

public class MetaExchangeService
{
    private readonly List<Exchange> _exchanges;

    public MetaExchangeService()
    {
        _exchanges = OrderBookLoader.Load("order_books_data.json");
    }

    public List<Exchange> GetExchanges()
    {
        return _exchanges;
    }
}