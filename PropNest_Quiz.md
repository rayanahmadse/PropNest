# University Of Management and Technology
## Pre-Final Grand Quiz (Subjective-Part) — PropNest Edition

**Course Title:** Software Design and Architecture
**Project:** PropNest — Property Management ERP
**Tech Stack:** ASP.NET Core MVC + Web API + ADO.NET + SQL Server

---

## Question 1 [CLO-3] — CRUD, ADO.NET, Search & Filter

**Question:**
PropNest is a Property Management ERP that manages rental property units across a building complex. The system requires a module that allows authorized staff to perform Create, Read, Update, and Delete (CRUD) operations on `PropertyUnit` records stored in SQL Server. Each record contains `UnitID`, `UnitNumber`, `PropertyType`, `FloorLevel`, `AreaSqFt`, `Status`, `AskingRent`, and `VacantSince`. The system uses ASP.NET Core MVC with ADO.NET (not Entity Framework) for direct database control. The system must support filtering units by `Status` (e.g., Vacant, Occupied) and displaying only available units. All database operations must be secured against SQL Injection. Write the complete implementation of the repository methods, database connectivity logic, SQL queries, controller actions, and validation mechanisms.

---

### Answer — `PropertyUnitRepository.cs` (ADO.NET — PropNestAPI)

```csharp
using Microsoft.Data.SqlClient;
using PropNest.Models;

namespace PropNestAPI
{
    public class PropertyUnitRepository
    {
        private readonly string _connectionString = string.Empty;

        public PropertyUnitRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // READ ALL
        public List<PropertyUnit> GetAll()
        {
            var list = new List<PropertyUnit>();
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand("SELECT * FROM PropertyUnit", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(Map(r));
            return list;
        }

        // READ BY ID
        public PropertyUnit? GetById(int id)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            // Parameterized query prevents SQL Injection
            using var cmd = new SqlCommand("SELECT * FROM PropertyUnit WHERE UnitID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        // CREATE
        public int Add(PropertyUnit unit)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            string sql = @"INSERT INTO PropertyUnit(UnitNumber, PropertyType, FloorLevel, AreaSqFt,
                           Amenities, Status, AskingRent, VacantSince)
                           VALUES(@UnitNumber, @PropertyType, @FloorLevel, @AreaSqFt,
                           @Amenities, @Status, @AskingRent, @VacantSince);
                           SELECT SCOPE_IDENTITY();";
            using var cmd = new SqlCommand(sql, con);
            SetParams(cmd, unit);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // UPDATE
        public void Update(PropertyUnit unit)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            string sql = @"UPDATE PropertyUnit SET UnitNumber=@UnitNumber, PropertyType=@PropertyType,
                           FloorLevel=@FloorLevel, AreaSqFt=@AreaSqFt, Amenities=@Amenities,
                           Status=@Status, AskingRent=@AskingRent, VacantSince=@VacantSince
                           WHERE UnitID=@id";
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", unit.UnitID);
            SetParams(cmd, unit);
            cmd.ExecuteNonQuery();
        }

        // DELETE
        public void Delete(int id)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand("DELETE FROM PropertyUnit WHERE UnitID=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // MAP SqlDataReader -> Model
        private PropertyUnit Map(SqlDataReader r) => new PropertyUnit
        {
            UnitID       = (int)r["UnitID"],
            UnitNumber   = r["UnitNumber"].ToString()!,
            PropertyType = r["PropertyType"].ToString()!,
            FloorLevel   = r["FloorLevel"] as string,
            AreaSqFt     = r["AreaSqFt"] == DBNull.Value ? null : (decimal?)r["AreaSqFt"],
            Amenities    = r["Amenities"] as string,
            Status       = r["Status"].ToString()!,
            AskingRent   = r["AskingRent"] == DBNull.Value ? 0m : (decimal)r["AskingRent"],
            VacantSince  = r["VacantSince"] as DateTime?
        };

        // SET PARAMS — reused by Add and Update
        private void SetParams(SqlCommand cmd, PropertyUnit unit)
        {
            cmd.Parameters.AddWithValue("@UnitNumber",   unit.UnitNumber);
            cmd.Parameters.AddWithValue("@PropertyType", unit.PropertyType);
            cmd.Parameters.AddWithValue("@FloorLevel",   (object?)unit.FloorLevel  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AreaSqFt",     (object?)unit.AreaSqFt    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Amenities",    (object?)unit.Amenities   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status",       unit.Status);
            cmd.Parameters.AddWithValue("@AskingRent",   unit.AskingRent);
            cmd.Parameters.AddWithValue("@VacantSince",  (object?)unit.VacantSince ?? DBNull.Value);
        }
    }
}
```

