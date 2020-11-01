using RS1_Ispit_asp.net_core.EntityModels;
using System;
using System.Collections.Generic;

namespace RS1_Ispit_asp.net_core.ViewModels
{
    public class DodajTakmicenjeVM
    {
        public List<Predmet> Predmeti { get; set; }
        public int PredmetId { get; set; }
        public List<Skola> Skole { get; set; }
        public int SkolaId { get; set; }
        public DateTime Datum { get; set; }
    }
}