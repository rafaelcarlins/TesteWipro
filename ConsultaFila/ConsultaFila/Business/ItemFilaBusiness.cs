using APIFila.Model;
using ConsultaFila.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsultaFila.Business
{
    public class ItemFilaBusiness
    {
        HttpClient client = new HttpClient();
        public ItemFilaBusiness()
        {
            client.BaseAddress = new Uri("https://localhost:44362");//localhost

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<ItemFila> BuscaItemFila()
        {
            ItemFila ret = new ItemFila();
            try
            {
                HttpResponseMessage response = await client.GetAsync("/Fila/GetItemFila");//endereço para acessar
                if (response.IsSuccessStatusCode)
                {
                    var dados = await response.Content.ReadAsStringAsync();

                    if (dados != "\"Nenhum item na fila\"")
                    {
                        ret = JsonConvert.DeserializeObject<ItemFila>(dados);
                        return ret;
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return ret;
        }
        internal List<DadosMoeda> DadosMoeda(ItemFila itemFila)
        {
            bool retData = false;

            List<DadosMoeda> RetMoedaData = new List<DadosMoeda>();
            DadosMoeda dadosMoeda = new DadosMoeda();
            int cont = 0;
            using (StreamReader sr = File.OpenText(@".\Arquivos\DadosMoeda.csv"))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (cont != 0)
                    {
                        var Separador = s.Split(";");
                        var IdMoeda = Separador[0];
                        var DataArquivo = Separador[1];
                        if (retData = ComparaDatas(DataArquivo, itemFila.Data_Inicio, itemFila.Data_Fim))
                        {
                            dadosMoeda.IdMoeda = IdMoeda;
                            dadosMoeda.DataRef = DataArquivo;
                            RetMoedaData.Add(dadosMoeda);
                            dadosMoeda = new DadosMoeda();
                        }
                    }
                    cont += 1;
                }
            }
            return RetMoedaData;

        }

        internal List<string> ConcatenaListas(List<DadosMoeda> ListaDadosMoeda, List<DePara> ListarDePara, List<DadosCotacao> ListaDadosCotacao)
        {
            List<string> ListaRetorno = new List<string>();

            foreach (var dadosMoeda in ListaDadosMoeda)
            {
                var IdMoeda = dadosMoeda.IdMoeda;
                var Data = Convert.ToDateTime(dadosMoeda.DataRef);
                var DataRef = Data.ToShortDateString();

                var CodCotacao = ListarDePara.Where(c => c.IdMoeda == IdMoeda).Select(d => d.CodCotacao).FirstOrDefault();

                string ValorCotacao = ListaDadosCotacao
                    .Where(c => c.CodCotacao == Convert.ToInt32(CodCotacao) && c.DataCotacao == DataRef)
                    .Select(d => d.ValorCotacao).FirstOrDefault();

                ListaRetorno.Add(IdMoeda + ";" + DataRef + ";" + ValorCotacao  );
            }

            return ListaRetorno;
        }
        internal List<DadosCotacao> DadosCotacao()
        {
            List<DadosCotacao> ListaDadosCotacao = new List<DadosCotacao>();
            DadosCotacao DadosCotacao = new DadosCotacao();

            int cont = 0;
            using (StreamReader sr = File.OpenText(@".\Arquivos\DadosCotacao.csv"))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (cont != 0)
                    {
                        var Separador = s.Split(";");
                        DadosCotacao.ValorCotacao = Separador[0];
                        DadosCotacao.CodCotacao = Convert.ToInt32(Separador[1]);
                        DadosCotacao.DataCotacao = Separador[2];

                        ListaDadosCotacao.Add(DadosCotacao);

                        DadosCotacao = new DadosCotacao();
                    }
                    cont += 1;
                }
            }

            return ListaDadosCotacao;
        }

        internal List<DePara> DadosDePara()
        {
            List<DePara> ListaDePara = new List<DePara>();
            DePara DePara = new DePara();
            int cont = 0;
            using (StreamReader sr = File.OpenText(@".\Arquivos\DePara.csv"))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (cont != 0)
                    {
                        var Separador = s.Split(";");
                        DePara.IdMoeda = Separador[0];
                        DePara.CodCotacao = Separador[1];
                        ListaDePara.Add(DePara);

                        DePara = new DePara();

                    }
                    cont += 1;
                }
            }


            return ListaDePara;
        }
        internal bool ComparaDatas(string dataArquivo, string dataInicio, string dataFim)
        {
            DateTime dtArquivo = Convert.ToDateTime(dataArquivo);
            DateTime dtinicio = Convert.ToDateTime(dataInicio);
            DateTime dtFim = Convert.ToDateTime(dataFim);

            if (dtArquivo >= dtinicio && dtArquivo <= dtFim)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        internal void GerarArquivo(IList<string> ListaResultado)
        {

            List<string> ListPrimeiraColuna = new List<string>
            {
                "ID_MOEDA",
            };

            List<string> ListSegundaColuna = new List<string>
            {
                "DATA_REF",
            };

            List<string> ListTerceiraColuna = new List<string>
            {
                "VL_COTACAO",
            };

            List<string> SegundaColuna = new List<string>
            {
            };

            foreach (var item in ListaResultado)
            {
                var Separador = item.Split(";");
                ListPrimeiraColuna.Add(Separador[0]);
                ListSegundaColuna.Add(Separador[1]);
                ListTerceiraColuna.Add(Separador[2]);
            }

            List<string> combined = new List<string>();
            int count = ListPrimeiraColuna.Count >= SegundaColuna.Count ? ListPrimeiraColuna.Count : SegundaColuna.Count;
            for (int i = 0; i < count; i++)
            {
                string firstColumn = ListPrimeiraColuna.Count <= i ? "" : ListPrimeiraColuna[i];
                string secondColumn = ListSegundaColuna.Count <= i ? "" : ListSegundaColuna[i];
                string thirdColumn = ListTerceiraColuna.Count <= i ? "" : ListTerceiraColuna[i];

                combined.Add(string.Format("{0} {1} {2}", firstColumn, secondColumn, thirdColumn));
            }
            string path = @"C:\\ResultadoItemFila";
            CriarDiretorio(path);

            string dataNome = DataNomeResultado();

            path = path + "\\Resultado"+ dataNome + ".csv";
            File.WriteAllLines(path, combined);
            
        }
        internal string DataNomeResultado()
        {
            string dataResultado;
            dataResultado = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();

            return dataResultado;
        }
        internal void CriarDiretorio(string path)
        {
            
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {

            }
        }
        internal void GerarArquivoTeste()
        {
            List<string> ListPrimeiraColuna = new List<string>
            {
                "ID_MOEDA",
            };

            List<string> ListSegundaColuna = new List<string>
            {
                "DATA_REF",
            };

            List<string> ListTerceiraColuna = new List<string>
            {
                "VL_COTACAO",
            };

            List<string> SegundaColuna = new List<string>
            {
            };
            ListPrimeiraColuna.Add("USD");
            ListPrimeiraColuna.Add("BLR");
            ListSegundaColuna.Add("22/08/1982");
            ListSegundaColuna.Add("22/09/2015");
            ListTerceiraColuna.Add("5,123");
            ListTerceiraColuna.Add("3,215");

            List<string> combined = new List<string>();
            int count = ListPrimeiraColuna.Count >= SegundaColuna.Count ? ListPrimeiraColuna.Count : SegundaColuna.Count;
            for (int i = 0; i < count; i++)
            {
                string firstColumn = ListPrimeiraColuna.Count <= i ? "" : ListPrimeiraColuna[i];
                string secondColumn = ListSegundaColuna.Count <= i ? "" : ListSegundaColuna[i];
                string thirdColumn = ListTerceiraColuna.Count <= i ? "" : ListTerceiraColuna[i];

                //string secondColumn = SegundaColuna.Count <= i ? "" : SegundaColuna[i].ToString();

                //if (firstColumn.Length > 20)
                //{
                //    //truncate rest of the values
                //    firstColumn = firstColumn.Substring(0, 20);
                //}
                //else
                //{
                //    firstColumn = firstColumn + new string(' ', 20 - firstColumn.Length);
                //}
                combined.Add(string.Format("{0} {1} {2}", firstColumn, secondColumn, thirdColumn));
            }

            



            
        }
        public class Foo
        {
            // obviously you find meaningful names of the 2 properties

            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
        }
    }
}
