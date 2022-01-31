using Recepti.Models.ReceptiBaza;

namespace Recepti.Models.ViewModel;

public class KategorijaVm
{
    /// <summary>
    /// Id kategorije
    /// </summary>
    public int KategorijaId { get; set; }
    
    /// <summary>
    /// Ime kategorije
    /// </summary>
    public string KategorijaIme { get; set; }
    
    /// <summary>
    /// Lista recepata koji se nalaze unutar neke kategorije
    /// </summary>
    public List<Recept> ListaRecepata { get; set; }
}