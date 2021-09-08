using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FunctionAppDadosCadastrais.Business;
using FunctionAppDadosCadastrais.Models;

namespace FunctionAppDadosCadastrais
{
    public class DadosCadastrais
    {
        private readonly CadastroServices _cadastroSvc;

        public DadosCadastrais(CadastroServices cadastroSvc)
        {
            _cadastroSvc = cadastroSvc;
        }

        [Function(nameof(GetDadosCadastrais))]
        [OpenApiOperation(operationId: "DadosCadastrais", tags: new[] { "DadosCadastrais" }, Summary = "Cadastro", Description   = "Consultar Dados Cadastrais", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Cadastro[]), Summary = "Dados Cadastrais", Description = "Dados Cadastrais")]

        public async Task<HttpResponseData> GetDadosCadastrais(
            [HttpTrigger(AuthorizationLevel.Function, "get",
                Route = "DadosCadastrais")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(GetDadosCadastrais));
            logger.LogInformation("Iniciando a consulta aos dados cadastrais...");

            var dadosCadastrais = _cadastroSvc.Get();

            logger.LogInformation(
                $"No. de registros encontrados: {dadosCadastrais.Count()}");

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(dadosCadastrais);
            return response;
        }

        [Function(nameof(PostDadosCadastrais))]
        [OpenApiOperation(operationId: "DadosCadastrais", tags: new[] { "DadosCadastrais" }, Summary = "Cadastro", Description   = "Incluir Dados Cadastrais", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Cadastro), Required = true, Description = "Objeto contendo os dados cadastrais")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Resultado), Summary = "Resultado da inclusão de Dados Cadastrais", Description = "Resultado da inclusão de Dados Cadastrais")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(Resultado), Summary = "Falha na inclusão de Dados Cadastrais", Description = "Falha na inclusão de Dados Cadastrais")]
        public async Task<HttpResponseData> PostDadosCadastrais(
            [HttpTrigger(AuthorizationLevel.Function, "post",
                Route = "DadosCadastrais")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(PostDadosCadastrais));
            logger.LogInformation("Iniciando a inclusao dos dados cadastrais...");

            var response = req.CreateResponse();
            var resultado = _cadastroSvc.Insert(req);
            await response.WriteAsJsonAsync(resultado);

            if (resultado.Sucesso)
                logger.LogInformation("Dados cadastrais incluidos com sucesso!");
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                logger.LogError("Falha na inclusao dos dados cadastrais!");
                foreach (var inconsistencia in resultado.Inconsistencias)
                    logger.LogError($" ## {inconsistencia}");
            }

            return response;
        }
    }
}