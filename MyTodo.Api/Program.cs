using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyTodo.Api.Core;
using System.Threading.Tasks;

namespace MyTodo.Api
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IJwtTokenValidator, JwtTokenValidator>();
                    services.AddScoped(typeof(ITableStorageDataService<>), typeof(TableStorageDataService<>));
                })
                .Build();

            await host.RunAsync();
        }
    }
}
