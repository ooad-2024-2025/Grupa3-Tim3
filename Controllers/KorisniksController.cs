using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoziBa.Models;
using System.ComponentModel.DataAnnotations; // Dodaj ako već nije tu
using VoziBa.ValidationAttributes; // Dodaj ako već nije tu

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("korisnikId,username,ime,prezime,datumRodjenja,email,brojTelefona,lozinka,uloga")] Korisnik korisnik)
        {
            if (ModelState.IsValid)
            {
                if (_context.Korisnik.Any(k => k.username == korisnik.username))
                {
                    ModelState.AddModelError("username", "Korisničko ime je već zauzeto.");
                    return View(korisnik);
                }
                if (_context.Korisnik.Any(k => k.email == korisnik.email))
                {
                    ModelState.AddModelError("email", "Email je već registriran.");
                    return View(korisnik);
                }

                _context.Add(korisnik);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Uspješno ste se registrirali!";
                return RedirectToAction("UspjesnaRegistracija");
            }
            return View(korisnik);
        }

        // GET: Korisniks/Edit/5 - OVO JE VAŠ POSTOJEĆI EDIT ZA ADMINISTRACIJU. NISMO GA MIJENJALI.
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

        // POST: Korisniks/Edit/5 - OVO JE VAŠ POSTOJEĆI EDIT ZA ADMINISTRACIJU. NISMO GA MIJENJALI.
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

        // GET: Korisniks/MyProfileEdit - NOVE AKCIJE ZA UREĐIVANJE VLASTITOG PROFILA
        [HttpGet]
        public async Task<IActionResult> MyProfileEdit() // Bez parametra ID, uzima iz sesije
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            if (string.IsNullOrEmpty(loggedInUserIdString))
            {
                TempData["ErrorMessage"] = "Morate se prijaviti za uređivanje profila.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            if (!int.TryParse(loggedInUserIdString, out int loggedInUserId))
            {
                TempData["ErrorMessage"] = "Došlo je do greške sa vašom sesijom. Pokušajte se ponovo prijaviti.";
                return RedirectToAction("Odjava", "Korisniks");
            }

            var korisnik = await _context.Korisnik.FindAsync(loggedInUserId);
            if (korisnik == null)
            {
                TempData["ErrorMessage"] = "Vaš korisnički račun više ne postoji.";
                return RedirectToAction("Odjava", "Korisniks");
            }

            // Mapiranje Korisnik modela na EditProfileViewModel
            var viewModel = new EditProfileViewModel
            {
                korisnikId = korisnik.korisnikId,
                username = korisnik.username,
                ime = korisnik.ime,
                prezime = korisnik.prezime,
                datumRodjenja = korisnik.datumRodjenja,
                email = korisnik.email,
                brojTelefona = korisnik.brojTelefona
            };

            return View(viewModel); // Prosljeđujemo ViewModel
        }

        // POST: Korisniks/MyProfileEdit - NOVE AKCIJE ZA UREĐIVANJE VLASTITOG PROFILA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfileEdit(EditProfileViewModel model) // Prima ViewModel
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");
            if (string.IsNullOrEmpty(loggedInUserIdString) || !int.TryParse(loggedInUserIdString, out int loggedInUserId) || loggedInUserId != model.korisnikId)
            {
                TempData["ErrorMessage"] = "Niste ovlašteni za ovu radnju ili je došlo do greške.";
                return RedirectToAction("Odjava", "Korisniks");
            }

            if (ModelState.IsValid)
            {
                var existingKorisnik = await _context.Korisnik.AsNoTracking().FirstOrDefaultAsync(k => k.email == model.email && k.korisnikId != model.korisnikId);
                if (existingKorisnik != null)
                {
                    ModelState.AddModelError("email", "Email je već registriran od strane drugog korisnika.");
                    return View(model);
                }

                try
                {
                    var userToUpdate = await _context.Korisnik.FindAsync(model.korisnikId);
                    if (userToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Ažuriraj samo dozvoljena polja iz ViewModel-a na entitetu iz baze
                    userToUpdate.ime = model.ime;
                    userToUpdate.prezime = model.prezime;
                    userToUpdate.datumRodjenja = model.datumRodjenja;
                    userToUpdate.email = model.email;
                    userToUpdate.brojTelefona = model.brojTelefona;
                    // username, lozinka i uloga se NE mijenjaju putem ove forme.

                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profil uspješno ažuriran!";
                    return RedirectToAction("Profil", "Korisniks"); // Preusmjeri na Profil akciju
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KorisnikExists(model.korisnikId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Došlo je do greške prilikom spremanja profila: {ex.Message}";
                    return View(model);
                }
            }
            TempData["ErrorMessage"] = "Molimo ispravite greške u formi.";
            return View(model);
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
            var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.username == username);

            if (korisnik != null)
            {
                // PAŽNJA: OVDJE TREBATE SIGURNO HASIRATI LOZINKE!
                if (korisnik.lozinka == password)
                {
                    HttpContext.Session.SetString("LoggedInUserId", korisnik.korisnikId.ToString());
                    HttpContext.Session.SetString("LoggedInUsername", korisnik.username);
                    HttpContext.Session.SetString("UserRole", korisnik.uloga.ToString());
                    TempData["SuccessMessage"] = $"Dobrodošli, {korisnik.username}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Neispravno korisničko ime ili lozinka.";
                    return RedirectToAction("Prijava");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Neispravno korisničko ime ili lozinka.";
                return RedirectToAction("Prijava");
            }
        }

        [HttpGet]
        public IActionResult Odjava()
        {
            HttpContext.Session.Remove("LoggedInUserId");
            HttpContext.Session.Remove("LoggedInUsername");
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "Uspješno ste se odjavili.";
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
                TempData["ErrorMessage"] = "Morate se prijaviti za pristup profilu.";
                return RedirectToAction("Prijava");
            }

            if (int.TryParse(loggedInUserIdString, out int loggedInUserId))
            {
                var korisnik = await _context.Korisnik.FindAsync(loggedInUserId);
                if (korisnik == null)
                {
                    TempData["ErrorMessage"] = "Vaš korisnički račun više ne postoji.";
                    return RedirectToAction("Odjava");
                }
                return View("Profil1", korisnik); // <--- IZMIJENJENA LINIJA: Sada traži "Profil1.cshtml"
            }
            else
            {
                TempData["ErrorMessage"] = "Došlo je do greške sa vašom sesijom. Pokušajte se ponovo prijaviti.";
                return RedirectToAction("Odjava");
            }
        }
        public async Task<IActionResult> PregledRezervacijaVlasnik()
        {
            var loggedInUserIdString = HttpContext.Session.GetString("LoggedInUserId");

            if (string.IsNullOrEmpty(loggedInUserIdString))
            {
                TempData["ErrorMessage"] = "Molimo prijavite se za pregled rezervacija.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            int userId;
            if (!int.TryParse(loggedInUserIdString, out userId))
            {
                TempData["ErrorMessage"] = "Došlo je do greške prilikom preuzimanja korisničkih podataka. ID korisnika nije u ispravnom format.";
                return RedirectToAction("Prijava", "Korisniks");
            }

            var userReservations = await _context.Rezervacija
                                                .Where(r => r.osobaId == userId)
                                                .Include(r => r.Vozilo)
                                                .ToListAsync();

            return View(userReservations);
        }
    }
}