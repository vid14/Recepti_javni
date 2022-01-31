using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recepti.Models.ReceptiBaza;
using Recepti.Models.ViewModel;
using Recepti.Paginacija;

namespace Recepti.Controllers
{
    public class KategorijaReceptaController : Controller
    {
        private readonly ReceptiContext _context;
        private const int brojRedaka = 20;

        public KategorijaReceptaController(ReceptiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista kagegorija u kojima moze biti neki recept
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(string kategorija, int stranica = 1)
        {
            var kat = await (from k in _context.KategorijaRecepta
                where string.IsNullOrEmpty(kategorija) || k.KategorijaIme.Contains(kategorija)
                orderby k.KategorijaIme
                select k).ToListAsync();
            int ukupanBrRedaka = kat.Count;

            kat = kat.OrderBy(o => o.KategorijaIme)
                .Skip((stranica - 1) * brojRedaka) // Preskoci broj redaka
                .Take(brojRedaka).ToList(); // Uzmi sljedeci broj zapisa

            PaginacijaInfo paginacijaInfo = new PaginacijaInfo
            {
                TrenutnaStranica = stranica,
                RedoviPoStranici = brojRedaka,
                SviRedovi = ukupanBrRedaka,
                urlParameter = "?stranica=:&kategorija=" + kategorija

            };
            ViewBag.PaginacijaInfo = paginacijaInfo;
            return View(kat);
        }

        /// <summary>
        /// Detalji recepta sa svim receptima koji se nalaze unutar te kategorije
        /// </summary>
        /// <param name="id">Id kategorije iz baze</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id, int  stranica = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategorijaRecepta = await _context.KategorijaRecepta.Where(w=>w.KategorijaId == id).Select(m => new KategorijaVm()
            {
                KategorijaId = m.KategorijaId,
                KategorijaIme = m.KategorijaIme
            }).FirstOrDefaultAsync();
            
            if (kategorijaRecepta == null)
            {
                return NotFound();
            }

            kategorijaRecepta.ListaRecepata = await (from k in _context.KategorijaRecepta
                join rk in _context.ReceptKategorija on k.KategorijaId equals rk.KategorijaId
                join r in _context.Recept on rk.ReceptId equals r.ReceptId
                where k.KategorijaId == id
                select r).ToListAsync();
            
            int ukupanBrRedaka = kategorijaRecepta.ListaRecepata.Count;

            kategorijaRecepta.ListaRecepata = kategorijaRecepta.ListaRecepata.OrderBy(o => o.ImeRecepta)
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

            return View(kategorijaRecepta);
        }

        /// <summary>
        /// Dodavanje nove kategorije u koju moze spadati neki recept
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KategorijaId,KategorijaIme")] KategorijaRecepta kategorijaRecepta)
        {
            if (ModelState.IsValid)
            {
                kategorijaRecepta.KategorijaIme = kategorijaRecepta.KategorijaIme.ToLower();
                _context.Add(kategorijaRecepta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kategorijaRecepta);
        }

        /// <summary>
        /// Uredivanje kategorije, primarno uredivanje imena za sada
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategorijaRecepta = await _context.KategorijaRecepta.FindAsync(id);
            if (kategorijaRecepta == null)
            {
                return NotFound();
            }
            return View(kategorijaRecepta);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KategorijaId,KategorijaIme")] KategorijaRecepta kategorijaRecepta)
        {
            if (id != kategorijaRecepta.KategorijaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategorijaRecepta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategorijaReceptaExists(kategorijaRecepta.KategorijaId))
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
            return View(kategorijaRecepta);
        }

        /// <summary>
        /// Brisanje kategorije
        /// </summary>
        /// <param name="id">Id iz baze</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategorijaRecepta = await _context.KategorijaRecepta.FirstOrDefaultAsync(m => m.KategorijaId == id);
            if (kategorijaRecepta == null)
            {
                return NotFound();
            }
            
            // Provjera koristi li se kategorija u nekom od recepata
            var recepti = await (from k in _context.KategorijaRecepta
                join rk in _context.ReceptKategorija on k.KategorijaId equals rk.KategorijaId
                where k.KategorijaId == id
                select rk).ToListAsync();
            if (recepti.Count > 0)
            {
                TempData["Greska"] = "Nije moguce obrisati sastojak koji se koristi u nekom receptu.";
                return RedirectToAction(nameof(Index));
            }

            return View(kategorijaRecepta);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategorijaRecepta = await _context.KategorijaRecepta.FindAsync(id);
            _context.KategorijaRecepta.Remove(kategorijaRecepta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KategorijaReceptaExists(int id)
        {
            return _context.KategorijaRecepta.Any(e => e.KategorijaId == id);
        }
    }
}
