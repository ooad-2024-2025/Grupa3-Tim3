using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public enum Uloga
    {
        [Display(Name = "Administrator")]
        administrator,

        [Display(Name = "Vlasnik")]
        vlasnik,

        [Display(Name = "Korisnik")]
        korisnik
    }
}
