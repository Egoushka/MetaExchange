using System.Text.Json.Serialization;

namespace MetaExchange.Infrastructure;

public class RawOrderDetails
{
    [JsonPropertyName("Price")]
    public decimal Price { get; set; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }
}