using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Recepti.Models.ReceptiBaza;
using Recepti.Models.ViewModel;
using Recepti.Paginacija;

namespace Recepti.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReceptiContext _rc;

        private const int brojRedaka = 20;

        public HomeController(ReceptiContext rc)
        {
            _rc = rc;
        }

        /// <summary>
        /// Paginarana lista recepata slozena po abecedi
        /// Mogucnost naprednog pretrazivanja po kategorijama i sastojcima, te cak i kolicini pojedinog sastojka
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(string ime, string kategorije, string sastojci, int stranica = 1)
        {
            var recept = (from r in _rc.Recept
                where string.IsNullOrEmpty(ime) || r.ImeRecepta.Contains(ime)
                select new ListaRecepataVm()
                {
                    ImeRecepta = r.ImeRecepta,
                    ReceptId = r.ReceptId
                }).OrderBy(o => o.ImeRecepta).ToList();
            
            foreach (var item in recept)
            {
                var kat = (from rk in _rc.ReceptKategorija
                    join k in _rc.KategorijaRecepta on rk.KategorijaId equals k.KategorijaId
                    where rk.ReceptId == item.ReceptId
                    orderby k.KategorijaIme
                    select k.KategorijaIme).ToList();
                foreach (var kategorijice in kat)
                {
                    item.KategorijaRecepta += kategorijice + "; ";
                }
            }

            if (!string.IsNullOrEmpty(kategorije))
            {
                // Izbaci recepte koji nemaju neku od ovih kategorija
                IList<string> kategorijeList = kategorije.Split(';').Reverse().ToList<string>();

                foreach (var item in recept.ToList())
                {
                    bool imaJedan = false;
                    foreach (var kl in kategorijeList)
                    {
                        if (item.KategorijaRecepta.Contains(kl))
                        {
                            imaJedan = true;
                        }
                    }

                    if (!imaJedan)
                    {
                        recept.Remove(item);
                    }

                    if (!recept.Any())
                    {
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(sastojci))
            {
                // Izbaci recepte koji nemaju neki od ovaj sastojaka
                IList<string> sastojciList = sastojci.Split(';').Reverse().ToList<string>();
                foreach (var item in recept.ToList())
                {
                    bool imaJedan = false;
                    foreach (var sl in sastojciList)
                    {
                        var imaSastojak = (from rs in _rc.ReceptSastojci
                            join s in _rc.Sastojak on rs.SastojakId equals s.SastojakId
                            where rs.ReceptId == item.ReceptId && s.Ime.Contains(sl)
                            select rs).Any();

                        if (imaSastojak)
                        {
                            imaJedan = true;
                        }
                    }

                    if (!imaJedan)
                    {
                        recept.Remove(item);
                    }

                    if (!recept.Any())
                    {
                        break;
                    }
                }
            }

            int ukupanBrRedaka = recept.Count;

            recept = recept.OrderBy(o => o.ImeRecepta)
                .Skip((stranica - 1) * brojRedaka) // Preskoci broj redaka
                .Take(brojRedaka).ToList(); // Uzmi sljedeci broj zapisa

            PaginacijaInfo paginacijaInfo = new PaginacijaInfo
            {
                TrenutnaStranica = stranica,
                RedoviPoStranici = brojRedaka,
                SviRedovi = ukupanBrRedaka,
                urlParameter = "?stranica=:&ime=" + ime + "&kategorije=" + kategorije + "&sastojci=" + sastojci

            };
            ViewBag.PaginacijaInfo = paginacijaInfo;
            ViewBag.UkupnoRecepata = ukupanBrRedaka;
            
            return View(recept);
        }

        #region Novi recept
        /// <summary>
        /// Dodavanje novog recepta
        /// </summary>
        /// <returns></returns>
        public IActionResult NoviRecept()
        {
            var rec = new ReceptVm()
            {
                ListaSastojaka = DohvatiListuSastojakaZaDropdown()
            };
            return View(rec);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NoviRecept(ReceptVm recept)
        {
            if (string.IsNullOrEmpty(recept.ImeRecepta) || string.IsNullOrEmpty(recept.DetaljanOpis) || recept.ReceptSastojak.First().IdSastojka == -1)
            {
                ViewBag.Greska = "Neko od polja nije ispunjeno koje je obavezno";
                return View(recept);
            }
            
            // Spremi recept u bazu
            var rec = new Recept
            {
                ImeRecepta = recept.ImeRecepta,
                DetaljanOpis = recept.DetaljanOpis,
                Napomena = recept.Napomena ?? "",
                VrijemeDodavanja = DateTime.Now
            };
            _rc.Add(rec);
            _rc.SaveChanges();

            // Popuni recept sa sastojcima recepta
            DodavanjeSastojakaNaRecept(recept, rec.ReceptId);
            
            // Dodavanje kategorije na recept
            DodavanjeKategorijaNaRecept(recept, rec.ReceptId);
            
            // Spremanje slika recepta u bazu
            DodavanjeSlikaNaRecept(recept, rec.ReceptId);
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Dodaje sastojke recepta u bazu vezane na recept
        /// </summary>
        /// <param name="recept"></param>
        /// <param name="receptId"></param>
        private void DodavanjeSastojakaNaRecept(ReceptVm recept, int receptId)
        {
            foreach (var item in recept.ReceptSastojak)
            {
                if (item.IdSastojka == -1)
                {
                    break;
                }
                
                // Trebamo mjernu jedinicu da znamo spojiti u bazi
                var mjernaJedinicaId = _rc.Sastojak.Where(w => w.SastojakId == item.IdSastojka).Select(s => s.MjernaJedinicaId).FirstOrDefault();
                var rs = new ReceptSastojci
                {
                    ReceptId = receptId,
                    SastojakId = item.IdSastojka,
                    MjernaJedinicaId = mjernaJedinicaId,
                    Vrijednost = item.Kolicina ?? 0
                };
                _rc.Add(rs);
                _rc.SaveChanges();
            }
        }

        /// <summary>
        /// Dodaje kategorije recepta u bazu vezane na recept
        /// </summary>
        /// <param name="recept"></param>
        /// <param name="receptId"></param>
        private void DodavanjeKategorijaNaRecept(ReceptVm recept, int receptId)
        {
            IList<string> kategorije = recept.Kategorije.Split(';').Reverse().ToList<string>();
            foreach (var item in kategorije)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                var trimmed = item.Trim();
                var katPostoji = _rc.KategorijaRecepta.FirstOrDefault(w => w.KategorijaIme.ToLower() == trimmed.ToLower());
                if (katPostoji is null)
                {
                    // Dodaj novu kategoriju recepta, a spoji recept s kategorijom
                    var k = new KategorijaRecepta
                    {
                        KategorijaIme = trimmed
                    };
                    _rc.Add(k);
                    _rc.SaveChanges();

                    var rk = new ReceptKategorija
                    {
                        ReceptId = receptId,
                        KategorijaId = k.KategorijaId
                    };
                    _rc.Add(rk);
                    _rc.SaveChanges();
                }
                else
                {
                    // Samo spoji recept s kategorijom
                    var rk = new ReceptKategorija()
                    {
                        ReceptId = receptId,
                        KategorijaId = katPostoji.KategorijaId
                    };
                    _rc.Add(rk);
                    _rc.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Dodaje slike recepta u bazu vezane na recept
        /// </summary>
        /// <param name="recept"></param>
        /// <param name="receptId"></param>
        private void DodavanjeSlikaNaRecept(ReceptVm recept, int receptId)
        {
            if (recept.SlikaRecepta != null && recept.SlikaRecepta.Count > 0)
            {
                foreach (var item in recept.SlikaRecepta)
                {
                    using (var ms = new MemoryStream())
                    {
                        item.CopyTo(ms);
                        var slikaByte = ms.ToArray();
                        var sr = new SlikaOriginalnogRecepta()
                        {
                            ReceptId = receptId,
                            Slika = slikaByte
                        };
                        _rc.Add(sr);
                        _rc.SaveChanges();
                    }
                }
            }
        }
        #endregion
        
        #region Detalji recepta
        /// <summary>
        /// Prikaz detalja pojedinog recepta
        /// </summary>
        /// <param name="receptId"></param>
        /// <returns></returns>
        public IActionResult DetaljiRecepta(int receptId)
        {
            var viewModel = new DetaljiReceptaVm();
            var receptBaza = _rc.Recept.FirstOrDefault(w => w.ReceptId == receptId);
            if (receptBaza is null)
            {
                TempData["Poruka"] = "Pogresan id recepta.";
                return RedirectToAction(nameof(Index));
            }

            viewModel.ImeRecepta = receptBaza.ImeRecepta;
            viewModel.IdRecepta = receptId;
            viewModel.OpisRecepta = receptBaza.DetaljanOpis;
            viewModel.NapomenaRecepta = receptBaza.Napomena;
            
            var sastojciBaza = _rc.ReceptSastojci.Where(w => w.ReceptId == receptId).ToList();
            var kategorijeBaza = _rc.ReceptKategorija.Where(w => w.ReceptId == receptId).ToList();
            var slikaRecepta = _rc.SlikaOriginalnogRecepta.Where(w => w.ReceptId == receptId).ToList();

            // Dodaj sastojek za detaljan prikaz
            viewModel.Sastojci = new List<Sastojci_u_Receptu>();
            foreach (var item in sastojciBaza)
            {
                var sastojak = _rc.Sastojak.Where(w => w.SastojakId == item.SastojakId).FirstOrDefault();
                viewModel.Sastojci.Add(new Sastojci_u_Receptu
                {
                    Ime = sastojak.Ime,
                    MjernaJedinica = _rc.MjernaJedinica.Where(w=>w.MjernaJedinicaId == sastojak.MjernaJedinicaId).Select(s=>s.JedinicaSkraceno).FirstOrDefault(),
                    Kolicina = item.Vrijednost.ToString()
                });
            }
            
            // Dodaj kategorije
            viewModel.KategorijeRecepta = "";
            var zadnjaKategorija = kategorijeBaza.Last();
            foreach (var item in kategorijeBaza)
            {
                var kategorijaIme = _rc.KategorijaRecepta.Where(w => w.KategorijaId == item.KategorijaId).Select(s => s.KategorijaIme).FirstOrDefault();
                viewModel.KategorijeRecepta += kategorijaIme;
                if (item != zadnjaKategorija)
                {
                    viewModel.KategorijeRecepta += "; ";
                }
            }
            
            // Dodaj slike recepta
            viewModel.SlikaRecepta = new List<string>();
            foreach (var item in slikaRecepta)
            {
                viewModel.SlikaRecepta.Add(Convert.ToBase64String(item.Slika));
            }
            return View(viewModel);
        }
        #endregion

        #region Uredi recept
        public IActionResult UrediRecept(int idRecepta)
        {
            var rec = (from r in _rc.Recept
                where r.ReceptId == idRecepta
                select new ReceptVm()
                {
                    ReceptId = idRecepta,
                    ImeRecepta = r.ImeRecepta,
                    DetaljanOpis = r.DetaljanOpis,
                    Napomena = r.Napomena
                }).FirstOrDefault();
            rec.ListaSastojaka = new List<SelectListItem>();
            rec.ListaSastojaka = DohvatiListuSastojakaZaDropdown();

            // Sastojci recepta
            rec.ReceptSastojak = (from sas in _rc.ReceptSastojci
                where sas.ReceptId == idRecepta
                select new SastojakZaRecept()
                {
                    IdSastojka = sas.SastojakId,
                    Kolicina = (float)sas.Vrijednost
                }).ToList();
            while (rec.ReceptSastojak.Count < 10)
            {
                rec.ReceptSastojak.Add(new SastojakZaRecept()
                {
                    IdSastojka = -1
                });
            }

            // Slike recepta
            rec.SlikeReceptaZaBrisanje = (from sr in _rc.SlikaOriginalnogRecepta
                where sr.ReceptId == idRecepta
                select new Slike()
                {
                    IdSlike = sr.Id,
                    Slika = sr.Slika
                }).ToList();
            
            // Dodaj kategorije
            var kategorijeBaza = _rc.ReceptKategorija.Where(w => w.ReceptId == rec.ReceptId).ToList();
            rec.Kategorije = "";
            var zadnjaKategorija = kategorijeBaza.Last();
            foreach (var item in kategorijeBaza)
            {
                var kategorijaIme = _rc.KategorijaRecepta.Where(w => w.KategorijaId == item.KategorijaId).Select(s => s.KategorijaIme).FirstOrDefault();
                rec.Kategorije += kategorijaIme;
                if (item != zadnjaKategorija)
                {
                    rec.Kategorije += "; ";
                }
            }
            
            return View(rec);
        }

        [HttpPost]
        public IActionResult UrediRecept(ReceptVm rec)
        {
            // Update recepata
            var recept = _rc.Recept.FirstOrDefault(f => f.ReceptId == rec.ReceptId);
            if (recept is not null)
            {
                recept.ImeRecepta = rec.ImeRecepta;
                recept.DetaljanOpis = rec.DetaljanOpis;
                recept.Napomena = rec.Napomena ?? "";
                _rc.Recept.Update(recept);
                _rc.SaveChanges();
            }

            // Dodavanje, oduzimanje i mijenjanje kolicine sastojaka
            var receptSastojciStari = _rc.ReceptSastojci.Where(w => w.ReceptId == rec.ReceptId).ToList();
            if (receptSastojciStari.Any())
            {
                foreach (var item in receptSastojciStari)
                {
                    _rc.ReceptSastojci.Remove(item);
                    _rc.SaveChanges();
                }
            }
            if (rec.ReceptSastojak.Any())
            {
                DodavanjeSastojakaNaRecept(rec, rec.ReceptId);
            }

            // Dodavanje kategorija
            var sveKategorijeRecepta = _rc.ReceptKategorija.Where(w => w.ReceptId == rec.ReceptId).ToList();
            if (sveKategorijeRecepta.Any())
            {
                foreach (var item in sveKategorijeRecepta)
                {
                    _rc.ReceptKategorija.Remove(item);
                    _rc.SaveChanges();
                }
            }
            if (!string.IsNullOrEmpty(rec.Kategorije))
            {
                DodavanjeKategorijaNaRecept(rec, rec.ReceptId);
            }

            // Dodavanje i brisanje slika
            if (rec.SlikeReceptaZaBrisanje != null && rec.SlikeReceptaZaBrisanje.Any())
            {
                foreach (var item in rec.SlikeReceptaZaBrisanje)
                {
                    if (item.Obrisati)
                    {
                        var sl = _rc.SlikaOriginalnogRecepta.FirstOrDefault(f => f.Id == item.IdSlike);
                        _rc.SlikaOriginalnogRecepta.Remove(sl);
                        _rc.SaveChanges();
                    }
                }
            }
            if (rec.SlikaRecepta != null && rec.SlikaRecepta.Any())
            {
                DodavanjeSlikaNaRecept(rec, rec.ReceptId);
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Brisanje recepta
        /// <summary>
        /// Brisanje recepta iz baze
        /// </summary>
        /// <param name="receptId"></param>
        /// <returns></returns>
        public async Task<IActionResult> ObrisiRecept(int? receptId)
        {
            if (receptId is null)
            {
                return NotFound();
            }

            var recept = await _rc.Recept.FindAsync(receptId);
            return View(recept);
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiRecept(int receptId)
        {
            var recept = await _rc.Recept.FindAsync(receptId);
            if (recept is null)
            {
                TempData["Poruka"] = "Recept ne postoji.";
                return RedirectToAction(nameof(Index));
            }
            _rc.Recept.Remove(recept);
            
            var kategorije = _rc.ReceptKategorija.Where(w => w.ReceptId == receptId).ToList();
            foreach (var item in kategorije)
            {
                _rc.ReceptKategorija.Remove(item);
            }
            var slike = _rc.SlikaOriginalnogRecepta.Where(w => w.ReceptId == receptId).ToList();
            foreach (var item in slike)
            {
                _rc.SlikaOriginalnogRecepta.Remove(item);
            }

            var sastojci = _rc.ReceptSastojci.Where(w => w.ReceptId == receptId).ToList();
            foreach (var item in sastojci)
            {
                _rc.ReceptSastojci.Remove(item);
            }

            await _rc.SaveChangesAsync();
            
            TempData["Poruka"] = "Recept uspjesno obrisan.";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        /// <summary>
        /// Dohvaca sve sastojke u bazi i trpa ih u dropdown box za pretraku i odabiranje
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> DohvatiListuSastojakaZaDropdown()
        {
            var sastojci = _rc.Sastojak.OrderBy(o=>o.Ime).ToList();
            var select = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "",
                    Value = "-1"
                }
            };
            select.AddRange(sastojci.Select(item => new SelectListItem()
            {
                Text = item.Ime + ", " + _rc.MjernaJedinica.Where(w=>w.MjernaJedinicaId == item.MjernaJedinicaId).Select(s=>s.JedinicaSkraceno).FirstOrDefault(), 
                Value = item.SastojakId.ToString()
            }));

            return select;
        }
    }
}