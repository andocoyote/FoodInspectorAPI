
using Azure.Identity;
using FoodInspector.Providers;
using FoodInspectorAPI.ConfigurationOptions;
using FoodInspectorAPI.Providers;
using Microsoft.Extensions.Azure;

namespace FoodInspectorAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration sources
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            string environmentName = builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
            builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

            string storageUrl = $"https://{builder.Configuration.GetValue<string>("Storage:TableStorageAccountName")}.table.core.windows.net/";
            string keyVaultUrl = $"https://{builder.Configuration.GetValue<string>("KeyVault:VaultName")}.vault.azure.net/";

            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUrl),
                new DefaultAzureCredential());

            // Azure clients
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddSecretClient(new Uri(keyVaultUrl));
                clientBuilder.AddTableServiceClient(new Uri(storageUrl));

                clientBuilder.UseCredential(new DefaultAzureCredential());
            });

            // Services
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();
            builder.Services.AddSingleton<IInspectionRecordsProvider, InspectionRecordsProvider>();
            builder.Services.AddSingleton<IEstablishmentsStorageTableProvider, EstablishmentsStorageTableProvider>();
            builder.Services.AddSingleton<IEstablishmentsProvider, EstablishmentsProvider>();

            // Services options
            builder.Services.Configure<KeyVaultOptions>(
                builder.Configuration.GetSection("KeyVault"));
            builder.Services.Configure<StorageAccountOptions>(
                builder.Configuration.GetSection("Storage"));
            builder.Services.Configure<AppTokenOptions>(
                builder.Configuration.GetSection("AppToken"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
