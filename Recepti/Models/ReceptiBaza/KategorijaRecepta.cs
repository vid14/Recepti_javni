using System.ComponentModel.DataAnnotations;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Recept spada npr u kolače, ali i božićne kolače, slatko, slano itd
    /// </summary>
    public class KategorijaRecepta
    {
        [Key]
        public int KategorijaId { get; set; }  
        [Required]
        public string KategorijaIme { get; set; }   
    }
}
