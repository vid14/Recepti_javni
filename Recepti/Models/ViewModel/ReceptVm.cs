using Microsoft.AspNetCore.Mvc.Rendering;

namespace Recepti.Models.ViewModel
{
    public class ReceptVm
    {
        /// <summary>
        /// Ime recepta tipa Snicli
        /// </summary>
        public string ImeRecepta { get; set; }
        
        public int ReceptId { get; set; }
        
        public List<SastojakZaRecept> ReceptSastojak { get; set; }
        
        public List<SelectListItem> ListaSastojaka { get; set; }

        /// <summary>
        /// Opis sto napraviti sa sastojcima da bi ispao kolac ili neko jelo
        /// </summary>
        public string DetaljanOpis { get; set; }
        
        /// <summary>
        /// Neka napomena za recept tipa moras paziti na nesto
        /// </summary>
        public string? Napomena { get; set; }
        
        /// <summary>
        /// Sve kategorije u koje spada recept, npr sladoled, ljeto, osvježenje, kolač, ...
        /// </summary>
        public string Kategorije { get; set; }
        
        /// <summary>
        /// Slika originalnog recepta, u slucaju da se recept prepisuje iz neke knjige, moguce su greske zbog ne citljivog rukopisa
        /// </summary>
        public List<IFormFile> SlikaRecepta { get; set; }
        
        public List<Slike> SlikeReceptaZaBrisanje { get; set; }
    }

    /// <summary>
    /// Koristi se za stvaranje novog recepta
    /// </summary>
    public class SastojakZaRecept
    {
        public int IdSastojka { get; set; }
        public float? Kolicina { get; set; }
    }

    public class Slike
    {
        public int IdSlike { get; set; }
        public byte[] Slika { get; set; }
        public bool Obrisati { get; set; }
    }
}
