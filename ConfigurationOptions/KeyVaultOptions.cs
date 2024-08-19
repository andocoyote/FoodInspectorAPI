namespace FoodInspectorAPI.ConfigurationOptions
{
    /// <summary>
    /// Provides access to Key Vault app settings using options.
    /// </summary>
    public class KeyVaultOptions
    {
        public KeyVaultOptions()
        {
            VaultName = string.Empty;
        }

        /// <summary>
        /// The configured Key Vault vault name.
        /// </summary>
        public string VaultName { get; set; }
    }
}
