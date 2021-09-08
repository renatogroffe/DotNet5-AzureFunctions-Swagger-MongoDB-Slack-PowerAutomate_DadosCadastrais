using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using FunctionAppDadosCadastrais.Data;
using FunctionAppDadosCadastrais.Models;
using FunctionAppDadosCadastrais.Validators;
using FunctionAppDadosCadastrais.Clients;

namespace FunctionAppDadosCadastrais.Business
{
    public class CadastroServices
    {
        private readonly CadastroRepository _repository;
        private readonly CanalSlackClient _clientSlack;
        private readonly PowerAutomateClient _clientPowerAutomate;

        public CadastroServices(CadastroRepository repository,
            CanalSlackClient clientSlack,
            PowerAutomateClient clientPowerAutomate)
        {
            _repository = repository;
            _clientSlack = clientSlack;
            _clientPowerAutomate = clientPowerAutomate;
        }

        public List<Cadastro> Get()
        {
            return _repository.ListAll().ConvertAll<Cadastro>(doc => new ()
            {
                nome = doc.Nome,
                nome_pai = doc.NomePai,
                nome_mae = doc.NomeMae,
                tecnologia = doc.Tecnologia,
                idade = doc.Idade,
                cidade = doc.Localidade,
                aceito_novidades = doc.ReceberNovidades == "Sim"
            });
        }

        public Resultado Insert(HttpRequestData requestData)
        {
            var dadosCadastrais = DeserializeDadosCadastrais(requestData);
            var resultado = DadosValidos(dadosCadastrais);
            resultado.Acao = "Inclusão de Dados Cadastrais";
        
            if (resultado.Inconsistencias.Count == 0)
            {
                _repository.Save(new ()
                {
                    Nome = dadosCadastrais.nome,
                    NomePai = dadosCadastrais.nome_pai,
                    NomeMae = dadosCadastrais.nome_mae,
                    Tecnologia = dadosCadastrais.tecnologia,
                    Idade = dadosCadastrais.idade.Value,
                    Localidade = dadosCadastrais.cidade,
                    ReceberNovidades = dadosCadastrais.aceito_novidades.Value ? "Sim" : "Não"
                });
                _clientSlack.GerarAvisoInclusao(dadosCadastrais.nome);
                _clientPowerAutomate.EnviarCadastro(dadosCadastrais);
            }
            else
                _clientSlack.GerarAvisoFalhaCadastro(
                    requestData.ReadAsString(), resultado.Inconsistencias);

            return resultado;
        }

        private Cadastro DeserializeDadosCadastrais(HttpRequestData requestData)
        {
            Cadastro dadosCadastrais;
            try
            {
                dadosCadastrais =
                    requestData.ReadFromJsonAsync<Cadastro>().AsTask().Result;
            }
            catch
            {
                dadosCadastrais = null;
            }

            return dadosCadastrais;
        }

        private Resultado DadosValidos(Cadastro cadastro)
        {
            var resultado = new Resultado();
            if (cadastro == null)
            {
                resultado.Inconsistencias.Add(
                    "Preencha os Dados Cadastrais");
            }
            else
            {
                var validator = new CadastroValidator().Validate(cadastro);
                if (!validator.IsValid)
                {
                    foreach (var errors in validator.Errors)
                        resultado.Inconsistencias.Add(errors.ErrorMessage);
                }
            }

            return resultado;
        }
    }
}