using RS1_Ispit_asp.net_core.EntityModels;
using System;
using System.Collections.Generic;

namespace RS1_Ispit_asp.net_core.ViewModels
{
    public class testVM
    {
        public int SkolaId { get; set; }
        public List<Skola> Skole { get; set; }
        public int Razred { get; set; }
        public List<short> Razredi { get; set; }
        public int TakmicenjeId { get; set; }
        public string Skola { get; set; }
        public DateTime Datum { get; set; }
        public string Predmet { get; set; }
        public string NajboljiUcenik { get; set; }
    }
}
