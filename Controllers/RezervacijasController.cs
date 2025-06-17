using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VoziBa.Models;


namespace VoziBa.Controllers
{
    public class RezervacijasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RezervacijasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rezervacijas/Create
        // Priprema formu za novu rezervaciju
        // GET: Rezervacijas/Create
        // Prima opcionalni voziloId iz URL-a
        public async Task<IActionResult> Create(int? voziloId)
        {
            // Pripremamo listu svih vozila za dropdown meni
            var vozilaList = await _context.Vozilo
                                           .Select(v => new { v.voziloId, Naziv = v.brend + " " + v.model })
                                           .ToListAsync();

            ViewBag.Vozila = new SelectList(vozilaList, "voziloId", "Naziv", voziloId);

            // --- POČETAK PROMJENE ---

            // Kreiramo novi model i odmah mu postavimo razumne početne datume
            var model = new Rezervacija
            {
                datumPocetka = DateTime.Today, // Početni datum je danas
                datumZavrsetka = DateTime.Today.AddDays(1) // Krajnji datum je sutra
            };

            // --- KRAJ PROMJENE ---

            // Ako smo dobili ID vozila sa prethodne stranice, postavljamo ga na naš model
            if (voziloId.HasValue)
            {
                model.voziloID = voziloId.Value;
            }

            // Vraćamo View sa modelom koji sada ima postavljen ID vozila i ispravne početne datume
            return View(model);
        }

        // POST: Rezervacijas/Create
        // Prima podatke sa forme i kreira rezervaciju
        // POST: Rezervacijas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rezervacija formData)
        {
            string pocetakDatumStr = Request.Form["DatumPocetka"].ToString();
            string zavrsetakDatumStr = Request.Form["DatumZavrsetka"].ToString();

            if (DateTime.TryParse(pocetakDatumStr, out DateTime pocetakDatum))
            {
                formData.datumPocetka = pocetakDatum;
            }
            else
            {
                ModelState.AddModelError("datumPocetka", "Datum početka je obavezan i mora biti u ispravnom formatu.");
            }

            if (DateTime.TryParse(zavrsetakDatumStr, out DateTime zavrsetakDatum))
            {
                formData.datumZavrsetka = zavrsetakDatum;
            }
            else
            {
                ModelState.AddModelError("datumZavrsetka", "Datum završetka je obavezan i mora biti u ispravnom formatu.");
            }

            if (formData.datumPocetka < DateTime.Today)
            {
                ModelState.AddModelError("DatumPocetka", "Datum početka ne može biti u prošlosti.");
            }
            if (formData.datumPocetka >= formData.datumZavrsetka)
            {
                ModelState.AddModelError("DatumZavrsetka", "Datum završetka mora biti nakon datuma početka.");
            }

            if (formData.voziloID == 0)
            {
                ModelState.AddModelError("voziloID", "Molimo odaberite vozilo.");
            }

            if (ModelState.IsValid)
            {
                var korisnikIdStr = HttpContext.Session.GetString("LoggedInUserId");
                if (string.IsNullOrEmpty(korisnikIdStr))
                {
                    TempData["ErrorMessage"] = "Morate biti prijavljeni da biste izvršili rezervaciju.";
                    return RedirectToAction("Prijava", "Korisniks");
                }

                formData.osobaId = int.Parse(korisnikIdStr);
                formData.datumKreiranja = DateTime.Now;

                _context.Add(formData);
                await _context.SaveChangesAsync();

                // --- POČETAK PROMJENE ---

                // 1. Postavljamo poruku o uspjehu u ViewBag
                ViewBag.SuccessMessage = "Vaša rezervacija je uspješno kreirana!";

                // 2. Čistimo ModelState da se forma resetuje
                ModelState.Clear();

                // 3. Ponovo pripremamo listu vozila za dropdown
                var vozilaList = await _context.Vozilo
                                               .Select(v => new { v.voziloId, Naziv = v.brend + " " + v.model })
                                               .ToListAsync();
                ViewBag.Vozila = new SelectList(vozilaList, "voziloId", "Naziv");

                // 4. Vraćamo isti View sa praznim modelom, spreman za novu rezervaciju
                return View(new Rezervacija());

                // --- KRAJ PROMJENE ---
            }

            // Ako validacija nije uspjela, ponovo popunjavamo ViewBag sa vozilima i vraćamo formu
            var vozilaListZaGresku = await _context.Vozilo
                                                   .Select(v => new { v.voziloId, Naziv = v.brend + " " + v.model })
                                                   .ToListAsync();
            ViewBag.Vozila = new SelectList(vozilaListZaGresku, "voziloId", "Naziv", formData.voziloID);

            return View(formData);
        }
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

        // POST: Rezervacijas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija != null)
            {
                _context.Rezervacija.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RezervacijaExists(int id)
        {
            return _context.Rezervacija.Any(e => e.rezervacijaID == id);
        }
    }

    // Ostale metode (Index, Details, Edit, Delete) mogu ostati kakve jesu ili ih možeš obrisati ako ti ne trebaju.
}
