using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoziBa.Models;

namespace VoziBa.Controllers
{
    public class RecenzijasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecenzijasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- POTPUNO IMPLEMENTIRANA METODA ZA KREIRANJE RECENZIJE ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ocjena,recenzija,komentar")] Recenzija novaRecenzija, [FromForm] int VoziloId)
        {
            // Korak 1: Provjeri da li je korisnik uopšte prijavljen
            if (!int.TryParse(HttpContext.Session.GetString("LoggedInUserId"), out int korisnikId))
            {
                TempData["ErrorMessage"] = "Morate biti prijavljeni da biste ostavili recenziju.";
                return RedirectToAction("Details", "Vozilos", new { id = VoziloId });
            }

            // Korak 2: Provjeri da li su podaci iz forme validni (npr. da polja nisu prazna)
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Došlo je do greške. Molimo popunite sva polja.";
                return RedirectToAction("Details", "Vozilos", new { id = VoziloId });
            }

            // Korak 3: Pronađi validnu, završenu rezervaciju za ovog korisnika i vozilo, koja još uvijek nema recenziju.
            // Trazimo najnoviju završenu rezervaciju.
            var zavrsenaRezervacija = await _context.Rezervacija
                .Where(r => r.osobaId == korisnikId &&
                             r.voziloID == VoziloId &&
                             r.datumZavrsetka < DateTime.Now && // Uslov da je rezervacija završena
                             r.recenzijaId == 0) // Uslov da već nije ocijenjena (pretpostavljamo da je 0 default vrijednost)
                .OrderByDescending(r => r.datumZavrsetka) // Uzimamo najsvježiju završenu rezervaciju
                .FirstOrDefaultAsync();

            // Korak 4: Ako ne postoji takva rezervacija, korisnik ne može ostaviti recenziju.
            if (zavrsenaRezervacija == null)
            {
                TempData["ErrorMessage"] = "Ne možete ostaviti recenziju. Morate imati završenu i neocijenjenu rezervaciju za ovo vozilo.";
                return RedirectToAction("Details", "Vozilos", new { id = VoziloId });
            }

            // Korak 5: Ako je sve uredu, spremi recenziju i poveži je sa rezervacijom.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Prvo spremamo recenziju da bismo dobili njen ID
                    _context.Add(novaRecenzija);
                    await _context.SaveChangesAsync();

                    // Sada kada imamo ID recenzije, ažuriramo rezervaciju
                    zavrsenaRezervacija.recenzijaId = novaRecenzija.recenzijaId;
                    _context.Update(zavrsenaRezervacija);
                    await _context.SaveChangesAsync();

                    // Potvrđujemo transakciju
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Hvala Vam! Vaša recenzija je uspješno poslana.";
                }
                catch (Exception)
                {
                    // U slučaju greške, poništavamo sve promjene
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Došlo je do greške prilikom spremanja recenzije.";
                }
            }

            // Vraćamo korisnika na stranicu vozila
            return RedirectToAction("Details", "Vozilos", new { id = VoziloId });
        }


        // --- Ostatak kontrolera (Index, Details, Edit, Delete...) ---
        // Ove metode služe za administraciju recenzija i ostaju nepromijenjene.

        public async Task<IActionResult> Index()
        {
            return View(await _context.Recenzija.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }
            var recenzija = await _context.Recenzija.FirstOrDefaultAsync(m => m.recenzijaId == id);
            if (recenzija == null) { return NotFound(); }
            return View(recenzija);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }
            var recenzija = await _context.Recenzija.FindAsync(id);
            if (recenzija == null) { return NotFound(); }
            return View(recenzija);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("recenzijaId,ocjena,recenzija,komentar")] Recenzija recenzija)
        {
            if (id != recenzija.recenzijaId) { return NotFound(); }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recenzija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecenzijaExists(recenzija.recenzijaId)) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recenzija);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) { return NotFound(); }
            var recenzija = await _context.Recenzija.FirstOrDefaultAsync(m => m.recenzijaId == id);
            if (recenzija == null) { return NotFound(); }
            return View(recenzija);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recenzija = await _context.Recenzija.FindAsync(id);
            if (recenzija != null)
            {
                // TODO: Ovdje također treba raskinuti vezu sa rezervacijom ako je potrebno
                _context.Recenzija.Remove(recenzija);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecenzijaExists(int id)
        {
            return _context.Recenzija.Any(e => e.recenzijaId == id);
        }
    }
}