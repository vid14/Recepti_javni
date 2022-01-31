namespace Recepti.Paginacija
{
    public class PaginacijaInfo
    {
        public int SviRedovi { get; set; }

        public int RedoviPoStranici { get; set; }
        public int TrenutnaStranica { get; set; }

        public int sveStranice => (int)Math.Ceiling((decimal)SviRedovi / RedoviPoStranici);

        public string urlParameter { get; set; }
    }
}
