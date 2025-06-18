using System;
// Nema potrebe za using VoziBa.Models; ako ne referenciramo enume direktno

namespace VoziBa.Models
{
    public class KorisnikRezervacijaViewModel
    {
        public int RezervacijaID { get; set; }
        public string VoziloModel { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumZavrsetka { get; set; }
        public DateTime DatumKreiranja { get; set; }

        // Podaci o vlasniku vozila
        public string VlasnikImePrezime { get; set; }
        public string VlasnikBrojTelefona { get; set; }

        // Status će biti deduciran ili pretpostavljen jer nema Status polja u modelu
        public string StatusTekst { get; set; }
        public string StatusIkonaKlasa { get; set; } // Za Font Awesome ikone i boje
    }
}