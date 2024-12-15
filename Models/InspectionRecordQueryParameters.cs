namespace FoodInspectorAPI.Models
{
    // Enumerates the fields used to search the inspections API
    // [DataContract]
    public class InspectionRecordQueryParameters
    {
        // [DataMember]
        public string? Program_Identifier { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Inspection_Date { get; set; } = null;
        public string? City { get; set; } = null;
        public string? Zip_Code { get; set; } = null;
        public string? Inspection_Score { get; set; } = null;
        public string? Inspection_Result { get; set; } = null;
        public string? Inspection_Closed_Business { get; set; } = null;
        public string? Violation_Points { get; set; } = null;
        public string? Grade { get; set; } = null;
        public string? Query { get; set; } = null;
    }
}
