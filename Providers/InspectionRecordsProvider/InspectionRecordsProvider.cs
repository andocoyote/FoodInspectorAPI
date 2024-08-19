using FoodInspectorAPI.ConfigurationOptions;
using FoodInspectorAPI.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json;
using System.Web;

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
        private readonly IOptions<AppTokenOptions> _appTokendOptions;
        private readonly ILogger _logger;

        public InspectionRecordsProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<AppTokenOptions> appTokendOptions,
            ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
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
        }

        /// <summary>
        /// Obtains food inspection data for a set of EstablishmentModels
        /// </summary>
        /// <param name="establishmentsModels">The list of EstablishmentModels to query</param>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        public async Task<List<InspectionRecord>> GetInspections(List<EstablishmentsModel> establishmentsModels)
        {
            List<InspectionRecord> list = new List<InspectionRecord>();

            try
            {
                foreach (EstablishmentsModel establishmentsModel in establishmentsModels)
                {
                    if (establishmentsModel == null ||
                        string.IsNullOrEmpty(establishmentsModel.Name) ||
                        string.IsNullOrEmpty(establishmentsModel.City))
                    {
                        continue;
                    }

                    // Set the parameter values on which to search
                    InspectionRecordQueryParameters inspectionRequest = CreateInspectionRequest(
                        establishmentsModel.Name,
                        establishmentsModel.City,
                        _startdate);

                    if (inspectionRequest == null ||
                        string.IsNullOrEmpty(inspectionRequest.Query))
                    {
                        continue;
                    }

                    // Call the API to obtain the data
                    // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                    List<InspectionRecord>? results = await MakeGetRequestAsync(inspectionRequest.Query);

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

        /// <summary>
        /// Obtains food inspection data for establishments matching the name, city, and startdate
        /// </summary>
        /// <param name="name">The name of the establishment for which to query</param>
        /// <param name="city">The city of the establishment for which to query</param>
        /// <param name="startdate">The earliest date of the inspection for which to query</param>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        public async Task<List<InspectionRecord>> GetInspections(string name, string city, string startdate)
        {
            List<InspectionRecord> list = new List<InspectionRecord>();

            try
            {
                // Set the parameter values on which to search
                InspectionRecordQueryParameters inspectionRequest = CreateInspectionRequest(
                    name,
                    city,
                    startdate);

                if (inspectionRequest != null && !string.IsNullOrEmpty(inspectionRequest.Query))
                {
                    // Call the API to obtain the data
                    // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                    list = await MakeGetRequestAsync(inspectionRequest.Query) ?? new List<InspectionRecord>();

                    AssignViolationRecordIds(list);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInspections] Failed to query inspection data for {name} in {city} from {startdate}. Exception: {ex}");
                list.Clear();   // Return an empty list
            }

            return list;
        }

        // This method creates the URI used to call the API from the query parameters
        private InspectionRecordQueryParameters CreateInspectionRequest(string name, string city, string startdate)
        {
            // Format the query URI to contain the complete URI plus search parameters
            UriBuilder builder = new UriBuilder(_base_uri + _relative_uri);
            NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);
            query["$limit"] = "50000";

            if (!string.IsNullOrEmpty(name))
            {
                query["name"] = name.ToUpper();
            }

            if (!string.IsNullOrEmpty(city))
            {
                query["city"] = city.ToUpper();
            }

            // Ex: "city in('KIRKLAND', 'REDMOND') AND inspection_date > 2020-01-01"
            query["$where"] = "inspection_date > \'" + (!string.IsNullOrEmpty(startdate) ? startdate : "2020-01-01") + "T00:00:00.000\'";

            builder.Query = query.ToString();

            // Create the request object with the parameters for which to search
            InspectionRecordQueryParameters inspectionDataRequest = new InspectionRecordQueryParameters
            {
                Name = name,
                City = city,
                Inspection_Date = startdate,
                Query = builder.ToString()
            };

            return inspectionDataRequest;
        }

        // Never return null when a Task is expected, but can return null wrapped in a Task
        private async Task<List<InspectionRecord>?> MakeGetRequestAsync(
            string relativeUri)
        {
            // Make the API call
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            HttpResponseMessage response = await _client.SendAsync(request).ConfigureAwait(false);

            // Parse the result
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<InspectionRecord>? result = JsonSerializer.Deserialize<List<InspectionRecord>>(content);

            return result;
        }

        private void AssignViolationRecordIds(IEnumerable<InspectionRecord> list)
        {
            // Group all InspectionRecord entries by Inspection_Serial_Num because each inspection
            // can result in zero or more violations and we want to keep them grouped together per establishment
            List<IGrouping<string?, InspectionRecord>> ordered = list.GroupBy(x => x.InspectionSerialNum).ToList();

            // Add a zero-based ID to each inspection entry.  If an inspection resulted in multiple
            // violations, each one will have an ID such as 0, 1, 2, ... , n
            for (int i = 0; i < ordered.Count(); i++)
            {
                int j = 0;
                ordered[i].ToList().ForEach(x => x.Id = j++);
            }
        }
    }
}
