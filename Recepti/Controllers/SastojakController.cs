using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recepti.Models.ReceptiBaza;
using Recepti.Models.ViewModel;
using Recepti.Paginacija;

namespace Recepti.Controllers
{
    public class SastojakController : Controller
    {
        private readonly ReceptiContext _context;
        private const int brojRedaka = 20;

        public SastojakController(ReceptiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista svih sastojaka koji se mogu spajati na recepte
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(string sastojak, int stranica = 1)
        {
            var sasto = (from s in _context.Sastojak
                join mj in _context.MjernaJedinica on s.MjernaJedinicaId equals mj.MjernaJedinicaId
                where string.IsNullOrEmpty(sastojak) || s.Ime.Contains(sastojak)
                select new SastojakVm()
                {
                    SastojakId = s.SastojakId,
                    ImeSastojka = s.Ime,
                    JedinicaSastojka = mj.JedinicaSkraceno
                }).OrderBy(o=>o.ImeSastojka).ToList();
            int ukupanBrRedaka = sasto.Count;

            sasto = sasto.OrderBy(o => o.ImeSastojka)
                .Skip((stranica - 1) * brojRedaka) // Preskoci broj redaka
                .Take(brojRedaka).ToList(); // Uzmi sljedeci broj zapisa

            PaginacijaInfo paginacijaInfo = new PaginacijaInfo
            {
                TrenutnaStranica = stranica,
                RedoviPoStranici = brojRedaka,
                SviRedovi = ukupanBrRedaka,
                urlParameter = "?stranica=:&sastojak=" + sastojak

            };
            ViewBag.PaginacijaInfo = paginacijaInfo;
            return View(sasto);
        }

        /// <summary>
        /// Detalji sastojka s listom svih recepata u kojima se sastojak nalazi
        /// </summary>
        /// <param name="id">Id sastojka iz baze</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id, int stranica = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sastojak = await (from s in _context.Sastojak
                join mj in _context.MjernaJedinica on s.MjernaJedinicaId equals mj.MjernaJedinicaId
                where s.SastojakId == id
                select new SastojakVm()
                {
                    SastojakId = s.SastojakId,
                    ImeSastojka = s.Ime,
                    JedinicaSastojka = mj.JedinicaSkraceno,
                }).FirstOrDefaultAsync();

            if (sastojak == null)
            {
                return NotFound();
            }
            
            sastojak.ListaRecepata = await (from s in _context.Sastojak
                join rs in _context.ReceptSastojci on s.SastojakId equals rs.SastojakId
                join r in _context.Recept on rs.ReceptId equals r.ReceptId
                where s.SastojakId == id
                select r).ToListAsync();
            
            int ukupanBrRedaka = sastojak.ListaRecepata.Count;

            sastojak.ListaRecepata = sastojak.ListaRecepata.OrderBy(o => o.ImeRecepta)
                .Skip((stranica - 1) * brojRedaka) // Preskoci broj redaka
                .Take(brojRedaka).ToList(); // Uzmi sljedeci broj zapisa

            PaginacijaInfo paginacijaInfo = new PaginacijaInfo
            {
                TrenutnaStranica = stranica,
                RedoviPoStranici = brojRedaka,
                SviRedovi = ukupanBrRedaka,
                urlParameter = "?stranica=:&id=" + id

            };
            ViewBag.PaginacijaInfo = paginacijaInfo;
            return View(sastojak);
        }

        /// <summary>
        /// Dodavanje novog sastojka koji moze biti dio recepta
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.MjerneJedinice = DohvatMjernihJedinicaZaDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sastojak sastojak)
        {
            if (ModelState.IsValid)
            {
                sastojak.Ime = sastojak.Ime.ToLower();
                _context.Add(sastojak);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MjerneJedinice = DohvatMjernihJedinicaZaDropdown();
            return View(sastojak);
        }

        /// <summary>
        /// Promjena jedinice ili imena sastojka
        /// </summary>
        /// <param name="id">Id sastojka iz baze</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sastojak = await _context.Sastojak.FindAsync(id);
            if (sastojak == null)
            {
                return NotFound();
            }

            ViewBag.MjerneJedinice = DohvatMjernihJedinicaZaDropdown();
            return View(sastojak);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sastojak sastojak)
        {
            if (id != sastojak.SastojakId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sastojak);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SastojakExists(sastojak.SastojakId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.MjerneJedinice = DohvatMjernihJedinicaZaDropdown();
            return View(sastojak);
        }

        /// <summary>
        /// Brisanje sastojka
        /// </summary>
        /// <param name="id">Id sastojka iz baze</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sastojak = await _context.Sastojak.FirstOrDefaultAsync(m => m.SastojakId == id);
            if (sastojak == null)
            {
                return NotFound();
            }

            // Provjera koristi li se sastojak u bilo kojem receptu, ako da vrati na index s error porukom
            var koristiLiSe = _context.ReceptSastojci.Where(w => w.SastojakId == id).ToList();
            if (koristiLiSe.Count > 0)
            {
                ViewBag.Greska = "Nije moguce obrisati sastojak koji se koristi u nekom receptu.";
                return RedirectToAction(nameof(Index));
            }

            return View(sastojak);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sastojak = await _context.Sastojak.FindAsync(id);
            _context.Sastojak.Remove(sastojak);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SastojakExists(int id)
        {
            return _context.Sastojak.Any(e => e.SastojakId == id);
        }

        /// <summary>
        /// Lista mjernih jedinica za sastojke
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> DohvatMjernihJedinicaZaDropdown()
        {
            var mjerneJedinice = _context.MjernaJedinica;
            var dropdown = new List<SelectListItem>();
            foreach (var item in mjerneJedinice)
            {
                dropdown.Add(new SelectListItem()
                {
                    Text = item.JedinicaSkraceno,
                    Value = item.MjernaJedinicaId.ToString()
                });
            }

            return dropdown;
        }
    }
}