


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace VoziBa.Models
{
    public class Vozilo
    {
        [Key]
        public int voziloId { get; set; }

        [ForeignKey("Korisnik")]
        public int korisnikId { get; set; }
        public int godinaProizvodnje { get; set; }
        public Brend brend { get; set; }
        public string model { get; set; }
        public string boja { get; set; }
        public Gorivo tipGoriva { get; set; }
        public Transmisija transmisija { get; set; }
        public double cijenaNajma { get; set; }
        public string opis { get; set; }
        public string slikaPath { get; set; }//cuva se path do slike, a ona se cuva u VoziBa\wwwroot\images\
        public Grad grad { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}