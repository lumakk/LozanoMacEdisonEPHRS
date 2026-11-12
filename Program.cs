using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPHRS
{
    // ---------- DATA MODEL ----------
    // Exactly 10 properties per requirement
    class Employee
    {
        public int Id { get; set; }                 // auto-increment, starts at 1001
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Department { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public decimal MonthlySalary { get; set; }
        public double TaxRate { get; set; }         // fraction, e.g. 0.12 = 12%
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; } = "";
    }

    class Program
    {
        // ---------- DATA STORAGE (starts empty) ----------
        static List<Employee> employees = new List<Employee>();
        static int nextId = 1001;

        static string AppTitle = "=== EMPLOYEE PAYROLL & HR MANAGEMENT SYSTEM ==="; // MERGE-CONFLICT-LINE


        static void Main(string[] args)
        {
            // Main menu loop until Exit
            while (true)
            {
                ClearScreen();
                Console.WriteLine(AppTitle);
                Console.WriteLine();
                Console.WriteLine("1. Create Employee");
                Console.WriteLine("2. Read (View All Employees)");
                Console.WriteLine("3. Search / Filter");
                Console.WriteLine("4. Update Employee");
                Console.WriteLine("5. Delete Employee");
                Console.WriteLine("6. Payroll Summary");
                Console.WriteLine("7. Exit");
                Console.WriteLine();

                Console.Write("Choose an option (1-7): ");
                string choice = ReadLine().Trim();

                switch (choice)
                {
                    case "1": CreateEmployee(); break;
                    case "2": ReadEmployees(); break;
                    case "3": SearchEmployees(); break;
                    case "4": UpdateEmployee(); break;
                    case "5": DeleteEmployee(); break;
                    case "6": PayrollSummary(); break;
                    case "7":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please choose 1-7.");
                        break;
                }

                // Pause so each action can be screenshotted
                Console.WriteLine();
                Console.WriteLine("Press Enter to continue...");
                ReadLine();
            }
        }

        // ==================== CREATE ====================
        // Prompts for every field, validates input, auto-assigns Id from nextId++
        static void CreateEmployee()
        {
            Console.WriteLine();
            Console.WriteLine("--- CREATE EMPLOYEE ---");

            string firstName = ReadNonEmpty("First Name: ");
            string lastName = ReadNonEmpty("Last Name: ");
            string department = ReadNonEmpty("Department (e.g. IT, HR, Sales, Finance): ");
            string jobTitle = ReadNonEmpty("Job Title: ");
            decimal salary = ReadPositiveDecimal("Monthly Salary: ");
            double taxRate = ReadTaxRate("Tax Rate (%): ");
            DateTime hireDate = ReadDate("Hire Date (yyyy-MM-dd): ");
            bool isActive = ReadYesNo("Active? (Y/N): ");
            string email = ReadEmail("Email: ");

            Employee emp = new Employee
            {
                Id = nextId++,
                FirstName = firstName,
                LastName = lastName,
                Department = department,
                JobTitle = jobTitle,
                MonthlySalary = salary,
                TaxRate = taxRate,
                HireDate = hireDate,
                IsActive = isActive,
                Email = email
            };

            employees.Add(emp);
            Console.WriteLine();
            Console.WriteLine("Employee created successfully. Assigned Id: " + emp.Id);
        }

        // ==================== READ ====================
        // Shows all 10 columns in an aligned table using a foreach loop
        static void ReadEmployees()
        {
            Console.WriteLine();
            Console.WriteLine("--- ALL EMPLOYEES ---");

            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            PrintEmployeeTable(employees);
        }

        // ==================== SEARCH / FILTER ====================
        // LINQ-based sub-menu: Id, name, department, status, salary range
        static void SearchEmployees()
        {
            Console.WriteLine();
            Console.WriteLine("--- SEARCH / FILTER ---");
            Console.WriteLine("a. Id");
            Console.WriteLine("b. Name (first or last, partial, case-insensitive)");
            Console.WriteLine("c. Department");
            Console.WriteLine("d. Status (Active/Inactive)");
            Console.WriteLine("e. Salary range (min to max)");
            Console.WriteLine("f. Back");
            Console.Write("Choose a filter: ");
            string choice = ReadLine().Trim().ToLowerInvariant();

            IEnumerable<Employee> results;

            switch (choice)
            {
                case "a":
                    int id = ReadInt("Enter Id: ");
                    results = employees.Where(e => e.Id == id);
                    break;

                case "b":
                    string name = ReadNonEmpty("Enter name (first or last): ");
                    results = employees.Where(e =>
                        e.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        e.LastName.Contains(name, StringComparison.OrdinalIgnoreCase));
                    break;

                case "c":
                    string dept = ReadNonEmpty("Enter Department: ");
                    results = employees.Where(e =>
                        e.Department.Contains(dept, StringComparison.OrdinalIgnoreCase));
                    break;

                case "d":
                    bool active = ReadStatus();
                    results = employees.Where(e => e.IsActive == active);
                    break;

                case "e":
                    decimal min, max;
                    while (true)
                    {
                        min = ReadDecimal("Minimum salary: ");
                        max = ReadDecimal("Maximum salary: ");
                        if (min <= max) break;
                        Console.WriteLine("Minimum cannot be greater than maximum. Try again.");
                    }
                    results = employees.Where(e => e.MonthlySalary >= min && e.MonthlySalary <= max);
                    break;

                case "f":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    return;
            }

            List<Employee> matches = results.ToList();
            Console.WriteLine();
            if (matches.Count == 0)
            {
                Console.WriteLine("No matching employees.");
                return;
            }

            PrintEmployeeTable(matches);
        }

        // ==================== UPDATE ====================
        // Finds by Id with FirstOrDefault; Enter keeps the current value of a field
        static void UpdateEmployee()
        {
            Console.WriteLine();
            Console.WriteLine("--- UPDATE EMPLOYEE ---");

            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            PrintEmployeeTable(employees);

            Console.WriteLine();
            Console.Write("Enter Id of the employee to update: ");
            string input = ReadLine().Trim();
            if (!int.TryParse(input, out int id))
            {
                Console.WriteLine("Invalid Id format. Please enter a number.");
                return;
            }

            Employee? emp = employees.FirstOrDefault(e => e.Id == id);
            if (emp == null)
            {
                Console.WriteLine("Employee with Id " + id + " not found.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Editing " + emp.FirstName + " " + emp.LastName +
                              " (press Enter to keep the current value)");

            emp.FirstName = ReadNonEmptyOptional("First Name", emp.FirstName);
            emp.LastName = ReadNonEmptyOptional("Last Name", emp.LastName);
            emp.Department = ReadNonEmptyOptional("Department", emp.Department);
            emp.JobTitle = ReadNonEmptyOptional("Job Title", emp.JobTitle);
            emp.MonthlySalary = ReadPositiveDecimalOptional("Monthly Salary", emp.MonthlySalary);
            emp.TaxRate = ReadTaxRateOptional("Tax Rate (%)", emp.TaxRate);
            emp.HireDate = ReadDateOptional("Hire Date (yyyy-MM-dd)", emp.HireDate);
            emp.IsActive = ReadYesNoOptional("Active? (Y/N)", emp.IsActive);
            emp.Email = ReadEmailOptional("Email", emp.Email);

            Console.WriteLine();
            Console.WriteLine("Employee with Id " + emp.Id + " updated successfully.");
        }

        // ==================== DELETE ====================
        // Confirms with Y/N, removes with RemoveAll and reports from the returned count
        static void DeleteEmployee()
        {
            Console.WriteLine();
            Console.WriteLine("--- DELETE EMPLOYEE ---");

            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            PrintEmployeeTable(employees);

            Console.WriteLine();
            Console.Write("Enter Id of the employee to delete: ");
            string input = ReadLine().Trim();
            if (!int.TryParse(input, out int id))
            {
                Console.WriteLine("Invalid Id format. Please enter a number.");
                return;
            }

            if (!ReadYesNo("Delete employee with Id " + id + "? (Y/N): "))
            {
                Console.WriteLine("Delete cancelled.");
                return;
            }

            int removed = employees.RemoveAll(e => e.Id == id);
            Console.WriteLine(removed > 0
                ? "Employee with Id " + id + " deleted successfully."
                : "Employee with Id " + id + " not found.");
        }

        // ==================== PAYROLL SUMMARY ====================
        // Net pay per employee, overall totals, and a GroupBy department breakdown
        static void PayrollSummary()
        {
            Console.WriteLine();
            Console.WriteLine("--- PAYROLL SUMMARY ---");

            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            // Per-employee net pay = MonthlySalary - (MonthlySalary * TaxRate)
            Console.WriteLine("{0,-6} {1,-24} {2,12} {3,10} {4,12}",
                "Id", "Name", "Gross Pay", "Tax", "Net Pay");
            Console.WriteLine(new string('-', 68));
            foreach (Employee e in employees)
            {
                decimal tax = e.MonthlySalary * (decimal)e.TaxRate;
                decimal net = e.MonthlySalary - tax;
                Console.WriteLine("{0,-6} {1,-24} {2,12:N2} {3,10:N2} {4,12:N2}",
                    e.Id, e.FirstName + " " + e.LastName, e.MonthlySalary, tax, net);
            }

            // Totals via LINQ
            int totalCount = employees.Count;
            int activeCount = employees.Count(e => e.IsActive);
            decimal totalGross = employees.Sum(e => e.MonthlySalary);
            decimal totalTax = employees.Sum(e => e.MonthlySalary * (decimal)e.TaxRate);
            decimal totalNet = employees.Sum(e => e.MonthlySalary - e.MonthlySalary * (decimal)e.TaxRate);

            Console.WriteLine();
            Console.WriteLine("Employee count       : " + totalCount);
            Console.WriteLine("Active employees     : " + activeCount);
            Console.WriteLine("Total gross payroll  : " + totalGross.ToString("N2"));
            Console.WriteLine("Total tax withheld   : " + totalTax.ToString("N2"));
            Console.WriteLine("Total net payroll    : " + totalNet.ToString("N2"));

            // Per-department breakdown with GroupBy
            Console.WriteLine();
            Console.WriteLine("--- DEPARTMENT BREAKDOWN ---");
            Console.WriteLine("{0,-20} {1,10} {2,18}", "Department", "Headcount", "Total Gross");
            Console.WriteLine(new string('-', 50));
            foreach (var group in employees.GroupBy(e => e.Department))
            {
                Console.WriteLine("{0,-20} {1,10} {2,18:N2}",
                    group.Key, group.Count(), group.Sum(e => e.MonthlySalary));
            }
        }

        // ---------- TABLE DISPLAY ----------
        // Prints all 10 columns with composite formatting and a header row
        static void PrintEmployeeTable(IEnumerable<Employee> list)
        {
            Console.WriteLine("{0,-6} {1,-10} {2,-10} {3,-10} {4,-14} {5,11} {6,7} {7,-10} {8,-9} {9,-24}",
                "Id", "FirstName", "LastName", "Department", "JobTitle",
                "Salary", "Tax", "HireDate", "Status", "Email");
            Console.WriteLine(new string('-', 120));

            foreach (Employee e in list)
            {
                Console.WriteLine("{0,-6} {1,-10} {2,-10} {3,-10} {4,-14} {5,11} {6,7} {7,-10} {8,-9} {9,-24}",
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.Department,
                    e.JobTitle,
                    e.MonthlySalary.ToString("N2"),
                    (e.TaxRate * 100).ToString("0.##") + "%",
                    e.HireDate.ToString("yyyy-MM-dd"),
                    e.IsActive ? "Active" : "Inactive",
                    e.Email);
            }
        }

        // ---------- INPUT VALIDATION HELPERS ----------

        // Reads a line; exits cleanly if input ends (EOF / piped input)
        static string ReadLine()
        {
            string? line = Console.ReadLine();
            if (line == null)
            {
                Console.WriteLine();
                Environment.Exit(0);
            }
            return line;
        }

        // Clears the menu area; ignored when output is redirected
        static void ClearScreen()
        {
            try { Console.Clear(); }
            catch (Exception) { /* no console buffer available */ }
        }

        // Non-blank string (used for names, department, job title)
        static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (input.Length > 0) return input;
                Console.WriteLine("Input cannot be blank. Try again.");
            }
        }

        // Any valid decimal number
        static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (decimal.TryParse(input, out decimal value)) return value;
                Console.WriteLine("Invalid number. Try again.");
            }
        }

        // Decimal greater than 0 (used for salary)
        static decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (decimal.TryParse(input, out decimal value) && value > 0) return value;
                Console.WriteLine("Invalid. Enter a number greater than 0.");
            }
        }

        // Valid date in yyyy-MM-dd, not in the future
        static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (TryParseHireDate(input, out DateTime date, out string error)) return date;
                Console.WriteLine(error);
            }
        }

        // Tax rate typed as a percentage (12) and stored as a fraction (0.12)
        static double ReadTaxRate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (double.TryParse(input, out double pct) && pct >= 0 && pct <= 100)
                    return pct / 100.0;
                Console.WriteLine("Invalid. Tax rate must be between 0 and 100.");
            }
        }

        // Y or N (case-insensitive)
        static bool ReadYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim().ToUpperInvariant();
                if (input == "Y") return true;
                if (input == "N") return false;
                Console.WriteLine("Invalid. Enter Y or N.");
            }
        }

        // Email must contain "@" and "."
        static string ReadEmail(string prompt)
        {
            while (true)
            {
                string email = ReadNonEmpty(prompt);
                if (email.Contains("@") && email.Contains(".")) return email;
                Console.WriteLine("Invalid email. It must contain \"@\" and \".\".");
            }
        }

        // Whole number (used for menu/filter choices)
        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine().Trim();
                if (int.TryParse(input, out int value)) return value;
                Console.WriteLine("Invalid number. Try again.");
            }
        }

        // Status filter: Active or Inactive
        static bool ReadStatus()
        {
            while (true)
            {
                Console.Write("Status (Active/Inactive): ");
                string input = ReadLine().Trim();
                if (input.Equals("Active", StringComparison.OrdinalIgnoreCase)) return true;
                if (input.Equals("Inactive", StringComparison.OrdinalIgnoreCase)) return false;
                Console.WriteLine("Invalid. Enter Active or Inactive.");
            }
        }

        // Shared date parser: yyyy-MM-dd (or any parseable date), never in the future
        static bool TryParseHireDate(string input, out DateTime date, out string error)
        {
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out date) || DateTime.TryParse(input, out date))
            {
                if (date.Date <= DateTime.Today)
                {
                    error = "";
                    return true;
                }
                error = "Hire date cannot be in the future.";
                return false;
            }

            date = default;
            error = "Invalid date. Use format yyyy-MM-dd.";
            return false;
        }

        // ---------- OPTIONAL (UPDATE) HELPERS: Enter keeps the current value ----------

        static string ReadNonEmptyOptional(string label, string current)
        {
            while (true)
            {
                Console.Write(label + " [" + current + "]: ");
                string input = ReadLine().Trim();
                if (input.Length == 0) return current;
                if (input.Length > 0) return input;
            }
        }

        static decimal ReadPositiveDecimalOptional(string label, decimal current)
        {
            while (true)
            {
                Console.Write(label + " [" + current + "]: ");
                string input = ReadLine().Trim();
                if (input.Length == 0) return current;
                if (decimal.TryParse(input, out decimal value) && value > 0) return value;
                Console.WriteLine("Invalid. Enter a number greater than 0.");
            }
        }

        static double ReadTaxRateOptional(string label, double current)
        {
            double currentPct = current * 100;
            while (true)
            {
                Console.Write(label + " [" + currentPct.ToString("0.##") + "]: ");
                string input = ReadLine().Trim();
                if (input.Length == 0) return current;
                if (double.TryParse(input, out double pct) && pct >= 0 && pct <= 100)
                    return pct / 100.0;
                Console.WriteLine("Invalid. Tax rate must be between 0 and 100.");
            }
        }

        static DateTime ReadDateOptional(string label, DateTime current)
        {
            while (true)
            {
                Console.Write(label + " [" + current.ToString("yyyy-MM-dd") + "]: ");
                string input = ReadLine().Trim();
                if (input.Length == 0) return current;
                if (TryParseHireDate(input, out DateTime date, out string error)) return date;
                Console.WriteLine(error);
            }
        }

        static bool ReadYesNoOptional(string label, bool current)
        {
            while (true)
            {
                Console.Write(label + " [" + (current ? "Y" : "N") + "]: ");
                string input = ReadLine().Trim().ToUpperInvariant();
                if (input.Length == 0) return current;
                if (input == "Y") return true;
                if (input == "N") return false;
                Console.WriteLine("Invalid. Enter Y or N.");
            }
        }

        static string ReadEmailOptional(string label, string current)
        {
            while (true)
            {
                Console.Write(label + " [" + current + "]: ");
                string input = ReadLine().Trim();
                if (input.Length == 0) return current;
                if (input.Contains("@") && input.Contains(".")) return input;
                Console.WriteLine("Invalid email. It must contain \"@\" and \".\".");
            }
        }
    }
}
