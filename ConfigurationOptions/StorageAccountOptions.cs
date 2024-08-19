namespace FoodInspectorAPI.ConfigurationOptions
{
    public class StorageAccountOptions
    {
        public StorageAccountOptions()
        {
            StorageAccountKey = string.Empty;
            TableEndpoint = string.Empty;
            TableName = string.Empty;
            TableStorageAccountName = string.Empty;
        }

        /// <summary>
        /// The access key for the stfoodinspector Azure Storage Account.
        /// </summary>
        public string StorageAccountKey { get; set; }

        public string TableEndpoint { get; set; }

        public string TableName { get; set; }

        public string TableStorageAccountName { get; set; }
    }
}
