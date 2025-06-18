using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoziBa.Models;

namespace VoziBa.Controllers
{
    [Authorize(Roles = "vlasnik")]
    public class OglasiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public OglasiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Objavi()
        {
            return View(new Vozilo());
        }

        [HttpPost]
        public async Task<IActionResult> Objavi(Vozilo vozilo, IFormFile slika)
        {
            if (!ModelState.IsValid)
                return View(vozilo);

            // Snimanje slike ako postoji
            if (slika != null && slika.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "images");
                string fileName = Guid.NewGuid().ToString() + "_" + slika.FileName;
                string fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await slika.CopyToAsync(stream);
                }

                vozilo.slikaPath = "/images/" + fileName;
            }

            // Dodavanje korisnika iz sesije/auth identiteta
            vozilo.korisnikId = int.Parse(User.FindFirst("Id").Value);

            _context.Vozilo.Add(vozilo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detalji", new { id = vozilo.voziloId });
        }

        public IActionResult Detalji(int id)
        {
            var vozilo = _context.Vozilo.FirstOrDefault(v => v.voziloId == id);
            if (vozilo == null) return NotFound();

            return View(vozilo);
        }
    }
}