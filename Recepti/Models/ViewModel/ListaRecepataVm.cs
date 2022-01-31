using System.ComponentModel;

namespace Recepti.Models.ViewModel;

public class ListaRecepataVm
{
    public int ReceptId { get; set; }
    [DisplayName("Ime")]
    public string ImeRecepta { get; set; }
    [DisplayName("Kategorije")]
    public string KategorijaRecepta { get; set; }
}