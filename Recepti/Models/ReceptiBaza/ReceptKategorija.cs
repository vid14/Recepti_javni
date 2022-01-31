using System.ComponentModel.DataAnnotations;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Sve kategorije pod koje spada neki recept
    /// </summary>
    public class ReceptKategorija
    {
        [Key]
        public int ReceptKategorijaId   { get; set; }
        [Required]
        public int KategorijaId { get; set; }
        [Required]
        public int ReceptId { get; set; }
    }
}
