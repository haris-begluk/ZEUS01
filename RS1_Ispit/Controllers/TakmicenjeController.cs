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
        public IActionResult DodajTakmicenje(DodajTakmicenjeVM obj)
        {
            var model = new Takmicenje
            {
                SkolaId = obj.SkolaId,
                PredmetId = obj.PredmetId,
                Datum = DateTime.Now,
                Zakljucaj = false,
                Razred = _context.Predmet.FirstOrDefault(p => p.Id.Equals(obj.PredmetId)).Razred
            };
            var takmicenje = _context.Takmicenja.Add(model);
            var ucenici = _context.DodjeljenPredmet.Include(o => o.OdjeljenjeStavka).ThenInclude(i => i.Odjeljenje).Include(i => i.Predmet)
                    .Where(dp => dp.ZakljucnoKrajGodine == 5 && dp.OdjeljenjeStavka.Odjeljenje.SkolaID == takmicenje.Entity.SkolaId && dp.Predmet.Id.Equals(takmicenje.Entity.PredmetId))
                    .Select(s => new { s.OdjeljenjeStavka.Id, s.OdjeljenjeStavka.UcenikId });
            foreach (var ucenik in ucenici)
            {
                if (UcenikProsjek(ucenik.UcenikId))
                    _context.TakmicenjeStavke.Add(new TakmicenjeStavka
                    {
                        TakmicenjeId = takmicenje.Entity.Id,
                        OdjeljenjeStavkaId = ucenik.Id,
                        Pristupio = true,
                        Bodovi = 0
                    });
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Takmicenja), new { obj.SkolaId, razredId = model.Razred });
        }

        public IActionResult TakmicenjeRezultati(int id)
        {
            var model = _context.Takmicenja.Where(t => t.Id.Equals(id)).Select(s => new RezultatiVM
            {
                TakmicenjeId = id, 
                Zakljucaj= s.Zakljucaj,
                Skola = s.Skola.Naziv,
                Predmet = s.Predmet.Naziv,
                Razred = s.Razred,
                Datum = s.Datum
            }).FirstOrDefault();

            model.Rezultati = _context.TakmicenjeStavke.Include(i => i.OdjeljenjeStavka).ThenInclude(i => i.Odjeljenje)
                .Where(r => r.TakmicenjeId.Equals(model.TakmicenjeId)).Select(s => new Rezultat
                { 
                    TakmicenjeStavkaId = s.Id,
                    UcesnikId = s.OdjeljenjeStavka.UcenikId,
                    Odjeljenje = s.OdjeljenjeStavka.Odjeljenje.Oznaka,
                    BrojUDnevniku = s.OdjeljenjeStavka.BrojUDnevniku,
                    Pristupio = s.Pristupio,
                    Bodovi = s.Bodovi
                }).ToList();
            return View(model);
        }
        public void Pristupio(int id) {

            var obj = _context.TakmicenjeStavke.FirstOrDefault(a => a.Id.Equals(id));
            obj.Pristupio = !obj.Pristupio;
            _context.SaveChanges();
        }

        public void Zakljucaj(int id)
        {
            var obj = _context.Takmicenja.FirstOrDefault(a => a.Id.Equals(id));
            obj.Zakljucaj = !obj.Zakljucaj;
            _context.SaveChanges();
        } 
        public void EditBodovi(int Id, int bodovi)
        {
            var obj = _context.TakmicenjeStavke.FirstOrDefault(o => o.Id.Equals(Id));
            obj.Bodovi = bodovi;
            _context.SaveChanges();
        }
        public IActionResult UrediRezultat(int id)
        {
            var model = new UcesnikEditVM
            {
                TakmicenjeStavkaId = id, 
                Ucesnici = _context.TakmicenjeStavke.Include(i => i.OdjeljenjeStavka).ThenInclude(i => i.Ucenik)
                .Select(s => new UcesnikVM {
                TakmicenjeStavkaId = s.Id,
                Text = s.OdjeljenjeStavka.Odjeljenje.Oznaka+" - "+s.OdjeljenjeStavka.Ucenik.ImePrezime +" - "+ s.OdjeljenjeStavka.BrojUDnevniku.ToString()
                }).ToList(), 
                Bodovi = _context.TakmicenjeStavke.FirstOrDefault(d => d.Id.Equals(id)).Bodovi
                
            };
            return PartialView("UrediRezultatPartial", model);
        }
        public void UpsertRezultat(UcesnikEditVM model)
        {
            var obj = _context.TakmicenjeStavke.FirstOrDefault(t => t.Id.Equals(model.TakmicenjeStavkaId));
            obj.Bodovi = model.Bodovi;
            _context.SaveChanges();
            
        }
        //Dodatne Metode Helperi
        private bool UcenikProsjek(int id)
        {
            var prosjek = _context.DodjeljenPredmet.Include(i => i.OdjeljenjeStavka).Where(o => o.OdjeljenjeStavka.UcenikId.Equals(id)).Average(o => o.ZakljucnoKrajGodine);
            if (prosjek >= 4) return true; return false;
        }
    }
}