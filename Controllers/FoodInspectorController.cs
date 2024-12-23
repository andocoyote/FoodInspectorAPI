using FoodInspector.Providers;
using FoodInspectorAPI.Providers;
using FoodInspectorModels;
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

            // Populate the table of establishments from the JSON file
            _storageTableProvider.CreateEstablishmentsSet().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/SingleEstablishmentAllInspectionsRaw")]
        public async Task<IActionResult> GetSingleEstablishmentAllInspectionsRaw(
            [FromQuery] string programIdentifier,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords =
                await _inspectionRecordsProvider.GetSingleEstablishmentAllInspectionsRaw(programIdentifier, city, startdate);

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the all establishments and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/AllEstablishmentsAllInspectionsRaw")]
        public async Task<IActionResult> GetAllEstablishmentsAllInspectionsRaw()
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords =
                await _inspectionRecordsProvider.GetAllEstablishmentsAllInspectionsRaw();

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/SingleEstablishmentLatestInspectionRaw")]
        public async Task<IActionResult> GetSingleEstablishmentLatestInspectionRaw(
            [FromQuery] string programIdentifier,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords =
                await _inspectionRecordsProvider.GetSingleEstablishmentLatestInspectionRaw(programIdentifier, city, startdate);

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the all establishments their latest inspection
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/AllEstablishmentsLatestInspectionsRaw")]
        public async Task<IActionResult> GetAllEstablishmentsLatestInspectionsRaw()
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords =
                await _inspectionRecordsProvider.GetAllEstablishmentsLatestInspectionsRaw();

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/SingleEstablishmentAllInspectionsAggregated")]
        public async Task<IActionResult> GetSingleEstablishmentAllInspectionsAggregated(
            [FromQuery] string programIdentifier,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordAggregated> inspectionRecords =
                await _inspectionRecordsProvider.GetSingleEstablishmentAllInspectionsAggregated(programIdentifier, city, startdate);

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/AllEstablishmentsAllInspectionsAggregated")]
        public async Task<IActionResult> GetAllEstablishmentsAllInspectionsAggregated()
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordAggregated> inspectionRecords =
                await _inspectionRecordsProvider.GetAllEstablishmentsAllInspectionsAggregated();

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/SingleEstablishmentLatestInspectionAggregated")]
        public async Task<IActionResult> GetSingleEstablishmentLatestInspectionAggregated(
            [FromQuery] string programIdentifier,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordAggregated> inspectionRecords =
                await _inspectionRecordsProvider.GetSingleEstablishmentLatestInspectionAggregated(programIdentifier, city, startdate);

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// </summary>
        /// <returns>JSON list InspectionRecordAggregated items</returns>
        [HttpGet("DefaultQueries/AllEstablishmentsLatestInspectionsAggregated")]
        public async Task<IActionResult> GetAllEstablishmentsLatestInspectionsAggregated()
        {
            List<InspectionRecordAggregated> inspectionRecords =
                await _inspectionRecordsProvider.GetAllEstablishmentsLatestInspectionsAggregated();
            return Ok(inspectionRecords);
        }
    }
}
