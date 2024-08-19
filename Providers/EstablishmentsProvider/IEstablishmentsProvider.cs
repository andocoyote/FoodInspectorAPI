using FoodInspectorAPI.Models;

namespace FoodInspector.Providers
{
    public interface IEstablishmentsProvider
    {
        List<EstablishmentsModel>? ReadEstablishmentsFile();
    }
}