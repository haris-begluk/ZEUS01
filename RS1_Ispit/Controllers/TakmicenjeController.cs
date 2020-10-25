using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RS1_Ispit_asp.net_core.EF;
using RS1_Ispit_asp.net_core.EntityModels;
using RS1_Ispit_asp.net_core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RS1_Ispit_asp.net_core.Controllers
{
    public class TakmicenjeController : Controller
    {
        private readonly MojContext _context;

        public TakmicenjeController(MojContext context)
        {
            _context = context;
        }

        // GET: TakmicenjeController
        public IActionResult Index()
        {
            var model = new IndexVM
            {
                Skole = _context.Skola.ToList(),
                Razredi = new List<short> { 1, 2, 3, 4 },
                Data = _context.Takmicenja.Include(t => t.Skola).Include(t => t.Predmet).Select(t =>
                   new Row
                   {
                       TakmicenjeId = t.Id,
                       Skola = t.Skola.Naziv,
                       Razred = t.Razred,
                       Datum = t.Datum,
                       Predmet = t.Predmet.Naziv,
                       NajboljiUcenik = _context.TakmicenjeStavke.Where(n => n.TakmicenjeId.Equals(t.Id))
                       .OrderByDescending(o => o.Bodovi)
                        .Select(o => o.OdjeljenjeStavka.Odjeljenje.Skola.Naziv +
                        "|" + o.OdjeljenjeStavka.Odjeljenje.Oznaka + "|" + o.OdjeljenjeStavka.Ucenik.ImePrezime)
                        .FirstOrDefault()
                   }).ToList()
            };

            return View(model);
        }

        // GET: TakmicenjeController/Details/5
        public IActionResult Takmicenja(int skolaId, int razredId)
        {
            var model = new IndexVM
            {
                SkolaId = skolaId,
                Razred = razredId,
                Skole = _context.Skola.ToList(),
                Razredi = new List<short> { 1, 2, 3, 4 },
                Data = _context.Takmicenja.Include(t => t.Skola)
                .Include(t => t.Predmet)
                .Where(t => t.SkolaId.Equals(skolaId) && t.Predmet.Razred.Equals(razredId))
                .Select(t =>
                   new Row
                   {
                       TakmicenjeId = t.Id,
                       Skola = t.Skola.Naziv,
                       Razred = t.Razred,
                       Datum = t.Datum,
                       Predmet = t.Predmet.Naziv,
                       NajboljiUcenik = _context.TakmicenjeStavke.Where(n => n.TakmicenjeId.Equals(t.Id))
                       .OrderByDescending(o => o.Bodovi)
                        .Select(o => o.OdjeljenjeStavka.Odjeljenje.Skola.Naziv +
                        "|" + o.OdjeljenjeStavka.Odjeljenje.Oznaka + "|" + o.OdjeljenjeStavka.Ucenik.ImePrezime)
                        .FirstOrDefault()
                   }).ToList()
            };

            return PartialView("TakmicenjaPartial", model);
        }

        public IActionResult DodajTakmicenje(int skolaId)
        {
            var model = new DodajTakmicenjeVM
            {
                Skole = _context.Skola.ToList(),
                SkolaId = skolaId,
                Predmeti = _context.Predmet.ToList(),
                Datum = DateTime.Now
            };
            return PartialView("DodajTakmicenjePartial", model);
        }

        [HttpPost]
        public void DodajTakmicenje(DodajTakmicenjeVM obj)
        {
            var model = new Takmicenje
            {
                SkolaId = obj.SkolaId,
                PredmetId = obj.PredmetId,
                Datum = DateTime.Now,
                Zakljucaj = false,
                Razred = _context.Predmet.FirstOrDefault(p => p.Id.Equals(obj.PredmetId)).Razred
            };
            _context.Takmicenja.Add(model);
            _context.SaveChanges();
        }
    }
}