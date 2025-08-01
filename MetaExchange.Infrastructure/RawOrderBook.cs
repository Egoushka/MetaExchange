using System.Text.Json.Serialization;

namespace MetaExchange.Infrastructure;

public class RawOrderBook
{
    [JsonPropertyName("AcqTime")]
    public DateTime AcquisitionTime { get; set; }

    [JsonPropertyName("Bids")]
    public List<RawOrderWrapper> Bids { get; set; }

    [JsonPropertyName("Asks")]
    public List<RawOrderWrapper> Asks { get; set; }
}