using FoodInspectorAPI.Models;

namespace FoodInspectorAPI.Providers
{
    public interface IInspectionRecordsProvider
    {
        Task<List<InspectionRecordRaw>> GetInspections(List<EstablishmentsModel> establishmentsModels);
        Task<List<InspectionRecordRaw>> GetInspections(string name, string city, string date);
    }
}