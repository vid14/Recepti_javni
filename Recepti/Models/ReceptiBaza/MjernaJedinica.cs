using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Odreduje mjernu jedinicu za pojedini sastojak
    /// </summary>
    public class MjernaJedinica
    {
        [Key]
        public int MjernaJedinicaId { get; set; }

        [Required]
        public string MjernaJedinicaIme { get; set; }

        [Required]
        public string JedinicaSkraceno { get; set; }
    }
}
