using MetaExchange.Domain;

namespace MetaExchange.Core;

public class ExchangeState
{
    public decimal Balance { get; set; }
    private readonly List<OrderBookEntry> _offers;
    private int _currentIndex;

    public bool IsBuyMode { get; }

    public ExchangeState(decimal initialBalance, List<OrderBookEntry> offers, bool isBuyMode = false)
    {
        Balance = initialBalance;
        _offers = offers;
        _currentIndex = 0;
        IsBuyMode = isBuyMode;
    }

    public void MoveToNextOffer() => _currentIndex++;
    public bool HasMoreOffers() => _currentIndex < _offers.Count;
    public OrderBookEntry GetCurrentOffer() => _offers[_currentIndex];
}