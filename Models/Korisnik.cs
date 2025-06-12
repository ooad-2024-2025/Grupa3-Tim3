using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace VoziBa.Models
{
    public class Korisnik
    {
        [Key]
        public int korisnikId {  get; set; }
        public string username { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public string datumRodjenja { get; set; }
        public string email { get; set; }
        public string brojTelefona { get; set; }
        public string lozinka { get; set; }
        public Uloga uloga { get; set; }
    }
}
