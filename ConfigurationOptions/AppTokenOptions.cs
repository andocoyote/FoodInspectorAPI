namespace FoodInspectorAPI.ConfigurationOptions
{
    public class AppTokenOptions
    {
        public AppTokenOptions()
        {
            KingCountyAppToken = string.Empty;
            BaseUri = string.Empty;
            RelativeUri = string.Empty;
            StartDate = string.Empty;
        }

        /// <summary>
        /// The app token for King Country food establishment inspection data.
        /// </summary>
        public string KingCountyAppToken { get; set; }

        public string BaseUri { get; set; }

        public string RelativeUri { get; set; }

        public string StartDate { get; set; }
    }
}
