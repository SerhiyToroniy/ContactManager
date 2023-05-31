using Contact_Manager.Models;
using Contact_Manager.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Contact_Manager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _appDbContext;

        public HomeController(ILogger<HomeController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _appDbContext.CsvDataModels.ToListAsync();
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (model.CsvFile == null || model.CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Please select a CSV file to upload.");
                return RedirectToAction("Index", "Home");
            }

            using (var reader = new StreamReader(model.CsvFile.OpenReadStream()))
            {
                string line;
                var csvDataList = new List<CsvDataModel>();

                // Skip the header line if it exists
                var skipHeader = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (skipHeader)
                    {
                        skipHeader = false;
                        continue;
                    }

                    var fields = line.Split(',');

                    var csvData = new CsvDataModel
                    {
                        Name = fields[0],
                        DateOfBirth = DateTime.Parse(fields[1]),
                        Married = bool.Parse(fields[2]),
                        Phone = fields[3],
                        Salary = decimal.Parse(fields[4])
                    };

                    csvDataList.Add(csvData);
                }

                _appDbContext.CsvDataModels.AddRange(csvDataList);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var recordCSV = await _appDbContext.CsvDataModels.Where(q => q.Id == id).FirstOrDefaultAsync();
            if (recordCSV != null)
            {
                _appDbContext.Remove(recordCSV);
                await _appDbContext.SaveChangesAsync();

                // Return a JSON response indicating the success or failure of the delete operation
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Record not found." });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(CsvDataModel model)
        {
            var recordCSV = await _appDbContext.CsvDataModels.Where(q => q.Id == model.Id).FirstOrDefaultAsync();

            if (recordCSV != null)
            {
                // Update the properties of the existing record with the new values
                recordCSV.Name = model.Name;
                recordCSV.DateOfBirth = model.DateOfBirth;
                recordCSV.Married = model.Married;
                recordCSV.Phone = model.Phone;
                recordCSV.Salary = model.Salary;

                // Save the changes to the database
                await _appDbContext.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Record not found." });
        }

    }
}