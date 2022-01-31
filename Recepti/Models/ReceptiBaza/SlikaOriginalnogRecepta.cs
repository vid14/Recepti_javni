using System.ComponentModel.DataAnnotations;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Slika nekog recepta ukoliko je tesko za procitati pa da se i na taj nacin zapamti
    /// </summary>
    public class SlikaOriginalnogRecepta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public byte[] Slika { get; set; }

        [Required]
        public int ReceptId { get; set; }
    }
}
