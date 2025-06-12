using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoziBa.Models;

namespace VoziBa.Controllers
{
    public class KorisniksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KorisniksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Korisniks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Korisnik.ToListAsync());
        }

        // GET: Korisniks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik
                .FirstOrDefaultAsync(m => m.korisnikId == id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // GET: Korisniks/Create
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Prijava()
        {
            return View();
        }

        public IActionResult UspjesnaRegistracija()
        {
            return View();
        }


        // POST: Korisniks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("korisnikId,username,ime,prezime,datumRodjenja,email,brojTelefona,lozinka,uloga")] Korisnik korisnik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(korisnik);
                await _context.SaveChangesAsync();
                return RedirectToAction("UspjesnaRegistracija");

            }
            return View(korisnik);
        }

        // GET: Korisniks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }
            return View(korisnik);
        }

        // POST: Korisniks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("korisnikId,username,ime,prezime,datumRodjenja,email,brojTelefona,lozinka,uloga")] Korisnik korisnik)
        {
            if (id != korisnik.korisnikId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korisnik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KorisnikExists(korisnik.korisnikId))
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
            return View(korisnik);
        }

        // GET: Korisniks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik
                .FirstOrDefaultAsync(m => m.korisnikId == id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // POST: Korisniks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var korisnik = await _context.Korisnik.FindAsync(id);
            if (korisnik != null)
            {
                _context.Korisnik.Remove(korisnik);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KorisnikExists(int id)
        {
            return _context.Korisnik.Any(e => e.korisnikId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Pronađi korisnika po korisničkom imenu i lozinci
            // **PAŽNJA: Ova provjera uspoređuje lozinku kao običan string. IZBJEGAVAJTE OVO U PRODUKCIJI!**
            var korisnik = await _context.Korisnik
                                       .FirstOrDefaultAsync(k => k.username == username && k.lozinka == password);

            if (korisnik != null)
            {
                // Prijava uspješna
                HttpContext.Session.SetString("LoggedInUserId", korisnik.korisnikId.ToString());
                HttpContext.Session.SetString("LoggedInUsername", korisnik.username); // Možda želiš i korisničko ime
                HttpContext.Session.SetString("UserRole", korisnik.uloga.ToString());

                return RedirectToAction("Index", "Home"); // Preusmjeri na početnu stranicu
            }
            else
            {
                // Korisnik nije pronađen ili lozinka nije ispravna
                TempData["ErrorMessage"] = "Neispravno korisničko ime ili lozinka.";
                return RedirectToAction("Prijava");
            }
        }

        [HttpGet]
        public IActionResult Odjava()
        {
            HttpContext.Session.Remove("LoggedInUserId");
            HttpContext.Session.Remove("LoggedInUsername");
            HttpContext.Session.Clear(); // Obriši sve iz sesije ako želiš potpuni reset

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Pocetna()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "administrator")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            if (string.IsNullOrEmpty(loggedInUserIdString))
            {
                // Korisnik nije prijavljen, preusmjeri na prijavu
                TempData["ErrorMessage"] = "Morate se prijaviti za pristup profilu.";
                return RedirectToAction("Prijava");
            }

            // Dohvati podatke korisnika iz baze koristeći ID iz sesije
            if (int.TryParse(loggedInUserIdString, out int loggedInUserId))
            {
                var korisnik = await _context.Korisnik.FindAsync(loggedInUserId);
                if (korisnik == null)
                {
                    // Korisnik nije pronađen u bazi (možda je obrisan), odjavi ga
                    TempData["ErrorMessage"] = "Vaš korisnički račun više ne postoji.";
                    return RedirectToAction("Odjava");
                }
                // Proslijedi cijeli Korisnik objekt pogledu
                return View(korisnik);
            }
            else
            {
                // ID u sesiji nije validan broj, odjavi korisnika
                TempData["ErrorMessage"] = "Došlo je do greške sa vašom sesijom. Pokušajte se ponovo prijaviti.";
                return RedirectToAction("Odjava");
            }
        }
    }
}
