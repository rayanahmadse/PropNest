using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class LeaseContractsController : Controller
    {
        private readonly PropNestContext _context;

        public LeaseContractsController(PropNestContext context)
        {
            _context = context;
        }

        // GET: LeaseContracts
        public async Task<IActionResult> Index()
        {
            var leases = _context.LeaseContracts
                .Include(l => l.Tenant)
                .Include(l => l.PropertyUnit);
            return View(await leases.ToListAsync());
        }

        // GET: LeaseContracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.LeaseContracts
                .Include(l => l.Tenant)
                .Include(l => l.PropertyUnit)
                .FirstOrDefaultAsync(m => m.LeaseID == id);
            if (lease == null) return NotFound();

            return View(lease);
        }

        // GET: LeaseContracts/Create
        public IActionResult Create()
        {
            ViewData["TenantID"] = new SelectList(_context.Tenants, "TenantID", "FullName");
            ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber");
            return View();
        }

        // POST: LeaseContracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenantID,UnitID,StartDate,EndDate,MonthlyRent,SecurityDeposit,LeaseStatus")] LeaseContract lease)
        {
            if (ModelState.IsValid)
            {
                lease.Version = 1;
                _context.Add(lease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantID"] = new SelectList(_context.Tenants, "TenantID", "FullName", lease.TenantID);
            ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", lease.UnitID);
            return View(lease);
        }

        // GET: LeaseContracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.LeaseContracts.FindAsync(id);
            if (lease == null) return NotFound();

            ViewData["TenantID"] = new SelectList(_context.Tenants, "TenantID", "FullName", lease.TenantID);
            ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", lease.UnitID);
            return View(lease);
        }

        // POST: LeaseContracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeaseID,TenantID,UnitID,StartDate,EndDate,MonthlyRent,SecurityDeposit,LeaseStatus")] LeaseContract lease)
        {
            if (id != lease.LeaseID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLease = await _context.LeaseContracts.FindAsync(id);
                    if (existingLease == null) return NotFound();

                    existingLease.TenantID = lease.TenantID;
                    existingLease.UnitID = lease.UnitID;
                    existingLease.StartDate = lease.StartDate;
                    existingLease.EndDate = lease.EndDate;
                    existingLease.MonthlyRent = lease.MonthlyRent;
                    existingLease.SecurityDeposit = lease.SecurityDeposit;
                    existingLease.LeaseStatus = lease.LeaseStatus;

                    _context.Update(existingLease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LeaseContracts.Any(e => e.LeaseID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantID"] = new SelectList(_context.Tenants, "TenantID", "FullName", lease.TenantID);
            ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", lease.UnitID);
            return View(lease);
        }

        // GET: LeaseContracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var lease = await _context.LeaseContracts
                .Include(l => l.Tenant)
                .Include(l => l.PropertyUnit)
                .FirstOrDefaultAsync(m => m.LeaseID == id);
            if (lease == null) return NotFound();

            return View(lease);
        }

        // POST: LeaseContracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lease = await _context.LeaseContracts.FindAsync(id);
            if (lease != null) _context.LeaseContracts.Remove(lease);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
