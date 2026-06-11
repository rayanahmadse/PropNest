
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

public class TenantsController : Controller
{
    private readonly PropNestContext _context;

    public TenantsController(PropNestContext context)
    {
        _context = context;
    }

    // GET: TENANTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Tenants.ToListAsync());
    }

    // GET: TENANTS/Details/5
    public async Task<IActionResult> Details(int? tenantid)
    {
        if (tenantid == null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(m => m.TenantID == tenantid);
        if (tenant == null)
        {
            return NotFound();
        }

        return View(tenant);
    }

    // GET: TENANTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TENANTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenantID,FullName,CNIC,Email,ContactNumber,EmergencyContact,Status")] Tenant tenant)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tenant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tenant);
    }

    // GET: TENANTS/Edit/5
    public async Task<IActionResult> Edit(int? tenantid)
    {
        if (tenantid == null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants.FindAsync(tenantid);
        if (tenant == null)
        {
            return NotFound();
        }
        return View(tenant);
    }

    // POST: TENANTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? tenantid, [Bind("TenantID,FullName,CNIC,Email,ContactNumber,EmergencyContact,Status")] Tenant tenant)
    {
        if (tenantid != tenant.TenantID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tenant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(tenant.TenantID))
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
        return View(tenant);
    }

    // GET: TENANTS/Delete/5
    public async Task<IActionResult> Delete(int? tenantid)
    {
        if (tenantid == null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(m => m.TenantID == tenantid);
        if (tenant == null)
        {
            return NotFound();
        }

        return View(tenant);
    }

    // POST: TENANTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? tenantid)
    {
        var tenant = await _context.Tenants.FindAsync(tenantid);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TenantExists(int? tenantid)
    {
        return _context.Tenants.Any(e => e.TenantID == tenantid);
    }
}