### Answer — `PropertyUnitsController.cs` (MVC — PropNest)

```csharp
using Microsoft.AspNetCore.Mvc;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class PropertyUnitsController : Controller
    {
        private readonly HttpClient _http;

        public PropertyUnitsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        // READ ALL — with optional Status filter
        public async Task<IActionResult> Index(string? filter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits")
                        ?? new List<PropertyUnit>();

            if (filter == "Vacant")
            {
                units = units.Where(u => u.Status == "Vacant").ToList();
                ViewBag.Filter = "Vacant";
            }

            return View(units);
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // CREATE (POST) — new units always start Vacant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,AskingRent")] PropertyUnit unit)
        {
            unit.Status = "Vacant";
            ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/PropertyUnits", unit);
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("UnitID,UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            if (id != unit.UnitID) return NotFound();
            if (ModelState.IsValid)
            {
                await _http.PutAsJsonAsync($"api/PropertyUnits/{id}", unit);
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        // DELETE — Admin only (role-based authorization via Session)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete property units.";
                return RedirectToAction(nameof(Index));
            }
            await _http.DeleteAsync($"api/PropertyUnits/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
```

> **SQL Injection Protection:** All queries use `cmd.Parameters.AddWithValue("@param", value)` — no string concatenation. The `using` keyword ensures connections are disposed properly.

---

## Question 2 [CLO-3] — Sessions, Cookies, Role-Based Auth

**Question:**
PropNest has two user portals: **Admin** and **User**. The system requires a secure authentication and authorization mechanism using Sessions and Cookies. Upon successful login, the system stores `Username`, `Role`, and `UserID` in Session variables. A cookie named `LastLogin` must be created and stored for **15 days** so that users can view their previous login date and time on the login page. If the Session expires, the user must be automatically redirected to the Login page. Only **Admins** can perform delete operations and access restricted modules. Write the complete ASP.NET Core MVC implementation for `AccountController` including `Login()`, `Signup()`, `Logout()`, session/cookie management, and demonstrate role-based access control in a controller action.

---

