using FoodInspector.Providers;
using FoodInspectorAPI.Models;
using FoodInspectorAPI.Providers;
using Microsoft.AspNetCore.Mvc;

namespace FoodInspectorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodInspectorController : ControllerBase
    {
        private readonly IInspectionRecordsProvider _inspectionRecordsProvider;
        private readonly IEstablishmentsStorageTableProvider _storageTableProvider;
        private readonly ILogger<FoodInspectorController> _logger;

        public FoodInspectorController(
            IInspectionRecordsProvider inspectionRecordsProvider,
            IEstablishmentsStorageTableProvider storageTableProvider,
            ILogger<FoodInspectorController> logger)
        {
            _inspectionRecordsProvider = inspectionRecordsProvider;
            _storageTableProvider = storageTableProvider;
            _logger = logger;
        }

        [HttpGet("default/inspections")]
        public async Task<IActionResult> GetInspections()
        {
            // Populate the table of establishments from the text file
            await _storageTableProvider.CreateEstablishmentsSet();

            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel>? establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            if (establishmentsList == null)
            {
                _logger.LogError("EstablishmentsSet is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Query the API to obtain the food inspection records
            List<InspectionRecord> inspectionRecords = await _inspectionRecordsProvider.GetInspections(establishmentsList);

            return Ok(inspectionRecords);
        }

        [HttpGet("userconfigured/inspections")]
        public async Task<IActionResult> GetInspections(
            [FromQuery] string name,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecord> inspectionRecords =
                await _inspectionRecordsProvider.GetInspections(name, city, startdate);

            return Ok(inspectionRecords);
        }
    }
}
