// RezervacijasController.cs
// Osiguraj da su ovi usingi prisutni na vrhu:
using VoziBa.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

// ... (ostatak usinga)

namespace VoziBa.Controllers
{
    public class RezervacijasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RezervacijasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ... (postojeće akcije: Index, Details, Create (GET/POST), Edit (GET/POST), Delete (GET))

        // POST: Rezervacijas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Ovo je standardno brisanje, npr. od strane administratora
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija != null)
            {
                _context.Rezervacija.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rezervacija uspješno obrisana.";
            return RedirectToAction(nameof(Index)); // Preusmjerava na Rezervacijas Index (admin view)
        }

        // POST: Rezervacijas/CancelConfirmed/5 (Vlasnik otkazuje rezervaciju)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Autorizacija: Samo vlasnik ili administrator
            if (string.IsNullOrEmpty(loggedInUserIdString) || (userRole != "vlasnik" && userRole != "administrator"))
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za otkazivanje rezervacija.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            int userId;
            if (!int.TryParse(loggedInUserIdString, out userId))
            {
                TempData["ErrorMessage"] = "Greška pri provjeri korisničkih podataka.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            // Dohvati rezervaciju i vozilo (za provjeru vlasništva)
            var rezervacija = await _context.Rezervacija
                                            .Include(r => r.Vozilo)
                                            .FirstOrDefaultAsync(m => m.rezervacijaID == id);

            if (rezervacija == null)
            {
                TempData["ErrorMessage"] = "Rezervacija nije pronađena ili je već otkazana/potvrđena/odbijena.";
                return NotFound();
            }

            // Provjera vlasništva: Vlasnik vozila mora biti trenutno prijavljeni korisnik ili administrator
            if (userRole == "vlasnik" && (rezervacija.Vozilo == null || rezervacija.Vozilo.korisnikId != userId))
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za otkazivanje ove rezervacije. Samo vlasnik vozila može otkazati rezervaciju.";
                return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
            }

            try
            {
                _context.Rezervacija.Remove(rezervacija); // Rezervacija se briše
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervacija je uspješno otkazana.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Došlo je do greške prilikom otkazivanja rezervacije: {ex.Message}";
            }

            // Preusmjeri nazad na pregled rezervacija vlasnika
            return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
        }


        // NEW ACTION: POST: Rezervacijas/ConfirmReservation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(loggedInUserIdString) || userRole != "vlasnik")
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za potvrđivanje rezervacija.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            int userId;
            if (!int.TryParse(loggedInUserIdString, out userId))
            {
                TempData["ErrorMessage"] = "Greška pri provjeri korisničkih podataka.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            var rezervacija = await _context.Rezervacija
                                            .Include(r => r.Vozilo)
                                            .FirstOrDefaultAsync(m => m.rezervacijaID == id);

            if (rezervacija == null)
            {
                TempData["ErrorMessage"] = "Rezervacija nije pronađena ili je već obrađena.";
                return NotFound();
            }

            // Provjera vlasništva
            if (rezervacija.Vozilo == null || rezervacija.Vozilo.korisnikId != userId)
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za potvrđivanje ove rezervacije. Samo vlasnik vozila može potvrditi.";
                return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
            }

            try
            {
                _context.Rezervacija.Remove(rezervacija); // POTVRĐENE REZERVACIJE SE BRIŠU IZ TABLICE REZERVACIJA
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervacija je uspješno potvrđena.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Došlo je do greške prilikom potvrđivanja rezervacije: {ex.Message}";
            }

            return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
        }


        // NEW ACTION: POST: Rezervacijas/RejectReservation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReservation(int id)
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(loggedInUserIdString) || userRole != "vlasnik")
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za odbijanje rezervacija.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            int userId;
            if (!int.TryParse(loggedInUserIdString, out userId))
            {
                TempData["ErrorMessage"] = "Greška pri provjeri korisničkih podataka.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            var rezervacija = await _context.Rezervacija
                                            .Include(r => r.Vozilo)
                                            .FirstOrDefaultAsync(m => m.rezervacijaID == id);

            if (rezervacija == null)
            {
                TempData["ErrorMessage"] = "Rezervacija nije pronađena ili je već obrađena.";
                return NotFound();
            }

            // Provjera vlasništva
            if (rezervacija.Vozilo == null || rezervacija.Vozilo.korisnikId != userId)
            {
                TempData["ErrorMessage"] = "Nemate dozvolu za odbijanje ove rezervacije. Samo vlasnik vozila može odbiti.";
                return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
            }

            try
            {
                _context.Rezervacija.Remove(rezervacija); // ODBIJENE REZERVACIJE SE BRIŠU IZ TABLICE REZERVACIJA
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervacija je uspješno odbijena.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Došlo je do greške prilikom odbijanja rezervacije: {ex.Message}";
            }

            return RedirectToAction("PregledRezervacijaVlasnik", "Korisniks");
        }

      

        // GET: Rezervacijas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rezervacija.ToListAsync());
        }

        // GET: Rezervacijas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .FirstOrDefaultAsync(m => m.rezervacijaID == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // GET: Rezervacijas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rezervacijas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("rezervacijaID,osobaId,recenzijaId,voziloID")] Rezervacija rezervacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rezervacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rezervacija);
        }

        // GET: Rezervacijas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }
            return View(rezervacija);
        }

        // POST: Rezervacijas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("rezervacijaID,osobaId,recenzijaId,voziloID")] Rezervacija rezervacija)
        {
            if (id != rezervacija.rezervacijaID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezervacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervacijaExists(rezervacija.rezervacijaID))
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
            return View(rezervacija);
        }

        // GET: Rezervacijas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .FirstOrDefaultAsync(m => m.rezervacijaID == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        

        private bool RezervacijaExists(int id)
        {
            return _context.Rezervacija.Any(e => e.rezervacijaID == id);
        }
    }

}
