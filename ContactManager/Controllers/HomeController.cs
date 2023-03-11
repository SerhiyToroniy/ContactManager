using ContactManager.Models;
using CsvHelper;
using CsvHelper.Configuration;
using DataTables.AspNet.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
namespace ContactManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _dbContext;

        public HomeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var people = _dbContext.People.ToList();
            return View(people);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, configuration))
            {
                csv.Read();
                csv.ReadHeader();

                var people = new List<Person>();

                while (csv.Read())
                {
                    var person = new Person
                    {
                        Name = csv.GetField<string>("Name"),
                        DateOfBirth = csv.GetField<DateTime>("DateOfBirth"),
                        Married = csv.GetField<bool>("Married"),
                        Phone = csv.GetField<string>("Phone"),
                        Salary = csv.GetField<decimal>("Salary")
                    };

                    people.Add(person);

                }

                await _dbContext.AddRangeAsync(people);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var person = await _dbContext.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _dbContext.People.Remove(person);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, string field, string value)
        {
            var person = await _dbContext.People.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            switch (field.ToLower())
            {
                case "name":
                    person.Name = value;
                    break;
                case "dateofbirth":
                    if (DateTime.TryParse(value, out var dateOfBirth))
                    {
                        person.DateOfBirth = dateOfBirth;
                    }
                    else
                    {
                        ModelState.AddModelError("DateOfBirth", "Invalid date format.");
                        return BadRequest(ModelState);
                    }
                    break;
                case "married":
                    if (bool.TryParse(value, out var married))
                    {
                        person.Married = married;
                    }
                    else
                    {
                        ModelState.AddModelError("Married", "Invalid value for married.");
                        return BadRequest(ModelState);
                    }
                    break;
                case "phone":
                    person.Phone = value;
                    break;
                case "salary":
                    if (decimal.TryParse(value, out var salary))
                    {
                        person.Salary = salary;
                    }
                    else
                    {
                        ModelState.AddModelError("Salary", "Invalid salary value.");
                        return BadRequest(ModelState);
                    }
                    break;
                default:
                    ModelState.AddModelError("Field", "Invalid field.");
                    return BadRequest(ModelState);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to update record.");
                return BadRequest(ModelState);
            }

            return Ok();
        }

    }
}