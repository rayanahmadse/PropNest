
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

namespace PropNest.Controllers
{
public class StaffController : Controller
{
    private readonly PropNestContext _context;

    public StaffController(PropNestContext context)
    {
        _context = context;
    }

    // GET: Staff
    public async Task<IActionResult> Index()
    {
        return View(await _context.Staff.ToListAsync());
    }

    // GET: Staff/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var staff = await _context.Staff
            .FirstOrDefaultAsync(m => m.StaffID == id);
        if (staff == null)
        {
            return NotFound();
        }

        return View(staff);
    }

    // GET: Staff/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Staff/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,ContactNumber,Specialty,Status")] Staff staff)
    {
        if (ModelState.IsValid)
        {
            _context.Add(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(staff);
    }

    // GET: Staff/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var staff = await _context.Staff.FindAsync(id);
        if (staff == null)
        {
            return NotFound();
        }
        return View(staff);
    }

    // POST: Staff/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("StaffID,FullName,ContactNumber,Specialty,Status")] Staff staff)
    {
        if (id != staff.StaffID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(staff);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(staff.StaffID))
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
        return View(staff);
    }

    // GET: Staff/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var staff = await _context.Staff
            .FirstOrDefaultAsync(m => m.StaffID == id);
        if (staff == null)
        {
            return NotFound();
        }

        return View(staff);
    }

    // POST: Staff/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff != null)
        {
            _context.Staff.Remove(staff);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool StaffExists(int id)
    {
        return _context.Staff.Any(e => e.StaffID == id);
    }
}
}
