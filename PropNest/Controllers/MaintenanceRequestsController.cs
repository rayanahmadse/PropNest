
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

namespace PropNest.Controllers
{
public class MaintenanceRequestsController : Controller
{
    private readonly PropNestContext _context;

    public MaintenanceRequestsController(PropNestContext context)
    {
        _context = context;
    }

    // GET: MaintenanceRequests
    public async Task<IActionResult> Index()
    {
        var propNestContext = _context.MaintenanceRequests.Include(m => m.PropertyUnit).Include(m => m.Staff);
        return View(await propNestContext.ToListAsync());
    }

    // GET: MaintenanceRequests/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenanceRequest = await _context.MaintenanceRequests
            .Include(m => m.PropertyUnit)
            .Include(m => m.Staff)
            .FirstOrDefaultAsync(m => m.RequestID == id);
        if (maintenanceRequest == null)
        {
            return NotFound();
        }

        return View(maintenanceRequest);
    }

    // GET: MaintenanceRequests/Create
    public IActionResult Create()
    {
        ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber");
        ViewData["StaffID"] = new SelectList(_context.Staff, "StaffID", "FullName");
        return View();
    }

    // POST: MaintenanceRequests/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UnitID,StaffID,Category,Description,DateLogged,Status")] MaintenanceRequest maintenanceRequest)
    {
        if (ModelState.IsValid)
        {
            var unitExists = await _context.PropertyUnits.FindAsync(maintenanceRequest.UnitID);
            if (unitExists == null)
            {
                ModelState.AddModelError("UnitID", "The selected property unit does not exist.");
            }

            if (maintenanceRequest.StaffID.HasValue)
            {
                var staffExists = await _context.Staff.FindAsync(maintenanceRequest.StaffID);
                if (staffExists == null)
                {
                    ModelState.AddModelError("StaffID", "The selected staff member does not exist.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
                ViewData["StaffID"] = new SelectList(_context.Staff, "StaffID", "FullName", maintenanceRequest.StaffID);
                return View(maintenanceRequest);
            }

            _context.Add(maintenanceRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
        ViewData["StaffID"] = new SelectList(_context.Staff, "StaffID", "FullName", maintenanceRequest.StaffID);
        return View(maintenanceRequest);
    }

    // GET: MaintenanceRequests/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
        if (maintenanceRequest == null)
        {
            return NotFound();
        }
        ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
        ViewData["StaffID"] = new SelectList(_context.Staff, "StaffID", "FullName", maintenanceRequest.StaffID);
        return View(maintenanceRequest);
    }

    // POST: MaintenanceRequests/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("RequestID,UnitID,StaffID,Category,Description,DateLogged,Status")] MaintenanceRequest maintenanceRequest)
    {
        if (id != maintenanceRequest.RequestID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(maintenanceRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceRequestExists(maintenanceRequest.RequestID))
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
        ViewData["UnitID"] = new SelectList(_context.PropertyUnits, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
        ViewData["StaffID"] = new SelectList(_context.Staff, "StaffID", "FullName", maintenanceRequest.StaffID);
        return View(maintenanceRequest);
    }

    // GET: MaintenanceRequests/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenanceRequest = await _context.MaintenanceRequests
            .Include(m => m.PropertyUnit)
            .Include(m => m.Staff)
            .FirstOrDefaultAsync(m => m.RequestID == id);
        if (maintenanceRequest == null)
        {
            return NotFound();
        }

        return View(maintenanceRequest);
    }

    // POST: MaintenanceRequests/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
        if (maintenanceRequest != null)
        {
            _context.MaintenanceRequests.Remove(maintenanceRequest);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MaintenanceRequestExists(int id)
    {
        return _context.MaintenanceRequests.Any(e => e.RequestID == id);
    }
}
}
