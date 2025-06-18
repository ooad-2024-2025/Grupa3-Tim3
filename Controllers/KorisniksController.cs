using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VoziBa.Models;

namespace VoziBa.Controllers
{
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
            public async Task<IActionResult> Details()
            {
                var userId = HttpContext.Session.GetString("LoggedInUserId");
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Prijava", "Korisniks");

                var korisnik = await _context.Korisnik.FindAsync(int.Parse(userId));
                if (korisnik == null)
                    return NotFound();

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

            [Authorize]
            public async Task<IActionResult> PregledRezervacijaKorisniks()
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    return Challenge();
                }

                var userReservations = await _context.Rezervacija
                    .Where(r => r.osobaId == userId)
                    .Include(r => r.Vozilo)
                        .ThenInclude(v => v.Korisnik)
                    .OrderByDescending(r => r.datumKreiranja)
                    .ToListAsync();

                // --- ISPRAVLJENA LOGIKA OVDJE ---
                var viewModelList = userReservations.Select(r =>
                {
                    string statusText;
                    string statusIconClass;

                    if (r.potvrda)
                    {
                        statusText = "Potvrđena";
                        statusIconClass = "fas fa-check-circle text-success";
                    }
                    else
                    {
                        statusText = "Na čekanju";
                        statusIconClass = "fas fa-hourglass-half text-warning";
                    }

                    return new KorisnikRezervacijaViewModel
                    {
                        RezervacijaID = r.rezervacijaID,
                        VoziloModel = r.Vozilo != null ? $"{r.Vozilo.brend} {r.Vozilo.model}" : "N/A",
                        DatumPocetka = r.datumPocetka,
                        DatumZavrsetka = r.datumZavrsetka,
                        DatumKreiranja = r.datumKreiranja,
                        VlasnikImePrezime = r.Vozilo?.Korisnik != null ? $"{r.Vozilo.Korisnik.ime} {r.Vozilo.Korisnik.prezime}" : "N/A",
                        VlasnikBrojTelefona = r.Vozilo?.Korisnik?.brojTelefona ?? "N/A",
                        StatusTekst = statusText,
                        StatusIkonaKlasa = statusIconClass
                    };
                }).ToList();

                return View(viewModelList);
            }

            [Authorize(Roles = "vlasnik")]
            public async Task<IActionResult> PregledRezervacijaVlasnik()
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    return Challenge();
                }

                var ownerReservations = await _context.Rezervacija
                    .Include(r => r.Korisnik)
                    .Include(r => r.Vozilo)
                    .Where(r => r.Vozilo.korisnikId == userId)
                    .OrderByDescending(r => r.datumKreiranja)
                    .ToListAsync();

                var viewModelList = ownerReservations.Select(r =>
                {
                    string statusText;
                    string statusIconClass;

                    if (r.potvrda)
                    {
                        statusText = "Potvrđena";
                        statusIconClass = "fas fa-check-circle text-success";
                    }
                    else
                    {
                        statusText = "Na čekanju";
                        statusIconClass = "fas fa-hourglass-half text-warning";
                    }

                    return new VlasnikRezervacijaViewModel
                    {
                        RezervacijaID = r.rezervacijaID,
                        VoziloModel = r.Vozilo != null ? $"{r.Vozilo.brend} {r.Vozilo.model}" : "N/A",
                        DatumPocetka = r.datumPocetka,
                        DatumZavrsetka = r.datumZavrsetka,
                        DatumKreiranja = r.datumKreiranja,
                        KorisnikImePrezime = r.Korisnik != null ? $"{r.Korisnik.ime} {r.Korisnik.prezime}" : "N/A",
                        KorisnikBrojTelefona = r.Korisnik?.brojTelefona ?? "N/A",
                        StatusTekst = statusText,
                        StatusIkonaKlasa = statusIconClass
                    };
                }).ToList();

                return View(viewModelList);
            }

            [Authorize][HttpGet] public async Task<IActionResult> EditProfile() { if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int loggedInUserId)) { return Challenge(); } var korisnik = await _context.Korisnik.FindAsync(loggedInUserId); if (korisnik == null) { return NotFound(); } var viewModel = new EditProfileViewModel { KorisnikId = korisnik.korisnikId, Ime = korisnik.ime, Prezime = korisnik.prezime, BrojTelefona = korisnik.brojTelefona, Email = korisnik.email, Username = korisnik.username }; return View(viewModel); }
            [Authorize][HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> EditProfile(EditProfileViewModel model) { if (!ModelState.IsValid) { var userForUsername = await _context.Korisnik.AsNoTracking().FirstOrDefaultAsync(k => k.korisnikId == model.KorisnikId); if (userForUsername != null) model.Username = userForUsername.username; return View(model); } var userToUpdate = await _context.Korisnik.FindAsync(model.KorisnikId); if (userToUpdate == null) { return NotFound(); } userToUpdate.ime = model.Ime; userToUpdate.prezime = model.Prezime; userToUpdate.brojTelefona = model.BrojTelefona; userToUpdate.email = model.Email; if (!string.IsNullOrEmpty(model.StaraLozinka) && !string.IsNullOrEmpty(model.NovaLozinka)) { if (userToUpdate.lozinka == model.StaraLozinka) { userToUpdate.lozinka = model.NovaLozinka; } else { ModelState.AddModelError("StaraLozinka", "Stara lozinka nije ispravna."); model.Username = userToUpdate.username; return View(model); } } await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Profil uspješno ažuriran!"; return RedirectToAction("Profil"); }

        }
    }

}