using System.ComponentModel.DataAnnotations;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Sastojak nekog recepta tipa jaje, brasno, itd
    /// </summary>
    public class Sastojak
    {
        [Key]
        public int SastojakId { get; set; } 

        /// <summary>
        /// Ime sastojka, npr jaje, žumanjak, brašno, itd
        /// </summary>
        [Required] 
        public string Ime { get; set; }

        /// <summary>
        /// Mjerna jedinica
        /// </summary>
        [Required]
        public int MjernaJedinicaId { get; set; }
    }
}
