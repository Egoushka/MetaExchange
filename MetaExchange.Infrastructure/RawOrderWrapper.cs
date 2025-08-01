using System.Text.Json.Serialization;

namespace MetaExchange.Infrastructure;

public class RawOrderWrapper
{
    [JsonPropertyName("Order")]
    public RawOrderDetails Order { get; set; }
}