using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VoziBa.Controllers
{
    public class KalkulatorController : Controller
    {
        private readonly ApplicationDbContext _context; // Za pristup bazi podataka

        public KalkulatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kalkulators/Index
        public async Task<IActionResult> Index()
        {
            var vozila = await _context.Vozilo.ToListAsync();
            return View(vozila);
        }

        
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> IzracunajTrosak(int voziloId, int brojDanaRezervacije)
        {
            var vozilo = await _context.Vozilo
                                       .FirstOrDefaultAsync(v => v.voziloId == voziloId);

            if (vozilo == null)
            {
                TempData["ErrorMessage"] = "Odabrano vozilo nije pronađeno.";
                return RedirectToAction(nameof(Index));
            }

            if (brojDanaRezervacije <= 0)
            {
                TempData["ErrorMessage"] = "Broj dana rezervacije mora biti veći od nule.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Algoritam za računanje (smješten ovdje, u kontroleru)
            double ukupanTrosak = vozilo.cijenaNajma * brojDanaRezervacije;
            if (brojDanaRezervacije >= 10) ukupanTrosak -= 0.1 * ukupanTrosak;
            if (brojDanaRezervacije >= 20) ukupanTrosak -= 0.2 * ukupanTrosak;

            // 3. Vraćanje rezultata na View
            ViewBag.UkupanTrosak = ukupanTrosak;
            ViewBag.OdabranoVozilo = vozilo.model;
            ViewBag.BrojDana = brojDanaRezervacije;

            // Ponovo dohvati listu vozila za prikaz forme sa rezultatom
            var vozila = await _context.Vozilo.ToListAsync();
            return View("Index", vozila); // Vraćamo Index View, ali s popunjenim rezultatima (vraca se view Kalkulator)
        }
    }
}