using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS1_Ispit_asp.net_core.ViewModels
{
    public class UcesnikEditVM
    {
        public int TakmicenjeStavkaId { get; set; }

        public int? Bodovi { get; set; }
        public List<UcesnikVM> Ucesnici { get; set; }
    } 
    public class UcesnikVM
    {
        public int TakmicenjeStavkaId { get; set; }
        public string Text { get; set; }
    }
}
