using MetaExchange.Core;
using MetaExchange.Domain;

namespace MetaExchange.Tests;

public class OrderExecutorTests
{
    private readonly IOrderExecutor _executor = new OrderExecutor();

    #region Buy Order Tests

    [Fact]
    public void GetBestExecutionPlan_SimpleBuy_SucceedsWithCheapestOffer()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("CheapExchange", new OrderBook(
                Asks: [new(3000m, 10m)],
                Bids: []), 100_000, 10),
            new("ExpensiveExchange", new OrderBook(
                Asks: [new(3100m, 10m)],
                Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 5m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.NotNull(plan);
        var order = Assert.Single(plan);
        Assert.Equal("CheapExchange", order.ExchangeName);
        Assert.Equal(5m, order.BtcAmount);
        Assert.Equal(3000m, order.PricePerBtc);
    }

    [Fact]
    public void GetBestExecutionPlan_BuyOrderSpanningMultipleExchanges_SucceedsInCorrectOrder()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("CheapExchange", new OrderBook(
                Asks: [new(3000m, 5m)], // Only 5 BTC available
                Bids: []), 100_000, 10),
            new("ExpensiveExchange", new OrderBook(
                Asks: [new(3100m, 10m)],
                Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 8m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(2, plan.Count);
        Assert.Collection(plan,
            order1 =>
            {
                Assert.Equal("CheapExchange", order1.ExchangeName);
                Assert.Equal(5m, order1.BtcAmount);
                Assert.Equal(3000m, order1.PricePerBtc);
            },
            order2 =>
            {
                Assert.Equal("ExpensiveExchange", order2.ExchangeName);
                Assert.Equal(3m, order2.BtcAmount);
                Assert.Equal(3100m, order2.PricePerBtc);
            }
        );
    }

    [Fact]
    public void GetBestExecutionPlan_BuyOrderSpanningMultipleOffersOnSameExchange_Succeeds()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("SingleExchange", new OrderBook(
                Asks:
                [
                    new(3000m, 5m), // First offer
                    new(3010m, 10m)
                ],
                Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 8m); // Need 8 BTC

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(2, plan.Count);
        Assert.Collection(plan,
            order1 =>
            {
                Assert.Equal("SingleExchange", order1.ExchangeName);
                Assert.Equal(5m, order1.BtcAmount);
                Assert.Equal(3000m, order1.PricePerBtc);
            },
            order2 =>
            {
                Assert.Equal("SingleExchange", order2.ExchangeName);
                Assert.Equal(3m, order2.BtcAmount);
                Assert.Equal(3010m, order2.PricePerBtc);
            }
        );
    }

    [Fact]
    public void GetBestExecutionPlan_BuyConstrainedByEurBalance_SkipsOfferAndSucceeds()
    {
        // Arrange: The cheapest exchange doesn't have enough EUR to fulfill the order.
        var exchanges = new List<Exchange>
        {
            new("PoorExchange", new OrderBook(
                Asks: [new(3000m, 10m)], // Very cheap offer
                Bids: []), 5000, 10), // But only 5000 EUR balance (can only buy 1.66 BTC)
            new("RichExchange", new OrderBook(
                Asks: [new(3100m, 10m)],
                Bids: []), 100_000, 10) // More expensive, but has enough EUR
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 2m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(2, plan.Count);
        Assert.Collection(plan,
            order1 =>
            {
                Assert.Equal("PoorExchange", order1.ExchangeName);
                Assert.Equal(5000m / 3000m, order1.BtcAmount); // Takes the max it can afford
            },
            order2 => { Assert.Equal("RichExchange", order2.ExchangeName); }
        );
    }

    [Fact]
    public void GetBestExecutionPlan_InsufficientTotalLiquidityForBuy_ReturnsFailureResult()
    {
        // Arrange: Total available BTC to buy is 5, but we need 10.
        var exchanges = new List<Exchange>
        {
            new("ExchangeA", new OrderBook(Asks: [new(3000m, 5m)], Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 10m);
        
        // Assert
        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.Contains("Not enough liquidity or balance", error.Message);
    }

    #endregion

    #region Sell Order Tests

    [Fact]
    public void GetBestExecutionPlan_SimpleSell_SucceedsWithHighestOffer()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("LowballExchange", new OrderBook(
                    Asks: [],
                    Bids: [new(2900m, 10m)]),
                100_000, 10),
            new("TopBidExchange", new OrderBook(
                    Asks: [],
                    Bids: [new(3000m, 10m)]),
                100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Sell, 5m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.NotNull(plan);
        var order = Assert.Single(plan);
        Assert.Equal("TopBidExchange", order.ExchangeName);
        Assert.Equal(5m, order.BtcAmount);
        Assert.Equal(3000m, order.PricePerBtc);
    }

    [Fact]
    public void GetBestExecutionPlan_SellOrderSpanningMultipleExchanges_SucceedsInCorrectOrder()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("LowballExchange", new OrderBook(Asks: [], Bids: [new(2900m, 10m)]), 100_000, 10),
            // Only wants 5 BTC
            new("TopBidExchange", new OrderBook(Asks: [], Bids: [new(3000m, 5m)]), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Sell, 8m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(2, plan.Count);
        Assert.Collection(plan,
            order1 =>
            {
                Assert.Equal("TopBidExchange", order1.ExchangeName);
                Assert.Equal(5m, order1.BtcAmount);
                Assert.Equal(3000m, order1.PricePerBtc);
            },
            order2 =>
            {
                Assert.Equal("LowballExchange", order2.ExchangeName);
                Assert.Equal(3m, order2.BtcAmount);
                Assert.Equal(2900m, order2.PricePerBtc);
            }
        );
    }

    [Fact]
    public void GetBestExecutionPlan_SellConstrainedByBtcBalance_LimitsSaleAndSucceeds()
    {
        // Arrange: The exchange with the best bid only has 2 BTC in its own wallet to sell.
        var exchanges = new List<Exchange>
        {
            new("LowballExchange", new OrderBook(Asks: [], Bids: [new(2900m, 10m)]), 100_000, 10),
            new("LimitedBtcExchange", new OrderBook(Asks: [], Bids: [new(3000m, 10m)]), 100_000,
                2)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Sell, 5m); // We want to sell 5

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.Equal(2, plan.Count);
        Assert.Collection(plan,
            order1 =>
            {
                Assert.Equal("LimitedBtcExchange", order1.ExchangeName);
                Assert.Equal(2m, order1.BtcAmount); // Sells the max it has
            },
            order2 =>
            {
                Assert.Equal("LowballExchange", order2.ExchangeName);
                Assert.Equal(3m, order2.BtcAmount); // Sells the rest
            }
        );
    }

    [Fact]
    public void GetBestExecutionPlan_InsufficientTotalLiquidityForSell_ReturnsFailureResult()
    {
        // Arrange: Total market demand is 5 BTC, but we want to sell 10.
        var exchanges = new List<Exchange>
        {
            new("ExchangeA", new OrderBook(Asks: [], Bids: [new(3000m, 5m)]), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Sell, 10m);

        // Assert
        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.Contains("Not enough liquidity or balance", error.Message);
    }

    #endregion

    #region General Edge Cases

    [Fact]
    public void GetBestExecutionPlan_RequestForZeroBtc_ReturnsEmptyPlan()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("ExchangeA", new OrderBook(Asks: [new(3000m, 5m)], Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 0m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        Assert.NotNull(plan);
        Assert.Empty(plan);
    }

    [Fact]
    public void GetBestExecutionPlan_OneExchangeHasNoValidOffers_SucceedsWithOtherExchange()
    {
        // Arrange
        var exchanges = new List<Exchange>
        {
            new("EmptyExchange", new OrderBook(Asks: [], Bids: []), 100_000, 10),
            new("WorkingExchange", new OrderBook(Asks: [new(3000m, 10m)], Bids: []), 100_000, 10)
        };

        // Act
        var result = _executor.GetBestExecutionPlan(exchanges, OrderType.Buy, 5m);

        // Assert
        Assert.True(result.IsSuccess);
        var plan = result.Value;
        var order = Assert.Single(plan);
        Assert.Equal("WorkingExchange", order.ExchangeName);
    }

    #endregion
}