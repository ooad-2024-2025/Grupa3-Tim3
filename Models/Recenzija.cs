using System.ComponentModel.DataAnnotations;

namespace VoziBa.Models
{
    public class Recenzija
    {
        [Key]
        public int recenzijaId { get; set; }
        public Ocjena ocjena { get; set; }
        public string recenzija {  get; set; }
    }
}
