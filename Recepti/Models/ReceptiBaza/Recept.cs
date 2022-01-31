using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recepti.Models.ReceptiBaza
{
    /// <summary>
    /// Ovo je recept
    /// </summary>
    public class Recept
    {
        [Key]
        public int ReceptId { get; set; }

        [Required]
        public string ImeRecepta { get; set; }

        [Required]
        public string DetaljanOpis { get; set; }

        /// <summary>
        /// Neka napomena u vezi recepta, na primjer deti vise soli ili manje secera itd
        /// </summary>
        public string Napomena { get; set; }
        
        public DateTime VrijemeDodavanja { get; set; }
    }
}
