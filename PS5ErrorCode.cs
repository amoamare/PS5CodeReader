using System.Text.Json.Serialization;

namespace PS5CodeReader
{
    public class PlayStationErrorCode
    {
        [JsonPropertyName("ID")]
        public required string ID { get; set; }

        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        [JsonPropertyName("Status")]
        public PlayStationErrorCodeStatus Status { get; set; }

        [JsonPropertyName("Priority")]
        public int Priority { get; set; }
    }

    public enum PlayStationErrorCodeStatus
    {
        [JsonPropertyName("Unknown")]
        Unknown,
        [JsonPropertyName("Unconfirmed")]
        Unconfirmed,
        [JsonPropertyName("User Submitted")]
        UserSubmitted,
        [JsonPropertyName("Confirmed")]
        Confirmed
    }
}
