using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIFila.Model
{
    public class ItemFila
    {
        public string Moeda { get; set; }
        public string Data_Inicio { get; set; }
        public string Data_Fim { get; set; }
        public string MensagemRetorno { get; set; }
    }
}
