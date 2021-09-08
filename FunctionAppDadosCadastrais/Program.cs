using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FunctionAppDadosCadastrais.Clients;
using FunctionAppDadosCadastrais.Data;
using FunctionAppDadosCadastrais.Business;

namespace FunctionAppDadosCadastrais
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson())
                .ConfigureServices(services =>
                {
                    services.AddScoped<CadastroRepository>();
                    services.AddHttpClient<CanalSlackClient>();
                    services.AddHttpClient<PowerAutomateClient>();
                    services.AddScoped<CadastroServices>();
                })
                .ConfigureOpenApi()
                .Build();

            host.Run();
        }
    }
}