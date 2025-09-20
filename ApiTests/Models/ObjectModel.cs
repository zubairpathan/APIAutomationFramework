using System.Text.Json.Serialization;

namespace ApiTests.Models
{
    public class AddObjectResponseModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("data")]
        public DataModel? Data { get; set; }

        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
    }
    public class PartialUpdateObjectResponseModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("data")]
        public DataModel? Data { get; set; }
    }

    public class DataModel
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("CPU model")]
        public string? CpuModel { get; set; }

        [JsonPropertyName("Hard disk size")]
        public string? HardDiskSize { get; set; }
    }
}
