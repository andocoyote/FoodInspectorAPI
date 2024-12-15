using FoodInspectorAPI.Models;

namespace FoodInspectorAPI.Providers
{
    public interface IInspectionRecordsProvider
    {
        Task<List<InspectionRecordRaw>> GetSingleEstablishmentAllInspectionsRaw(string programIdentifier, string city, string startdate);
        Task<List<InspectionRecordRaw>> GetAllEstablishmentsAllInspectionsRaw();
        Task<List<InspectionRecordRaw>> GetSingleEstablishmentLatestInspectionRaw(string programIdentifier, string city, string date);
        Task<List<InspectionRecordRaw>> GetAllEstablishmentsLatestInspectionsRaw();
        Task<List<InspectionRecordAggregated>> GetSingleEstablishmentAllInspectionsAggregated(string programIdentifier, string city, string startdate);
        Task<List<InspectionRecordAggregated>> GetAllEstablishmentsAllInspectionsAggregated();
        Task<List<InspectionRecordAggregated>> GetSingleEstablishmentLatestInspectionAggregated(string programIdentifier, string city, string startdate);
        Task<List<InspectionRecordAggregated>> GetAllEstablishmentsLatestInspectionsAggregated();
    }
}