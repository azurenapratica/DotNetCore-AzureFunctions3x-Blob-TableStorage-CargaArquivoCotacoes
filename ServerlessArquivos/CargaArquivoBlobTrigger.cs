using System;
using System.IO;
using System.Globalization;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Logging;
using ServerlessArquivos.Entities;

namespace ServerlessArquivos
{
    public static class CargaArquivoBlobTrigger
    {
        private static readonly CultureInfo culturePtBr = new CultureInfo("pt-BR");

        [FunctionName("CargaArquivoBlobTrigger")]
        public static void Run([BlobTrigger("arquivos-processamento/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"Arquivo: {name}");

            if (myBlob.Length > 0)
            {
                using var reader = new StreamReader(myBlob);

                var storageAccount = CloudStorageAccount
                    .Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                var cotacoesTable = storageAccount
                    .CreateCloudTableClient().GetTableReference("CotacoesTable");

                if (cotacoesTable.CreateIfNotExistsAsync().Result)
                    log.LogInformation("Criando a tabela de Cotações...");

                int numLinha = 1;
                string linha = reader.ReadLine();
                while (linha != null)
                {
                    log.LogInformation($"Linha {numLinha}: {linha}");
                    var dados = linha.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    if (dados.Length == 2 &&
                        !String.IsNullOrWhiteSpace(dados[0]) &&
                        double.TryParse(dados[1], out _))
                    {
                        var valorCotacao = Convert.ToDouble(
                            dados[1], culturePtBr.NumberFormat);
                        var insertOperation = TableOperation.Insert(
                            new CotacaoEntity(dados[0].Trim())
                            {
                                Linha = numLinha,
                                Valor = valorCotacao
                            });
                        var resultInsert = cotacoesTable.ExecuteAsync(insertOperation).Result;
                        log.LogInformation("Dados incluídos com sucesso!");
                    }

                    numLinha++;
                    linha = reader.ReadLine();
                }
            }

            log.LogInformation($"Concluído o processamento do arquivo {name}");
        }
    }
}