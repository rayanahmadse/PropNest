# PropNest 🏢

A comprehensive property management system built with **ASP.NET Core MVC** and **Entity Framework Core**. PropNest simplifies the day-to-day operations of property management by providing a centralized platform to track units, tenants, staff, and maintenance requests.

## 🌟 Features

* **Tenant Management**: Track tenant details, contact information, emergency contacts, and active status.
* **Property Units**: Manage available property units, floor levels, amenities, asking rent, and occupancy status.
* **Staff Directory**: Keep a structured record of staff members, their specialties, and contact details.
* **Maintenance Requests**: Log and monitor maintenance tickets, linking them to specific property units and assigning them to relevant staff members.
* **Lease Contracts & Rent Payments**: Track active leases and record rent payment histories.

## 🛠️ Tech Stack

* **Framework**: .NET 8
* **Web Architecture**: ASP.NET Core MVC
* **ORM**: Entity Framework Core
* **Database**: SQL Server
* **Frontend**: HTML5, CSS3, Razor Views, Bootstrap

## 🚀 Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server (or LocalDB for development)
* Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mahadbintalat2003-svg/PropNest.git
   cd PropNest
   ```

2. **Update Database Connection:**
   Open `appsettings.json` and ensure the `DefaultConnection` string points to your local SQL Server instance.

3. **Apply Migrations:**
   Run the following command in the Package Manager Console or terminal to create the database:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Navigate to `https://localhost:5001` in your browser.

## 📄 License

This project is licensed under the MIT License.
