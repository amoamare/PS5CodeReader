using System.Text.Json.Serialization;

namespace PS5CodeReader
{
    public class PS5ErrorCode
    {
        [JsonPropertyName("ID")]
        public required string ID { get; set; }

        [JsonPropertyName("Message")]
        public string? Message { get; set; }
    }
}
