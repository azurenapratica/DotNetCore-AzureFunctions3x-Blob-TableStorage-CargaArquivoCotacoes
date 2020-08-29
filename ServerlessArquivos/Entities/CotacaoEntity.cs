using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServerlessArquivos.Entities
{
    public class CotacaoEntity : TableEntity
    {
        public CotacaoEntity(string moeda)
        {
            PartitionKey = moeda;
            RowKey = Guid.NewGuid().ToString();
            Horario = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public CotacaoEntity() { }

        public string Horario { get; set; }
        public int Linha { get; set; }
        public double Valor { get; set; }
    }
}