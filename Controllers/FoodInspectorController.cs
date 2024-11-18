using FoodInspector.Providers;
using FoodInspectorAPI.Models;
using FoodInspectorAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Emit;

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
        /// Query inspection data for all establishments and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("DefaultQueries/InspectionsRaw")]
        public async Task<IActionResult> GetInspectionsRaw()
        {
            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel>? establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            if (establishmentsList == null)
            {
                _logger.LogError("EstablishmentsSet is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Query the inspection data API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords = await _inspectionRecordsProvider.GetInspections(establishmentsList);

            return Ok(inspectionRecords);
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// </summary>
        /// <returns>JSON list InspectionRecordAggregated items</returns>
        [HttpGet("DefaultQueries/InspectionsAggregatedAll")]
        public async Task<IActionResult> GetInspectionsAggregated()
        {
            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel>? establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            if (establishmentsList == null)
            {
                _logger.LogError("EstablishmentsSet is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords = await _inspectionRecordsProvider.GetInspections(establishmentsList);

            if (inspectionRecords == null)
            {
                _logger.LogError("inspectionRecords is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Create a list of InspectionRecordAggregated
            List<InspectionRecordAggregated> recordsAggregated = new List<InspectionRecordAggregated>();

            // Get the list of distinct extablishment names from inspectionRecords
            var distinctEstablishments = inspectionRecords.DistinctBy(rec => rec.Name).Select(rec => new InspectionRecordAggregated
            {
                Name = rec.Name,
                Description = rec.Description,
                Address = rec.Address,
                City = rec.City,
                ZipCode = rec.ZipCode,
                InspectionBusinessName = rec.InspectionBusinessName,
                InspectionType = rec.InspectionType,
                InspectionScore = rec.InspectionScore,
                InspectionResult = rec.InspectionResult,
                InspectionClosedBusiness = rec.InspectionClosedBusiness,
                InspectionSerialNum = rec.InspectionSerialNum
            }).ToList();

            // For each distinct establishment, get all records for that establishment
            foreach (InspectionRecordAggregated record in distinctEstablishments)
            {
                var records = inspectionRecords.Where(rec => rec.Name == record.Name).ToList();
                var maxDate = records.Max(rec => rec.InspectionDate);
                record.InspectionDate = inspectionRecords.Where(rec => rec.InspectionDate == maxDate).First().InspectionDate;

                // For each record for each distinct establishment, add a violation to the Violations list
                record.Violations = records.Where(rec => rec.InspectionDate == maxDate).ToList().Select(rec => new Violation
                {
                    ViolationType = rec.ViolationType,
                    ViolationDescription = rec.ViolationDescription,
                    ViolationPoints = rec.ViolationPoints
                }).ToList();
            }

            return Ok(distinctEstablishments);
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// For each establishment, return only the data for the most recent inspection.
        /// </summary>
        /// <returns>JSON list InspectionRecordAggregated items</returns>
        [HttpGet("DefaultQueries/InspectionsAggregatedLatest")]
        public async Task<IActionResult> GetInspectionsLatest()
        {
            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel>? establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            if (establishmentsList == null)
            {
                _logger.LogError("EstablishmentsSet is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords = await _inspectionRecordsProvider.GetInspections(establishmentsList);

            if (inspectionRecords == null)
            {
                _logger.LogError("inspectionRecords is null.");
                return StatusCode(500, "An unexpected error occurred.");
            }

            // Create a list of InspectionRecordAggregated
            List<InspectionRecordAggregated> recordsAggregated = new List<InspectionRecordAggregated>();

            // Get the list of distinct extablishment names from inspectionRecords
            var distinctEstablishments = inspectionRecords.DistinctBy(rec => rec.Name).Select(rec => new InspectionRecordAggregated
            {
                Name = rec.Name,
                Description = rec.Description,
                Address = rec.Address,
                City = rec.City,
                ZipCode = rec.ZipCode,
                InspectionBusinessName = rec.InspectionBusinessName,
                InspectionType = rec.InspectionType,
                InspectionScore = rec.InspectionScore,
                InspectionResult = rec.InspectionResult,
                InspectionClosedBusiness = rec.InspectionClosedBusiness,
                InspectionSerialNum = rec.InspectionSerialNum
            }).ToList();

            // For each distinct establishment, get all records for that establishment
            foreach (InspectionRecordAggregated record in distinctEstablishments)
            {
                var records = inspectionRecords.Where(rec => rec.Name == record.Name).ToList();
                var maxDate = records.Max(rec => rec.InspectionDate);
                record.InspectionDate = inspectionRecords.Where(rec => rec.InspectionDate == maxDate).First().InspectionDate;

                // For each record for each distinct establishment, add a violation to the Violations list
                record.Violations = records.Where(rec => rec.InspectionDate == maxDate).ToList().Select(rec => new Violation
                {
                    ViolationType = rec.ViolationType,
                    ViolationDescription = rec.ViolationDescription,
                    ViolationPoints = rec.ViolationPoints
                }).ToList();
            }

            return Ok(distinctEstablishments);
        }

        /// <summary>
        /// Query inspection data for the specified establishment and inspections since StartDate
        /// </summary>
        /// <returns>JSON list InspectionRecordRaw items</returns>
        [HttpGet("UserQueries/InspectionsRaw")]
        public async Task<IActionResult> GetInspectionsRaw(
            [FromQuery] string name,
            [FromQuery] string city,
            [FromQuery] string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords =
                await _inspectionRecordsProvider.GetInspections(name, city, startdate);

            return Ok(inspectionRecords);
        }
    }
}
