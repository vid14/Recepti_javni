using Recepti.Models.ReceptiBaza;

namespace Recepti.Models.ViewModel;

public class DetaljiReceptaVm
{
    public int IdRecepta { get; set; }
    public string ImeRecepta { get; set; }
    public string OpisRecepta { get; set; }
    public string NapomenaRecepta { get; set; }
    public List<string> SlikaRecepta { get; set; }
    public string KategorijeRecepta { get; set; }
    public List<Sastojci_u_Receptu> Sastojci { get; set; }
}

public class Sastojci_u_Receptu
{
    public string Ime { get; set; }
    public string MjernaJedinica { get; set; }
    public string Kolicina { get; set; }
}