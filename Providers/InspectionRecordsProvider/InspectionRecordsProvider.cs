using FoodInspector.Providers;
using FoodInspectorAPI.ConfigurationOptions;
using FoodInspectorAPI.Models;
using FoodInspectorModels;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoodInspectorAPI.Providers
{
    public class InspectionRecordsProvider : IInspectionRecordsProvider
    {
        private string _base_uri = string.Empty;
        private string _relative_uri = string.Empty;
        private Uri _uri;

        private readonly string _headername = "X-App-Token";
        private string _app_token = string.Empty;
        private string _startdate = string.Empty;

        private HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEstablishmentsStorageTableProvider _storageTableProvider;
        private readonly IOptions<AppTokenOptions> _appTokendOptions;
        private readonly ILogger _logger;

        private List<EstablishmentsModel>? _establishmentsList;

        public InspectionRecordsProvider(
            IHttpClientFactory httpClientFactory,
            IEstablishmentsStorageTableProvider storageTableProvider,
            IOptions<AppTokenOptions> appTokendOptions,
            ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _storageTableProvider = storageTableProvider;
            _appTokendOptions = appTokendOptions;
            _logger = loggerFactory.CreateLogger<InspectionRecordsProvider>();

            _base_uri = _appTokendOptions.Value.BaseUri;
            _relative_uri = _appTokendOptions.Value.RelativeUri;
            _app_token = _appTokendOptions.Value.KingCountyAppToken;
            _startdate = _appTokendOptions.Value.StartDate;
            _uri = new Uri(_base_uri);

            // Configure the HttpClient with the app token and data type
            _client = _httpClientFactory.CreateClient();
            _client.BaseAddress = _uri;
            _client.DefaultRequestHeaders.Add(_headername, _app_token);

            // Populate the table of establishments from the JSON file
            _storageTableProvider.CreateEstablishmentsSet().GetAwaiter().GetResult();
            _establishmentsList = _storageTableProvider.GetEstablishmentsSet().GetAwaiter().GetResult();
        }

        public async Task<List<InspectionRecordRaw>> GetSingleEstablishmentAllInspectionsRaw(string programIdentifier, string city, string startdate)
        {
            List<InspectionRecordRaw> inspectionRecords = new List<InspectionRecordRaw>();

            inspectionRecords = await GetInspectionsRawAsync(programIdentifier, city, startdate);

            return inspectionRecords;
        }

        public async Task<List<InspectionRecordRaw>> GetAllEstablishmentsAllInspectionsRaw()
        {
            List<InspectionRecordRaw> inspectionRecords = new List<InspectionRecordRaw>();

            inspectionRecords = await GetInspectionsRawAsync();

            return inspectionRecords;
        }

        public async Task<List<InspectionRecordRaw>> GetSingleEstablishmentLatestInspectionRaw(string programIdentifier, string city, string startdate)
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords = await GetInspectionsRawAsync(programIdentifier, city, startdate);

            var maxDate = inspectionRecords.Max(rec => rec.InspectionDate);

            List<InspectionRecordRaw> latestRecords = inspectionRecords.Where(r => r.InspectionDate == maxDate).ToList();

            return latestRecords;
        }

        public async Task<List<InspectionRecordRaw>> GetAllEstablishmentsLatestInspectionsRaw()
        {
            // Query the API to obtain the food inspection records
            List<InspectionRecordRaw> inspectionRecords = await GetInspectionsRawAsync();
            List<InspectionRecordRaw> latestRecords = new();

            var distinctEstablishments = inspectionRecords.DistinctBy(rec => rec.ProgramIdentifier).Select(rec => new
            {
                ProgramIdentifier = rec.ProgramIdentifier
            }).ToList();

            foreach (var record in distinctEstablishments)
            {
                // Get all records for a single establishment
                var singleEstablishmentrecords = inspectionRecords.Where(rec => rec.ProgramIdentifier == record.ProgramIdentifier).ToList();

                // Get the latest inspection date for a single establishment
                var maxDate = singleEstablishmentrecords.Max(rec => rec.InspectionDate);

                // Get all records where inspection date is the latest date
                var singleEstablishmentrecordsLatest = singleEstablishmentrecords.Where(rec => rec.InspectionDate == maxDate).ToList();

                latestRecords.AddRange(singleEstablishmentrecordsLatest);
            }

            return latestRecords;
        }


        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// For each establishment, return only the data for the most recent inspection.
        /// </summary>
        /// <returns>JSON list of InspectionRecordAggregated items</returns>
        public async Task<List<InspectionRecordAggregated>> GetSingleEstablishmentAllInspectionsAggregated(string programIdentifier, string city, string startdate)
        {
            // Fetch inspection records
            var inspectionRecords = await GetInspectionsRawAsync(programIdentifier, city, startdate);

            // Aggregate data into InspectionRecordAggregated objects
            var aggregatedRecord = AggregateInspectionRecords(inspectionRecords);

            return aggregatedRecord;
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// For each establishment, return only the data for the most recent inspection.
        /// </summary>
        /// <returns>JSON list of InspectionRecordAggregated items</returns>
        public async Task<List<InspectionRecordAggregated>> GetAllEstablishmentsAllInspectionsAggregated()
        {
            // Fetch inspection records
            var inspectionRecords = await GetInspectionsRawAsync();

            // Aggregate data into InspectionRecordAggregated objects
            var aggregatedRecord = AggregateInspectionRecords(inspectionRecords);

            return aggregatedRecord;
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// For each establishment, return only the data for the most recent inspection.
        /// </summary>
        /// <returns>JSON list of InspectionRecordAggregated items</returns>
        public async Task<List<InspectionRecordAggregated>> GetSingleEstablishmentLatestInspectionAggregated(string programIdentifier, string city, string startdate)
        {
            // Fetch inspection records
            var inspectionRecords = await GetInspectionsRawAsync(programIdentifier, city, startdate);

            // Aggregate data into InspectionRecordAggregated objects
            var aggregatedRecord = AggregateInspectionRecordsLatest(inspectionRecords);

            return aggregatedRecord;
        }

        /// <summary>
        /// Query inspection data for all establishments and inspections since StartDate.
        /// Aggregate data for each inspection of an establishment into an InspectionRecordAggregated object.
        /// For each establishment, return only the data for the most recent inspection.
        /// </summary>
        /// <returns>JSON list of InspectionRecordAggregated items</returns>
        public async Task<List<InspectionRecordAggregated>> GetAllEstablishmentsLatestInspectionsAggregated()
        {
            // Fetch inspection records
            var inspectionRecords = await GetInspectionsRawAsync();

            // Aggregate data into InspectionRecordAggregated objects
            var aggregatedRecords = AggregateInspectionRecordsLatest(inspectionRecords);

            return aggregatedRecords;
        }

        /// <summary>
        /// Obtains food inspection data for establishments matching the name, city, and startdate
        /// </summary>
        /// <param name="programIdentifier">The name of the establishment for which to query</param>
        /// <param name="city">The city of the establishment for which to query</param>
        /// <param name="startdate">The earliest date of the inspection for which to query</param>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        private async Task<List<InspectionRecordRaw>> GetInspectionsRawAsync(string programIdentifier, string city, string startdate)
        {
            List<InspectionRecordRaw> list = new List<InspectionRecordRaw>();

            try
            {
                // Create the model containing the parameter values on which to search
                InspectionRecordQueryParameters inspectionRequest = CreateInspectionRequest(
                    programIdentifier,
                    city,
                    startdate);

                if (inspectionRequest != null && !string.IsNullOrEmpty(inspectionRequest.Query))
                {
                    // Call the API to obtain the data
                    // The HttpClient does the actual calls to get the data.
                    list = await MakeGetRequestAsync(inspectionRequest.Query) ?? new List<InspectionRecordRaw>();

                    AssignViolationRecordIds(list);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInspections] Failed to query inspection data for {programIdentifier} in {city} from {startdate}. Exception: {ex}");
                list.Clear();   // Return an empty list
            }

            return list;
        }

        /// <summary>
        /// Obtains food inspection data for a set of EstablishmentModels
        /// </summary>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        private async Task<List<InspectionRecordRaw>> GetInspectionsRawAsync()
        {
            List<InspectionRecordRaw> list = new List<InspectionRecordRaw>();

            try
            {
                foreach (EstablishmentsModel establishmentsModel in _establishmentsList ?? Enumerable.Empty<EstablishmentsModel>())
                {
                    if (establishmentsModel == null ||
                        string.IsNullOrEmpty(establishmentsModel.ProgramIdentifier) ||
                        string.IsNullOrEmpty(establishmentsModel.City))
                    {
                        continue;
                    }

                    // Set the parameter values on which to search
                    InspectionRecordQueryParameters inspectionRequest = CreateInspectionRequest(
                        establishmentsModel.ProgramIdentifier,
                        establishmentsModel.City,
                        _startdate);

                    if (inspectionRequest == null ||
                        string.IsNullOrEmpty(inspectionRequest.Query))
                    {
                        continue;
                    }

                    // Call the API to obtain the data
                    // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                    List<InspectionRecordRaw>? results = await MakeGetRequestAsync(inspectionRequest.Query);

                    if (results == null) continue;

                    list.AddRange(results);
                }

                AssignViolationRecordIds(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInspections] Failed to query inspection data. Exception: {ex}");
                list.Clear();   // Return an empty list
            }

            return list;
        }

        // This method creates the URI used to call the API from the query parameters
        private InspectionRecordQueryParameters CreateInspectionRequest(string programIdentifier, string city, string startdate)
        {
            // Format the query URI to contain the complete URI plus search parameters
            List<string> whereClause = new();
            string limit = "50000";
            string escapedProgramIdentifier = EscapeForSoQL(programIdentifier);

            // Build the 'where' clause with programIdentifier, city, and startdate
            if (!string.IsNullOrEmpty(escapedProgramIdentifier))
            {
                whereClause.Add($"upper(program_identifier)=upper('{Uri.EscapeDataString(escapedProgramIdentifier)}')");
            }

            if (!string.IsNullOrEmpty(city))
            {
                whereClause.Add($"upper(city)=upper('{Uri.EscapeDataString(city)}')");
            }

            whereClause.Add("inspection_date > \'" + (!string.IsNullOrEmpty(startdate) ? startdate : "2020-01-01") + "T00:00:00.000\'");

            string where = string.Join(" AND ", whereClause);

            string fullUrl = $"{_base_uri + _relative_uri}?$limit={limit}&$where={where}";

            // Create the request object with the parameters for which to search
            InspectionRecordQueryParameters inspectionDataRequest = new InspectionRecordQueryParameters
            {
                Program_Identifier = escapedProgramIdentifier,
                City = city,
                Inspection_Date = startdate,
                Query = fullUrl
            };

            return inspectionDataRequest;
        }

        // Never return null when a Task is expected, but can return null wrapped in a Task
        private async Task<List<InspectionRecordRaw>?> MakeGetRequestAsync(
            string relativeUri)
        {
            // Make the API call
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);

            // Parse the result
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<InspectionRecordRaw>? result = JsonSerializer.Deserialize<List<InspectionRecordRaw>>(content);

            return result;
        }

        private void AssignViolationRecordIds(IEnumerable<InspectionRecordRaw> list)
        {
            // Group all InspectionRecord entries by Inspection_Serial_Num because each inspection
            // can result in zero or more violations and we want to keep them grouped together per establishment
            List<IGrouping<string?, InspectionRecordRaw>> ordered = list.GroupBy(x => x.InspectionSerialNum).ToList();

            // Add a zero-based ID to each inspection entry.  If an inspection resulted in multiple
            // violations, each one will have an ID such as 0, 1, 2, ... , n
            for (int i = 0; i < ordered.Count(); i++)
            {
                int j = 0;
                ordered[i].ToList().ForEach(x => x.Id = j++);
            }
        }

        /// <summary>
        /// Aggregates inspection records into a list of InspectionRecordAggregated.
        /// </summary>
        private List<InspectionRecordAggregated> AggregateInspectionRecordsLatest(List<InspectionRecordRaw> inspectionRecords)
        {
            // Group records by establishment name and save the date of the latest inspection as LatestInspection
            var groupedRecords = inspectionRecords
                .GroupBy(rec => rec.ProgramIdentifier)
                .Select(g => new
                {
                    ProgramIdentifier = g.Key,
                    Records = g.ToList(),
                    LatestInspection = g.OrderByDescending(rec => rec.InspectionDate).FirstOrDefault()
                })
                .ToList();

            // Map to aggregated model (InspectionRecordAggregated) and add each violation with an inspection date equal to LatestInspection
            List<InspectionRecordAggregated> aggegatedInspections = groupedRecords
                .Select(group => new InspectionRecordAggregated
                {
                    ProgramIdentifier = group.ProgramIdentifier,
                    Name = group.LatestInspection?.Name,
                    Description = group.LatestInspection?.Description,
                    Address = group.LatestInspection?.Address,
                    City = group.LatestInspection?.City,
                    ZipCode = group.LatestInspection?.ZipCode,
                    InspectionBusinessName = group.LatestInspection?.InspectionBusinessName,
                    InspectionType = group.LatestInspection?.InspectionType,
                    InspectionScore = group.LatestInspection?.InspectionScore,
                    InspectionResult = group.LatestInspection?.InspectionResult,
                    InspectionClosedBusiness = group.LatestInspection?.InspectionClosedBusiness,
                    InspectionSerialNum = group.LatestInspection?.InspectionSerialNum,
                    InspectionDate = group.LatestInspection?.InspectionDate,
                    Violations = group.Records
                        .Where(r => r.InspectionDate == group.LatestInspection?.InspectionDate && r.ViolationDescription != null)
                        .Select(r => new Violation
                        {
                            ViolationType = r.ViolationType,
                            ViolationDescription = r.ViolationDescription,
                            ViolationPoints = r.ViolationPoints
                        })
                        .ToList()
                }).ToList();

            return aggegatedInspections;
        }

        /// <summary>
        /// Aggregates inspection records into a list of InspectionRecordAggregated, grouped by program identifier and inspection date.
        /// </summary>
        private List<InspectionRecordAggregated> AggregateInspectionRecords(List<InspectionRecordRaw> inspectionRecords)
        {
            // Group by establishment programIdentifier and then by inspection date
            var groupedRecords = inspectionRecords
                .GroupBy(rec => new { rec.ProgramIdentifier, rec.InspectionDate })
                .Select(g => new InspectionRecordAggregated
                {
                    ProgramIdentifier = g.Key.ProgramIdentifier,
                    InspectionDate = g.Key.InspectionDate,
                    Name = g.First().Name,
                    Description = g.First().Description,
                    Address = g.First().Address,
                    City = g.First().City,
                    ZipCode = g.First().ZipCode,
                    InspectionBusinessName = g.First().InspectionBusinessName,
                    InspectionType = g.First().InspectionType,
                    InspectionScore = g.First().InspectionScore,
                    InspectionResult = g.First().InspectionResult,
                    InspectionClosedBusiness = g.First().InspectionClosedBusiness,
                    InspectionSerialNum = g.First().InspectionSerialNum,
                    Violations = g
                        .Where(r => r.ViolationDescription != null)
                        .Select(r => new Violation
                        {
                            ViolationType = r.ViolationType,
                            ViolationDescription = r.ViolationDescription,
                            ViolationPoints = r.ViolationPoints
                        })
                        .ToList()
                })
                .ToList();

            return groupedRecords;
        }

        private string EscapeForSoQL(string value)
        {
            return value.Replace("'", "''"); // Escape single quotes
        }
    }
}
