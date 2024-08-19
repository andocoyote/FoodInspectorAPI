using FoodInspectorAPI.Models;

namespace FoodInspector.Providers
{
    public interface IEstablishmentsStorageTableProvider
    {
        Task CreateEstablishmentsSet();

        // Never return null when a Task is expected, but can return null wrapped in a Task
        Task<List<EstablishmentsModel>?> GetEstablishmentsSet();
    }
}