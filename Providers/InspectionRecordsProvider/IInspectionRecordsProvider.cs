using FoodInspectorAPI.Models;

namespace FoodInspectorAPI.Providers
{
    public interface IInspectionRecordsProvider
    {
        Task<List<InspectionRecord>> GetInspections(List<EstablishmentsModel> establishmentsModels);
        Task<List<InspectionRecord>> GetInspections(string name, string city, string date);
    }
}