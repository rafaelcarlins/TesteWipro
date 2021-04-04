using APIFila.Model;
using ConsultaFila.Business;
using ConsultaFila.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsultaFila
{
    class Program
    {
        static void Main()
        {

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ExecutarAsync().Wait();

            sw.Stop();

            Console.WriteLine("Tempo gasto : " + sw.ElapsedMilliseconds.ToString() + " milisegundos");


        }

        static async Task ExecutarAsync()
        {

            ItemFilaBusiness itemFila = new ItemFilaBusiness();

            itemFila.GerarArquivoTeste();


            ItemFila retItemFila = await itemFila.BuscaItemFila();

            if (retItemFila.MensagemRetorno != null)
            {
                return;
            }
            List<DadosMoeda> ListaDadosMoeda = new List<DadosMoeda>();
            ListaDadosMoeda = itemFila.DadosMoeda(retItemFila);

            List<DePara> ListarDePara = new List<DePara>();
            ListarDePara = itemFila.DadosDePara();

            List<DadosCotacao> ListaDadosCotacao = new List<DadosCotacao>();
            ListaDadosCotacao = itemFila.DadosCotacao();

            IList<string> ListaResultado = new List<string>();

            ListaResultado = itemFila.ConcatenaListas(ListaDadosMoeda, ListarDePara, ListaDadosCotacao);

            itemFila.GerarArquivo(ListaResultado);
        }

    }
}
