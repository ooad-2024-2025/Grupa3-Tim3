namespace VoziBa.Models
{
    public class VlasnikRezervacijaViewModel
    {
        public int RezervacijaID { get; set; }
        public string VoziloModel { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumZavrsetka { get; set; }
        public DateTime DatumKreiranja { get; set; }

        // Podaci o korisniku koji je napravio rezervaciju
        public string KorisnikImePrezime { get; set; }
        public string KorisnikBrojTelefona { get; set; }

        // Status će biti deduciran ili pretpostavljen jer nema Status polja u modelu
        public string StatusTekst { get; set; } // Uglavnom će biti "Na čekanju" za aktivne rezervacije
        public string StatusIkonaKlasa { get; set; }

    }
}