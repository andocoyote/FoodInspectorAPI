using System.Text.Json;
using FoodInspectorAPI.Models;

namespace FoodInspector.Providers
{
    public class EstablishmentsProvider : IEstablishmentsProvider
    {
        private readonly string _path = @"Data\Establishments.json";

        /// <summary>
        /// Reads the JSON file containing establishment descriptors and creates a list of EstablishmentModel items
        /// </summary>
        /// <returns>A list of EstablishmentModel items</returns>
        public List<EstablishmentsModel>? ReadEstablishmentsFile()
        {
            string json = File.ReadAllText(_path);
            List<EstablishmentsModel>? establishments = JsonSerializer.Deserialize<List<EstablishmentsModel>>(json);

            return establishments;
        }
    }
}
