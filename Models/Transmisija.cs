using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public enum Transmisija
    {
        [Display(Name = "Automatik")]
        automatik,

        [Display(Name = "Manuelni")]
        manuelni
    }
}
