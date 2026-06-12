using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class RentPaymentsController : Controller
    {
        private readonly PropNestContext _context;

        public RentPaymentsController(PropNestContext context)
        {
            _context = context;
        }

        // GET: RentPayments
        public async Task<IActionResult> Index()
        {
            var payments = _context.RentPayments.Include(r => r.LeaseContract);
            return View(await payments.ToListAsync());
        }

        // GET: RentPayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments
                .Include(r => r.LeaseContract)
                .FirstOrDefaultAsync(m => m.PaymentID == id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        // GET: RentPayments/Create
        public IActionResult Create()
        {
            ViewData["LeaseID"] = new SelectList(_context.LeaseContracts, "LeaseID", "LeaseID");
            return View();
        }

        // POST: RentPayments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeaseID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status")] RentPayment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeaseID"] = new SelectList(_context.LeaseContracts, "LeaseID", "LeaseID", payment.LeaseID);
            return View(payment);
        }

        // GET: RentPayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewData["LeaseID"] = new SelectList(_context.LeaseContracts, "LeaseID", "LeaseID", payment.LeaseID);
            return View(payment);
        }

        // POST: RentPayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentID,LeaseID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status")] RentPayment payment)
        {
            if (id != payment.PaymentID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.RentPayments.Any(e => e.PaymentID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LeaseID"] = new SelectList(_context.LeaseContracts, "LeaseID", "LeaseID", payment.LeaseID);
            return View(payment);
        }

        // GET: RentPayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.RentPayments
                .Include(r => r.LeaseContract)
                .FirstOrDefaultAsync(m => m.PaymentID == id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: RentPayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.RentPayments.FindAsync(id);
            if (payment != null) _context.RentPayments.Remove(payment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
