

using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace VoziBa.Models
{
    public class Recenzija
    {
        [Key]
        public int recenzijaId { get; set; }
        public Ocjena ocjena { get; set; }
        public string recenzija { get; set; }
        public string komentar { get; set; }
    }
}