### Answer — `AccountController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PropNest.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _http;

        public AccountController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            // Read LastLogin cookie and show previous login info
            var lastLogin = Request.Cookies["LastLogin"];
            if (!string.IsNullOrEmpty(lastLogin))
                ViewBag.LastLogin = $"Last login: {lastLogin}";

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            var payload  = new { Username = username, Password = password };
            var response = await _http.PostAsJsonAsync("api/Users/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            var body       = await response.Content.ReadAsStringAsync();
            var user       = JsonSerializer.Deserialize<JsonElement>(body);
            var actualRole = user.GetProperty("role").GetString();

            if (!string.Equals(actualRole, role, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = $"This account is registered as {actualRole}, not {role}.";
                return View();
            }

            // Store in Session
            HttpContext.Session.SetString("Username", user.GetProperty("username").GetString()!);
            HttpContext.Session.SetString("Role",     actualRole!);
            HttpContext.Session.SetInt32("UserID",    user.GetProperty("userID").GetInt32());

            // Store LastLogin cookie — 15 days
            Response.Cookies.Append("LastLogin", DateTime.Now.ToString("MMM dd yyyy, hh:mm tt"),
                new CookieOptions
                {
                    Expires  = DateTimeOffset.Now.AddDays(15),
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                });

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Signup
        public IActionResult Signup() => View();

        // POST: /Account/Signup
        [HttpPost]
        public async Task<IActionResult> Signup(string username, string password, string role)
        {
            var payload  = new { Username = username, Password = password, Role = role };
            var response = await _http.PostAsJsonAsync("api/Users/register", payload);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Username already taken. Please choose another.";
                return View();
            }

            TempData["Success"] = "Account created! Please log in.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
```

### Role-Based Access Enforcement

```csharp
// Session expiry guard + Admin-only delete
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    // Redirect to login if session has expired
    if (HttpContext.Session.GetString("Username") == null)
        return RedirectToAction("Login", "Account");

    // Restrict to Admin role
    if (HttpContext.Session.GetString("Role") != "Admin")
    {
        TempData["Error"] = "Only Admins can delete records.";
        return RedirectToAction(nameof(Index));
    }

    await _http.DeleteAsync($"api/PropertyUnits/{id}");
    return RedirectToAction(nameof(Index));
}
```

### Displaying LastLogin in View

```cshtml
@if (ViewBag.LastLogin != null)
{
    <div class="alert alert-info">
        <i class="bi bi-clock-history"></i> @ViewBag.LastLogin
    </div>
}
```

### Session Configuration in `Program.cs`

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});
app.UseSession();
```

---

## Question 3 [CLO-2] — RESTful Web API with Business Rules

**Question:**
PropNest provides a mobile app, web portal, and management dashboard that all require access to centralized rent payment data. To support multiple client applications, PropNest exposes a Custom RESTful Web API. According to business rules, a rent payment is considered **Overdue** if its `DueDate` is before today and its `Status` is still `"Pending"`. A late fee of 5% is applied to overdue payments past a 5-day grace period. The API must provide endpoints that: return all payments, return a single payment by ID, create/update/delete payments, mark payments as overdue in bulk, apply late fees, and return due reminders. The API must return meaningful HTTP status codes, validate requests, and handle errors gracefully. Write the complete implementation of the `RentPaymentsController` in PropNestAPI.

---

### Answer — `RentPaymentsController.cs` (PropNestAPI)

```csharp
using Microsoft.AspNetCore.Mvc;
using PropNest.Models;

namespace PropNestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentPaymentsController : ControllerBase
    {
        private readonly RentPaymentRepository _repo;
        private readonly RentalAgreementRepository _agreementRepo;
        private readonly TenantRepository _tenantRepo;
        private readonly PropertyUnitRepository _unitRepo;

        public RentPaymentsController(
            RentPaymentRepository repo,
            RentalAgreementRepository agreementRepo,
            TenantRepository tenantRepo,
            PropertyUnitRepository unitRepo)
        {
            _repo          = repo;
            _agreementRepo = agreementRepo;
            _tenantRepo    = tenantRepo;
            _unitRepo      = unitRepo;
        }

        // GET api/RentPayments
        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        // GET api/RentPayments/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _repo.GetById(id);
            return p == null ? NotFound(new { error = $"Payment {id} not found." }) : Ok(p);
        }

        // POST api/RentPayments
        [HttpPost]
        public IActionResult Create(RentPayment p)
        {
            p.PaymentID = _repo.Add(p);
            return Ok(p); // 200 with created record
        }

        // PUT api/RentPayments/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, RentPayment p)
        {
            if (_repo.GetById(id) == null)
                return NotFound(new { error = $"Payment {id} not found." });
            p.PaymentID = id;
            _repo.Update(p);
            return NoContent(); // 204 — success, no body
        }

        // DELETE api/RentPayments/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repo.GetById(id) == null) return NotFound();
            _repo.Delete(id);
            return NoContent();
        }

        // Business Rule: POST api/RentPayments/check-overdue
        // Marks all Pending payments past DueDate as Overdue
        [HttpPost("check-overdue")]
        public IActionResult CheckOverdue()
        {
            var count = _repo.MarkOverdueForDueBefore(DateTime.Today);
            return Ok(new { message = $"{count} payment(s) marked as Overdue.", marked = count });
        }

        // Business Rule: PUT api/RentPayments/{id}/mark-overdue
        [HttpPut("{id}/mark-overdue")]
        public IActionResult MarkOverdue(int id)
        {
            var updated = _repo.MarkOverdueIfDueAndUnpaid(id);
            return updated == null
                ? NotFound(new { error = "Payment not found or not eligible." })
                : Ok(updated);
        }

        // Business Rule: POST api/RentPayments/apply-late-fees
        [HttpPost("apply-late-fees")]
        public IActionResult ApplyLateFees(
            [FromQuery] int graceDays = 5,
            [FromQuery] decimal lateFeeRate = 0.05m)
        {
            var updated = _repo.ApplyLateFees(graceDays, lateFeeRate);
            return Ok(new { message = $"Late fees applied to {updated} payment(s).", updated });
        }

        // GET api/RentPayments/due-reminders
        [HttpGet("due-reminders")]
        public IActionResult GetDueReminders([FromQuery] int reminderDays = 3)
        {
            var reminders = _repo.GetReminderCandidates(reminderDays);
            return Ok(reminders);
        }
    }
}
```

| HTTP Code | When Used |
|-----------|-----------|
| `200 OK` | Successful GET / POST with body |
| `204 No Content` | Successful PUT / DELETE |
| `404 Not Found` | Record doesn't exist |
| `400 Bad Request` | Business rule violation / invalid input |

---

## Question 4 [CLO-2] — XML to JSON Data Migration *(Original — Unchanged)*

A university is replacing its legacy ERP system with a modern cloud-based solution. The old ERP exports student records in XML format, while the new ERP accepts only JSON documents. The migration team has been tasked with developing a utility that reads XML files containing student records, deserializes the data into C# objects, validates the records, removes students whose CGPA is below 2.50, converts the remaining data into JSON format, and stores the final output in a separate file. In addition, the utility should generate migration statistics including total records processed, total records migrated, and total records rejected. You are provided with a `DataMigrationService` class containing `ConvertXmlToJson()`, `GenerateMigrationReport()`, and `ValidateStudentData()` methods. Using XML Serialization, JSON Serialization, Generic Collections, Lambda Expressions, and LINQ, write the complete implementation of the migration utility and explain how data consistency, integrity, and exception handling are maintained throughout the conversion process.

### Answer — `DataMigrationService.cs`

```csharp
using System.Text.Json;
using System.Xml.Serialization;

// ── Models ───────────────────────────────────────────────────────────────────
[XmlRoot("Students")]
public class StudentList
{
    [XmlElement("Student")]
    public List<Student> Students { get; set; } = new();
}

public class Student
{
    public int    StudentId   { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Department  { get; set; } = string.Empty;
    public int    Semester    { get; set; }
    public string Email       { get; set; } = string.Empty;
    public double CGPA        { get; set; }
}

public class MigrationReport
{
    public int TotalProcessed { get; set; }
    public int TotalMigrated  { get; set; }
    public int TotalRejected  { get; set; }
}

// ── Service ──────────────────────────────────────────────────────────────────
public class DataMigrationService
{
    private List<Student> _allStudents      = new();
    private List<Student> _validStudents    = new();
    private List<Student> _rejectedStudents = new();

    public void ConvertXmlToJson(string xmlPath, string jsonPath)
    {
        try
        {
            // Deserialize XML -> List<Student>
            var serializer = new XmlSerializer(typeof(StudentList));
            using var stream = File.OpenRead(xmlPath);
            var data = (StudentList?)serializer.Deserialize(stream)
                       ?? throw new InvalidDataException("XML file is empty or invalid.");

            _allStudents = data.Students;

            // Validate and split using LINQ + Lambda
            _validStudents    = _allStudents.Where(s => ValidateStudentData(s)).ToList();
            _rejectedStudents = _allStudents.Except(_validStudents).ToList();

            // Serialize valid students -> JSON
            var json = JsonSerializer.Serialize(_validStudents,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(jsonPath, json);
            Console.WriteLine("Migration complete.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"XML parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    public bool ValidateStudentData(Student s)
    {
        if (s.CGPA < 2.50)                            return false; // Business rule
        if (string.IsNullOrWhiteSpace(s.StudentName)) return false;
        if (string.IsNullOrWhiteSpace(s.Email))       return false;
        if (s.Semester < 1 || s.Semester > 8)         return false;
        return true;
    }

    public MigrationReport GenerateMigrationReport()
    {
        var report = new MigrationReport
        {
            TotalProcessed = _allStudents.Count,
            TotalMigrated  = _validStudents.Count,
            TotalRejected  = _rejectedStudents.Count
        };

        Console.WriteLine("=== Migration Report ===");
        Console.WriteLine($"Total Processed : {report.TotalProcessed}");
        Console.WriteLine($"Total Migrated  : {report.TotalMigrated}");
        Console.WriteLine($"Total Rejected  : {report.TotalRejected}");

        // LINQ GroupBy — department breakdown
        var deptStats = _validStudents
            .GroupBy(s => s.Department)
            .Select(g => new { Department = g.Key, Count = g.Count() });

        Console.WriteLine("\nDepartment-wise migrated:");
        foreach (var d in deptStats)
            Console.WriteLine($"  {d.Department}: {d.Count}");

        return report;
    }
}
```

---

## Question 5 [CLO-3] — HttpClient, Async, LINQ Analytics Dashboard

**Question:**
PropNest management requires a real-time **Analytics Dashboard** that consumes data from its own REST API at `https://localhost:7120/api/`. The API returns property unit, rental agreement, and rent payment records in JSON format. The dashboard uses `HttpClient` to asynchronously consume these APIs, deserializes JSON responses into strongly typed C# objects, and calculates the following metrics:
- Total rent collected this calendar month (Paid payments only)
- Total number of Vacant property units
- Total count of Overdue payments requiring follow-up
- Number of Active rental agreements expiring within 30 days

The results are displayed in an ASP.NET Core MVC View. Write the complete implementation using `HttpClient`, JSON Deserialization, `LINQ`, and Lambda Expressions with proper exception handling and graceful recovery from API failures.

---

### Answer — `DashboardViewModel.cs`

```csharp
namespace PropNest.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRentCollectedThisMonth { get; set; }
        public int     VacantUnitsCount            { get; set; }
        public int     OverduePaymentsCount        { get; set; }
        public int     AgreementsExpiringIn30Days  { get; set; }
    }
}
```

### Answer — `HomeController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using Newtonsoft.Json.Linq;

namespace PropNest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username        = HttpContext.Session.GetString("Username");
            ViewBag.Role            = HttpContext.Session.GetString("Role");
            ViewBag.RecentActivities = PropNest.Helpers.RecentActivityHelper.GetActivities(HttpContext);

            var viewModel = new DashboardViewModel();

            try
            {
                var client = _httpClientFactory.CreateClient();

                // 1. Rent Payments — async API call
                var paymentResponse = await client.GetAsync("https://localhost:7120/api/RentPayments");
                if (paymentResponse.IsSuccessStatusCode)
                {
                    var payments   = JArray.Parse(await paymentResponse.Content.ReadAsStringAsync());
                    var now        = DateTime.Now;
                    var monthStart = new DateTime(now.Year, now.Month, 1);

                    // LINQ + Lambda: filter Paid payments this month, handle null paymentDate safely
                    viewModel.TotalRentCollectedThisMonth = payments
                        .Where(p =>
                        {
                            if (p["status"]?.ToString() != "Paid") return false;
                            var dateStr = p["paymentDate"]?.ToString();
                            if (string.IsNullOrWhiteSpace(dateStr)) return false;
                            return DateTime.TryParse(dateStr, out var d) && d >= monthStart;
                        })
                        .Sum(p => decimal.TryParse(p["amountPaid"]?.ToString(), out var amt) ? amt : 0);

                    // LINQ: count Overdue
                    viewModel.OverduePaymentsCount = payments
                        .Count(p => p["status"]?.ToString() == "Overdue");
                }

                // 2. Property Units — async API call
                var unitResponse = await client.GetAsync("https://localhost:7120/api/PropertyUnits");
                if (unitResponse.IsSuccessStatusCode)
                {
                    var units = JArray.Parse(await unitResponse.Content.ReadAsStringAsync());
                    viewModel.VacantUnitsCount = units.Count(u => u["status"]?.ToString() == "Vacant");
                }

                // 3. Rental Agreements — async API call
                var agreementResponse = await client.GetAsync("https://localhost:7120/api/RentalAgreements");
                if (agreementResponse.IsSuccessStatusCode)
                {
                    var agreements      = JArray.Parse(await agreementResponse.Content.ReadAsStringAsync());
                    var now             = DateTime.Now;
                    var thirtyDaysLater = now.AddDays(30);

                    viewModel.AgreementsExpiringIn30Days = agreements
                        .Count(a =>
                            a["agreementStatus"]?.ToString() == "Active" &&
                            DateTime.TryParse(a["endDate"]?.ToString(), out var end) &&
                            end <= thirtyDaysLater && end >= now);
                }
            }
            catch (HttpRequestException ex)
            {
                // API unreachable — graceful degradation, metrics stay at 0
                _logger.LogError($"API connection failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard metrics: {ex.Message}");
            }

            return View(viewModel);
        }
    }
}
```

### Answer — Dashboard Razor View

```cshtml
@model PropNest.Models.DashboardViewModel

<div class="row">
    <div class="col-md-3">
        <div class="card">
            <h5>Rent Collected</h5>
            <h2>Rs. @Model.TotalRentCollectedThisMonth.ToString("N0")</h2>
            <small>This calendar month</small>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card">
            <h5>Vacant Units</h5>
            <h2>@Model.VacantUnitsCount</h2>
            <small>Ready to be rented</small>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card">
            <h5>Overdue Payments</h5>
            <h2>@Model.OverduePaymentsCount</h2>
            <small>Requires follow-up</small>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card">
            <h5>Agreements Expiring</h5>
            <h2>@Model.AgreementsExpiringIn30Days</h2>
            <small>Within 30 days</small>
        </div>
    </div>
</div>
```

**Exception Handling Strategy:**

| Exception | Cause | Recovery |
|-----------|-------|----------|
| `HttpRequestException` | API server offline / network error | Log error, dashboard shows 0s |
| `JsonException` | Malformed API response | Caught by outer `catch`, metrics default to 0 |
| `null paymentDate` | Pending/Overdue payments have no date | `TryParse` returns false, record safely skipped |

---
*All answers based on actual PropNest source code.*
