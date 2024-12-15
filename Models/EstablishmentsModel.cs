using Azure;
using Azure.Data.Tables;

namespace FoodInspectorAPI.Models
{
    // Storage Table model to uniquely identify food establishments by name (ProgramIdentifier) and city
    public class EstablishmentsModel : ITableEntity
    {
        public string? PartitionKey { get; set; }

        public string? RowKey { get; set; }

        // The unique business name by which to query
        public string? ProgramIdentifier { get; set; }

        // The name of the food establishment
        public string? Name { get; set; }

        // The city in which the establishment is located
        public string? City { get; set; }

        public DateTimeOffset? Timestamp { get; set; } = default;

        public ETag ETag { get; set; } = default;
    }
}
