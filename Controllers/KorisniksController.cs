using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VoziBa.Models;
using VoziBa.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VoziBa.Controllers
{
    public class KorisniksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KorisniksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // PREGLED REZERVACIJA
        // ===================================================================

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

        // ===================================================================
        // SVE OSTALE METODE (NEPROMIJENJENE)
        // ===================================================================
        #region Ostatak Kontrolera
        public async Task<IActionResult> Index() { return View(await _context.Korisnik.ToListAsync()); }
        public async Task<IActionResult> Details(int? id) { if (id == null) { return NotFound(); } var korisnik = await _context.Korisnik.FirstOrDefaultAsync(m => m.korisnikId == id); if (korisnik == null) { return NotFound(); } return View(korisnik); }
        public IActionResult Create() { return View(); }
        public IActionResult Prijava() { return View(); }
        public IActionResult UspjesnaRegistracija() { return View(); }
        [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Create([Bind("korisnikId,username,ime,prezime,datumRodjenja,email,brojTelefona,lozinka,uloga")] Korisnik korisnik) { if (ModelState.IsValid) { if (await _context.Korisnik.AnyAsync(k => k.username == korisnik.username)) { ModelState.AddModelError("username", "Korisničko ime je već zauzeto."); return View(korisnik); } if (await _context.Korisnik.AnyAsync(k => k.email == korisnik.email)) { ModelState.AddModelError("email", "Email je već registriran."); return View(korisnik); } _context.Add(korisnik); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Uspješno ste se registrirali! Sada se možete prijaviti."; return RedirectToAction("Prijava"); } return View(korisnik); }
        [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Login(string username, string password) { var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.username == username); if (korisnik != null && korisnik.lozinka == password) { var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, korisnik.korisnikId.ToString()), new Claim(ClaimTypes.Name, korisnik.username), new Claim(ClaimTypes.Role, korisnik.uloga.ToString()) }; var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)); HttpContext.Session.SetString("LoggedInUserId", korisnik.korisnikId.ToString()); HttpContext.Session.SetString("UserRole", korisnik.uloga.ToString()); TempData["SuccessMessage"] = $"Dobrodošli, {korisnik.username}!"; return RedirectToAction("Index", "Home"); } TempData["ErrorMessage"] = "Neispravno korisničko ime ili lozinka."; return RedirectToAction("Prijava"); }
        [HttpGet] public async Task<IActionResult> Odjava() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); HttpContext.Session.Clear(); TempData["SuccessMessage"] = "Uspješno ste se odjavili."; return RedirectToAction("Index", "Home"); }
        [Authorize(Roles = "administrator")][HttpGet] public async Task<IActionResult> Edit(int? id) { if (id == null) { return NotFound(); } var korisnik = await _context.Korisnik.FindAsync(id); if (korisnik == null) { return NotFound(); } return View("EditAdmin", korisnik); }
        [Authorize(Roles = "administrator")][HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, [Bind("korisnikId,username,ime,prezime,datumRodjenja,email,brojTelefona,lozinka,uloga")] Korisnik korisnik) { if (id != korisnik.korisnikId) { return NotFound(); } if (ModelState.IsValid) { try { _context.Update(korisnik); await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!KorisnikExists(id)) { return NotFound(); } else { throw; } } return RedirectToAction(nameof(Index)); } return View("EditAdmin", korisnik); }
        [Authorize][HttpGet] public async Task<IActionResult> EditProfile() { if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int loggedInUserId)) { return Challenge(); } var korisnik = await _context.Korisnik.FindAsync(loggedInUserId); if (korisnik == null) { return NotFound(); } var viewModel = new EditProfileViewModel { KorisnikId = korisnik.korisnikId, Ime = korisnik.ime, Prezime = korisnik.prezime, BrojTelefona = korisnik.brojTelefona, Email = korisnik.email, Username = korisnik.username }; return View(viewModel); }
        [Authorize][HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> EditProfile(EditProfileViewModel model) { if (!ModelState.IsValid) { var userForUsername = await _context.Korisnik.AsNoTracking().FirstOrDefaultAsync(k => k.korisnikId == model.KorisnikId); if (userForUsername != null) model.Username = userForUsername.username; return View(model); } var userToUpdate = await _context.Korisnik.FindAsync(model.KorisnikId); if (userToUpdate == null) { return NotFound(); } userToUpdate.ime = model.Ime; userToUpdate.prezime = model.Prezime; userToUpdate.brojTelefona = model.BrojTelefona; userToUpdate.email = model.Email; if (!string.IsNullOrEmpty(model.StaraLozinka) && !string.IsNullOrEmpty(model.NovaLozinka)) { if (userToUpdate.lozinka == model.StaraLozinka) { userToUpdate.lozinka = model.NovaLozinka; } else { ModelState.AddModelError("StaraLozinka", "Stara lozinka nije ispravna."); model.Username = userToUpdate.username; return View(model); } } await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Profil uspješno ažuriran!"; return RedirectToAction("Profil"); }
        [Authorize][HttpGet] public async Task<IActionResult> Profil() { if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)) { var korisnik = await _context.Korisnik.FindAsync(id); if (korisnik == null) { return RedirectToAction("Odjava"); } return View("Profil1", korisnik); } return Challenge(); }
        [Authorize(Roles = "administrator")][HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id) { var korisnik = await _context.Korisnik.FindAsync(id); if (korisnik != null) { _context.Korisnik.Remove(korisnik); } await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        private bool KorisnikExists(int id) { return _context.Korisnik.Any(e => e.korisnikId == id); }
        #endregion
    }
}