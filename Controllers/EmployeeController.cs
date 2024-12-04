using HR_Application.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Xml.Linq;

public class EmployeeController : Controller
{
    private readonly OracleDbContext _dbContext;

    public EmployeeController(OracleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Employees()
    {
        var employees = _dbContext.GetAllEmployees();
        return View(employees);
    }

    public IActionResult GetEmployeeById(int id)
    {
        var employee = _dbContext.GetEmployeeById(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpGet]
    public IActionResult UpdateEmployee(int id)
    {
        var employee = _dbContext.GetEmployeeById(id);

        ViewData["EmployeeId"] = employee.EmployeeId;
        ViewData["FirstName"] = employee.FirstName;
        ViewData["LastName"] = employee.LastName;
        ViewData["Division"] = employee.Division;
        ViewData["Building"] = employee.Building;
        ViewData["Title"] = employee.Title;
        ViewData["Room"] = employee.Room;

        return View();
    }

    [HttpPost]
    public IActionResult UpdateEmployee(Employee employee)
    {
        _dbContext.AddOrUpdateEmployee(employee);
        return RedirectToAction("Employees");
    }

    public IActionResult Delete(int id)
    {
        _dbContext.DeleteEmployee(id);
        return RedirectToAction("Employees");
    }

    //[HttpPost, HttpPost]
    /*public IActionResult AddEmployee()
    {
        _dbContext.ImportXml();
        return View();
    }*/

}
