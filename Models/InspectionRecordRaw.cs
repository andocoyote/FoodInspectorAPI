using System.Text.Json.Serialization;

namespace FoodInspectorAPI.Models
{
    public class InspectionRecordRaw
    {
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("program_identifier")]
        public string? ProgramIdentifier { get; set; }

        [JsonPropertyName("inspection_date")]
        public DateTime InspectionDate { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("zip_code")]
        public string? ZipCode { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("inspection_business_name")]
        public string? InspectionBusinessName { get; set; }

        [JsonPropertyName("inspection_type")]
        public string? InspectionType { get; set; }

        [JsonPropertyName("inspection_score")]
        public string? InspectionScore { get; set; }

        [JsonPropertyName("inspection_result")]
        public string? InspectionResult { get; set; }

        [JsonPropertyName("inspection_closed_business")]
        public bool InspectionClosedBusiness { get; set; }

        [JsonPropertyName("violation_type")]
        public string? ViolationType { get; set; }

        [JsonPropertyName("violation_description")]
        public string? ViolationDescription { get; set; }

        [JsonPropertyName("violation_points")]
        public string? ViolationPoints { get; set; }

        [JsonPropertyName("business_id")]
        public string? BusinessId { get; set; }

        [JsonPropertyName("inspection_serial_num")]
        public string? InspectionSerialNum { get; set; }

        [JsonPropertyName("violation_record_id")]
        public string? ViolationRecordId { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }
    }
}