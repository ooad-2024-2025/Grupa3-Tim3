using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public enum Brend
    {
        [Display(Name = "Volkswagen")]
        Volkswagen,

        [Display(Name = "Toyota")]
        Toyota,

        [Display(Name = "Opel")]
        Opel,

        [Display(Name = "Hyundai")]
        Hyundai,

        [Display(Name = "Audi")]
        Audi,

        [Display(Name = "BMW")]
        BMW,

        [Display(Name = "Mercedes-Benz")]
        Mercedes
    }

}
