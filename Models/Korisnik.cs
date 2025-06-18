using System.ComponentModel.DataAnnotations;
using System.Drawing;
using VoziBa.ValidationAttributes;

namespace VoziBa.Models
{
    public class Korisnik
    {
        [Key]
        public int korisnikId { get; set; }

        [Required(ErrorMessage = "Korisničko ime je obavezno polje.")]
        public string username { get; set; }

        [Required(ErrorMessage = "Ime je obavezno polje.")]
        public string ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno polje.")]
        public string prezime { get; set; }

        [Required(ErrorMessage = "Datum rođenja je obavezno polje.")]
        [MinimumAge(18, ErrorMessage = "Morate imati najmanje 18 godina da biste se registrovali.")]
        public string datumRodjenja { get; set; }

        [Required(ErrorMessage = "Email je obavezno polje.")]
        [EmailAddress(ErrorMessage = "Email adresa nije u ispravnom formatu.")]
        public string email { get; set; }

        [Required(ErrorMessage = "Broj telefona je obavezno polje.")]
        [RegularExpression(@"^(06[0-689])(?:[ -]?)(\d{3})(?:[ -]?)(\d{3,4})$", ErrorMessage = "Broj telefona nije u ispravnom formatu (npr. 06x xxx xxx ili 06x xxx xxx x).")]
        public string brojTelefona { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezno polje.")]
        [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera.")]
        [RegularExpression(@"^(?=.*\d).{8,}$", ErrorMessage = "Lozinka mora imati najmanje 8 karaktera i barem jedan broj.")]
        public string lozinka { get; set; }

        [Required(ErrorMessage = "Uloga je obavezno polje.")]
        public Uloga uloga { get; set; }

        
    }

}
