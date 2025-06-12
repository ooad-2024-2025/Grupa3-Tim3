using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public enum Ocjena
    {
        [Display(Name = "1")]
        Jedan = 1,

        [Display(Name = "2")]
        Dva = 2,

        [Display(Name = "3")]
        Tri = 3,

        [Display(Name = "4")]
        Cetiri = 4,

        [Display(Name = "5")]
        Pet = 5
    }

}
