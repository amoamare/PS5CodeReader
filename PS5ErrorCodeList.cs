using System.Text.Json.Serialization;

namespace PS5CodeReader
{
    public class PS5ErrorCodeList
    {
        [JsonPropertyName("Revision")]
        public required Version Revision { get; set; }

        [JsonPropertyName("Description")]
        public required string Description { get; set; }

        [JsonPropertyName("ErrorCodes")]
        public required List<PS5ErrorCode> ErrorCodes { get; set; }
    }

}
