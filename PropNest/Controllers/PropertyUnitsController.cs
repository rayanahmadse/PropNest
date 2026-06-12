using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class PropertyUnitsController : Controller
    {
        private readonly PropNestContext _context;

        public PropertyUnitsController(PropNestContext context)
        {
            _context = context;
        }

        // GET: PropertyUnits
        public async Task<IActionResult> Index()
        {
            return View(await _context.PropertyUnits.ToListAsync());
        }

        // GET: PropertyUnits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.PropertyUnits.FirstOrDefaultAsync(m => m.UnitID == id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // GET: PropertyUnits/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PropertyUnits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // GET: PropertyUnits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.PropertyUnits.FindAsync(id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: PropertyUnits/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitID,UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            if (id != unit.UnitID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PropertyUnits.Any(e => e.UnitID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // GET: PropertyUnits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.PropertyUnits.FirstOrDefaultAsync(m => m.UnitID == id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: PropertyUnits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.PropertyUnits.FindAsync(id);
            if (unit != null) _context.PropertyUnits.Remove(unit);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
