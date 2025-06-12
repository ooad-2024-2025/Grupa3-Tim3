using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public enum Gorivo
    {
        [Display(Name = "Dizel")]
        dizel,

        [Display(Name = "Benzin")]
        benzin,

        [Display(Name = "Električni")]
        elektricni,

        [Display(Name = "Hibrid")]
        hibrid,

        [Display(Name = "Plin")]
        plin
    }
}
