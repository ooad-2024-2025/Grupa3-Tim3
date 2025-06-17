using System.ComponentModel.DataAnnotations;
using VoziBa.ValidationAttributes; // Pretpostavljamo da je ova klasa ovdje, ako nije, izbrišite ovu liniju

namespace VoziBa.Models
{
    public class EditProfileViewModel
    {
        // ID korisnika je potreban za ažuriranje, ali ga skrivamo u formi
        public int korisnikId { get; set; }

        // Korisničko ime je obično nepromjenjivo nakon registracije ili se mijenja putem posebnog procesa.
        // Ovdje ga prikazujemo, ali ga ne dozvoljavamo za uređivanje u formi.
        [Display(Name = "Korisničko ime")]
        public string username { get; set; }

        [Required(ErrorMessage = "Ime je obavezno polje.")]
        [Display(Name = "Ime")]
        public string ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno polje.")]
        [Display(Name = "Prezime")]
        public string prezime { get; set; }

        [Required(ErrorMessage = "Datum rođenja je obavezno polje.")]
        [Display(Name = "Datum rođenja")]
        [DataType(DataType.Date)]
        [MinimumAge(18, ErrorMessage = "Morate imati najmanje 18 godina.")] // Pretpostavljamo da imate ovu validaciju
        public string datumRodjenja { get; set; }

        [Required(ErrorMessage = "Email je obavezno polje.")]
        [EmailAddress(ErrorMessage = "Email adresa nije u ispravnom formatu.")]
        [Display(Name = "Email adresa")]
        public string email { get; set; }

        [Required(ErrorMessage = "Broj telefona je obavezno polje.")]
        [RegularExpression(@"^(06[0-689])(?:[ -]?)(\d{3})(?:[ -]?)(\d{3,4})$", ErrorMessage = "Broj telefona nije u ispravnom formatu (npr. 06x xxx xxx ili 06x xxx xxx x).")]
        [Display(Name = "Broj telefona")]
        public string brojTelefona { get; set; }

        // Lozinku i ulogu NE dodajemo u ovaj ViewModel jer ih korisnik ne smije sam mijenjati.
    }
}