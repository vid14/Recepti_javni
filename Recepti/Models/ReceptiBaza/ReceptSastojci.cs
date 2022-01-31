using System.ComponentModel.DataAnnotations;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Svi sastojci koji se stavljaju u recept
    /// </summary>
    public class ReceptSastojci
    {
        [Key]
        public int ReceptSastojciId { get; set; }
        [Required]
        public int ReceptId { get; set; }
        [Required]
        public int SastojakId { get; set; }
        [Required]
        public int MjernaJedinicaId { get; set; }
        [Required]
        public double Vrijednost { get; set; }
    }
}
