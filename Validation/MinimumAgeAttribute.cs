using System;
using System.ComponentModel.DataAnnotations;

namespace VoziBa.ValidationAttributes
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Datum rođenja je obavezno polje.", new[] { validationContext.MemberName });
            }

            
            if (DateTime.TryParse(value.ToString(), out DateTime birthDate))
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;

                if (birthDate.Date > today.AddYears(-age))
                {
                    age--;
                }

                if (age < _minimumAge)
                {
                    return new ValidationResult(ErrorMessage ?? $"Morate imati najmanje {_minimumAge} godina da biste se registrovali.", new[] { validationContext.MemberName });
                }
            }
            else
            {
                return new ValidationResult("Datum rođenja nije u ispravnom formatu.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}