using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

[assembly: FunctionsStartup(typeof(Arian.Imaging.Sample.Startup))]

namespace Arian.Imaging.Sample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, ApplicationInsightsLoggerProvider>();
        }
    }
}