using System.ComponentModel;
using Recepti.Models.ReceptiBaza;

namespace Recepti.Models.ViewModel;

public class SastojakVm
{
    /// <summary>
    /// Id sastojka za kada se poziva uredivanje ili brisanje
    /// </summary>
    public int SastojakId { get; set; }
    
    /// <summary>
    /// Ime sastojka
    /// </summary>
    [DisplayName("Ime")]
    public string ImeSastojka { get; set; }
    
    /// <summary>
    /// Skraceno ime mjerne jedinice
    /// </summary>
    [DisplayName("Mjerna jedinica")]
    public string JedinicaSastojka { get; set; }
    
    /// <summary>
    /// Recepti u kojima se koristi ovaj sastojak
    /// </summary>
    /// <returns></returns>
    [DisplayName("Recepti")]
    public List<Recept> ListaRecepata { get; set; }
}