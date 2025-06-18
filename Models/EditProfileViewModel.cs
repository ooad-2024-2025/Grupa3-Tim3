using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public class EditProfileViewModel
    {
        public int KorisnikId { get; set; }

        // --- IZMIJENJENO ---
        [Required(ErrorMessage = "Ime je obavezno.")]
        [Display(Name = "Ime")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; }
        // --- KRAJ IZMJENE ---

        [Required(ErrorMessage = "Broj telefona je obavezno polje.")]
        [RegularExpression(@"^(06[0-689])(?:[ -]?)(\d{3})(?:[ -]?)(\d{3,4})$", ErrorMessage = "Broj telefona nije u ispravnom formatu.")]
        [Display(Name = "Broj telefona")]
        public string BrojTelefona { get; set; }

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email adresa nije u ispravnom formatu.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        public string Username { get; set; }

        [Display(Name = "Stara lozinka")]
        [DataType(DataType.Password)]
        public string StaraLozinka { get; set; }

        [Display(Name = "Nova lozinka")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Nova lozinka mora imati najmanje 8 karaktera.")]
        [RegularExpression(@"^(?=.*\d).{8,}$", ErrorMessage = "Nova lozinka mora imati najmanje 8 karaktera i barem jedan broj.")]
        public string NovaLozinka { get; set; }
    }
}