using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VoziBa.Models;
using System.ComponentModel.DataAnnotations; // Dodano za DisplayName
using System.Reflection; // Dodano za pristup atributima


namespace VoziBa.Controllers
{
    public static class EnumExtensions // Helper klasa za dohvaćanje Display Name-a
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()
                            ?.GetCustomAttribute<DisplayAttribute>()
                            ?.Name ?? enumValue.ToString();
        }
    }

    public class VoziloesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoziloesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Voziloes
        public async Task<IActionResult> Index(string selectedBrend, string selectedGrad, string selectedTransmisija, string selectedGorivo)
        {
            IQueryable<Vozilo> vozila = _context.Vozilo;

            // Filtriranje po marki (Brend)
            if (!string.IsNullOrEmpty(selectedBrend) && selectedBrend != "Svi brendovi")
            {
                // Moramo pronaći Enum vrijednost na osnovu DisplayName-a
                Brend? brendEnum = null;
                foreach (Brend b in Enum.GetValues(typeof(Brend)))
                {
                    if (b.GetDisplayName() == selectedBrend)
                    {
                        brendEnum = b;
                        break;
                    }
                }

                if (brendEnum.HasValue)
                {
                    vozila = vozila.Where(v => v.brend == brendEnum.Value);
                }
            }

            // Filtriranje po gradu (Grad)
            if (!string.IsNullOrEmpty(selectedGrad) && selectedGrad != "Svi gradovi")
            {
                // Moramo pronaći Enum vrijednost na osnovu DisplayName-a
                Grad? gradEnum = null;
                foreach (Grad g in Enum.GetValues(typeof(Grad)))
                {
                    if (g.GetDisplayName() == selectedGrad)
                    {
                        gradEnum = g;
                        break;
                    }
                }

                if (gradEnum.HasValue)
                {
                    vozila = vozila.Where(v => v.grad == gradEnum.Value);
                }
            }

            if (!string.IsNullOrEmpty(selectedTransmisija) && selectedTransmisija != "Sve transmisije")
            {
                // We need to find the Enum value based on the DisplayName
                Transmisija? transmisijaEnum = null;
                foreach (Transmisija t in Enum.GetValues(typeof(Transmisija)))
                {
                    if (t.GetDisplayName() == selectedTransmisija)
                    {
                        transmisijaEnum = t;
                        break;
                    }
                }

                if (transmisijaEnum.HasValue)
                {
                    vozila = vozila.Where(v => v.transmisija == transmisijaEnum.Value);
                }
            }
            if (!string.IsNullOrEmpty(selectedGorivo) && selectedGorivo != "Sva goriva")
            {
                // We need to find the Enum value based on the DisplayName
                Gorivo? gorivoEnum = null;
                foreach (Gorivo g in Enum.GetValues(typeof(Gorivo)))
                {
                    if (g.GetDisplayName() == selectedGorivo)
                    {
                        gorivoEnum = g;
                        break;
                    }
                }

                if (gorivoEnum.HasValue)
                {
                    vozila = vozila.Where(v => v.tipGoriva == gorivoEnum.Value);
                }
            }
            // Priprema liste jedinstvenih marki za dropdown
            var uniqueBrands = Enum.GetValues(typeof(Brend))
                                   .Cast<Brend>()
                                   .Select(b => new { Value = b.GetDisplayName(), Text = b.GetDisplayName() })
                                   .OrderBy(b => b.Text)
                                   .ToList();
            uniqueBrands.Insert(0, new { Value = "Svi brendovi", Text = "Svi brendovi" });
            ViewBag.Brendovi = new SelectList(uniqueBrands, "Value", "Text", selectedBrend);


            // Priprema liste jedinstvenih gradova za dropdown
            var uniqueCities = Enum.GetValues(typeof(Grad))
                                   .Cast<Grad>()
                                   .Select(g => new { Value = g.GetDisplayName(), Text = g.GetDisplayName() })
                                   .OrderBy(g => g.Text)
                                   .ToList();
            uniqueCities.Insert(0, new { Value = "Svi gradovi", Text = "Svi gradovi" });
            ViewBag.Gradovi = new SelectList(uniqueCities, "Value", "Text", selectedGrad);

            var uniqueTransmissions = Enum.GetValues(typeof(Transmisija))
                            .Cast<Transmisija>()
                            .Select(t => new { Value = t.GetDisplayName(), Text = t.GetDisplayName() })
                            .OrderBy(t => t.Text)
                            .ToList();
            uniqueTransmissions.Insert(0, new { Value = "Sve transmisije", Text = "Sve transmisije" }); // Or whatever default text makes sense
            ViewBag.Transmisije = new SelectList(uniqueTransmissions, "Value", "Text", selectedTransmisija);

            var uniqueFuels = Enum.GetValues(typeof(Gorivo))
                      .Cast<Gorivo>()
                      .Select(f => new { Value = f.GetDisplayName(), Text = f.GetDisplayName() })
                      .OrderBy(f => f.Text)
                      .ToList();
            uniqueFuels.Insert(0, new { Value = "Sva goriva", Text = "Sva goriva" }); // Or whatever default text makes sense
            ViewBag.Goriva = new SelectList(uniqueFuels, "Value", "Text", selectedGorivo);

            return View(await vozila.ToListAsync());
        }

        // Ostatak kontrolera ostaje isti kao u prethodnom kodu
        // GET: Voziloes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozilo
                .FirstOrDefaultAsync(m => m.voziloId == id);
            if (vozilo == null)
            {
                return NotFound();
            }

            return View(vozilo);
        }

        // GET: Voziloes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Voziloes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("voziloId,korisnikId,godinaProizvodnje,brend,model,boja,tipGoriva,transmisija,cijenaNajma,opis,slikaPath,grad,Latitude,Longitude")] Vozilo vozilo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vozilo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vozilo);
        }

        // GET: Voziloes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozilo.FindAsync(id);
            if (vozilo == null)
            {
                return NotFound();
            }
            return View(vozilo);
        }

        // POST: Voziloes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("voziloId,korisnikId,godinaProizvodnje,brend,model,boja,tipGoriva,transmisija,cijenaNajma,opis,slikaPath,grad,Latitude,Longitude")] Vozilo vozilo)
        {
            if (id != vozilo.voziloId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vozilo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoziloExists(vozilo.voziloId))
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
            return View(vozilo);
        }

        // GET: Voziloes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozilo
                .FirstOrDefaultAsync(m => m.voziloId == id);
            if (vozilo == null)
            {
                return NotFound();
            }

            return View(vozilo);
        }

        // POST: Voziloes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vozilo = await _context.Vozilo.FindAsync(id);
            if (vozilo != null)
            {
                _context.Vozilo.Remove(vozilo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoziloExists(int id)
        {
            return _context.Vozilo.Any(e => e.voziloId == id);
        }

        public async Task<IActionResult> UpravljanjeAutomobilima()
        {
            return View(await _context.Vozilo.ToListAsync());
        }

        [HttpGet]
        public IActionResult Objavi()
        {
            return View(); 
        }
    }
}