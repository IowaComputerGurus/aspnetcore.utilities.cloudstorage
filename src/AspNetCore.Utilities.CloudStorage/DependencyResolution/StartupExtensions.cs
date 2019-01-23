using ICG.AspNetCore.Utilities.CloudStorage;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Registers the items included in the ICG AspNetCore Utilities project for Dependency Injection
        /// </summary>
        /// <param name="services">Your existing services collection</param>
        /// <param name="configuration">The current configuration of the application</param>
        public static void UseIcgAspNetCoreUtilitiesCloudStorage(this IServiceCollection services, IConfiguration configuration)
        {
            //Bind additional services
            services.AddTransient<IMimeTypeMapper, MimeTypeMapper>();
            services.AddTransient<IAzureCloudStorageProvider, AzureCloudStorageProvider>();
            services.Configure<AzureCloudStorageOptions>(configuration.GetSection(nameof(AzureCloudStorageOptions)));
        }
    }
}