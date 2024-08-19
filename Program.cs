
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
            var environmentName = builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
            builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{builder.Configuration.GetValue<string>("KeyVault:VaultName")}.vault.azure.net/"),
                new DefaultAzureCredential());

            // Azure clients
            builder.Services.AddAzureClients(clientBuilder =>
            {
                string keyVaultUri = "https://kv-food-inspector.vault.azure.net/";
                clientBuilder.AddSecretClient(new Uri(keyVaultUri));
                clientBuilder.AddTableServiceClient(new Uri("https://stdiwithazuresdk.table.core.windows.net"));

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
