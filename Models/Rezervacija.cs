


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoziBa.Models
{
    public class Rezervacija
    {
        [Key]
        public int rezervacijaID { get; set; }

        [ForeignKey("Korisnik")]
        public int osobaId { get; set; }

        [ForeignKey("Recenzija")]
        public int recenzijaId { get; set; }

        [ForeignKey("Vozilo")]
        public int voziloID { get; set; }

        [Required(ErrorMessage = "Datum pocetka rezervacije je obavezan. ")]
        public DateTime datumPocetka { get; set; }

        [Required(ErrorMessage = "Datum zavrsetka rezervacije je obavezan.")]
        public DateTime datumZavrsetka { get; set; }

        public DateTime datumKreiranja { get; set; }
        public Boolean potvrda { get; set; }
    }
}