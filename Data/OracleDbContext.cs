using HR_Application.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

public class OracleDbContext
{
    private readonly string _connectionString;

    public OracleDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Employee> GetAllEmployees()
    {
        var employees = new List<Employee>();
        using (var connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            var command = new OracleCommand("SELECT * FROM Employees", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employees.Add(new Employee
                    {
                        EmployeeId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Division = reader.GetString(3),
                        Building = reader.GetString(4),
                        Title = reader.GetString(5),
                        Room = reader.GetString(6)
                    });
                }
            }
        }
        return employees;
    }

    public Employee GetEmployeeById(int employeeId)
    {
        Employee? employee = null;
        using (var connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            var command = new OracleCommand("SELECT * FROM Employees WHERE EmployeeId = :EmployeeId", connection);
            command.Parameters.Add(new OracleParameter("EmployeeId", employeeId));

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    employee = new Employee
                    {
                        EmployeeId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Division = reader.GetString(3),
                        Building = reader.GetString(4),
                        Title = reader.GetString(5),
                        Room = reader.GetString(6)
                    };
                }
            }
        }
        return employee;
    }

    public void AddOrUpdateEmployee(Employee employee)
    {
        using (var connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

           /* Console.WriteLine(employee.EmployeeId);

            Console.WriteLine(employee.FirstName);

            Console.WriteLine(employee.LastName);

            Console.WriteLine(employee.Division);

            Console.WriteLine(employee.Building);

            Console.WriteLine(employee.Title);

            Console.WriteLine(employee.Room);


            Console.WriteLine(employee);*/


            command.CommandText = @"
    UPDATE Employees SET
    FIRSTNAME = :FirstName,
    LASTNAME = :LastName,
    DIVISION = :Division,
    BUILDING = :Building,
    TITLE = :Title,
    ROOM = :Room
    WHERE EMPLOYEEID = :EmployeeId";

            // Bind the parameters from the 'employee' object to the query
            command.Parameters.Add(new OracleParameter("FirstName", employee.FirstName));
            command.Parameters.Add(new OracleParameter("LastName", employee.LastName));
            command.Parameters.Add(new OracleParameter("Division", employee.Division));
            command.Parameters.Add(new OracleParameter("Building", employee.Building));
            command.Parameters.Add(new OracleParameter("Title", employee.Title));
            command.Parameters.Add(new OracleParameter("Room", employee.Room));
            command.Parameters.Add(new OracleParameter("EmployeeId", employee.EmployeeId));

            // Execute the query
            using (var transaction = connection.BeginTransaction())
            {
                command.Transaction = transaction;
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rows updated successfully.");

                // Commit the transaction
                transaction.Commit();
            }

        }
    }

    public void DeleteEmployee(int employeeId)
    {
        using (var connection = new OracleConnection(_connectionString))
        {
            connection.Open();
            var command = new OracleCommand("DELETE FROM Employees WHERE EmployeeId = :EmployeeId", connection);
            command.Parameters.Add(new OracleParameter("EmployeeId", employeeId));
            command.ExecuteNonQuery();
        }
    }

    public void ImportXml()
    {
        string xmlFilePath = "./Assets/Employees.xml";
        // Load the XML document
        XDocument xmlDoc = XDocument.Load(xmlFilePath);

        // Extract Employee data from the XML
        var employees = xmlDoc.Descendants("Employee")
            .Select(x => new
            {
                EmployeeId = int.Parse(x.Element("EmployeeId")?.Value ?? "0"),
                FirstName = x.Element("FirstName")?.Value,
                LastName = x.Element("LastName")?.Value,
                Division = x.Element("Division")?.Value,
                Building = x.Element("Building")?.Value,
                Title = x.Element("Title")?.Value,
                Room = x.Element("Room")?.Value,
            }).ToList();

        // Open a connection to the Oracle database
        using (OracleConnection connection = new OracleConnection(_connectionString))
        {
            connection.Open();

            foreach (var employee in employees)
            {
                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                    INSERT INTO Employees (EmployeeId, FirstName, LastName, Division, Building, Title, Room)
                    VALUES (:EmployeeId, :FirstName, :LastName, :Division, :Building, :Title, :Room)";

                    // Add parameters
                    command.Parameters.Add(new OracleParameter("EmployeeId", employee.EmployeeId));
                    command.Parameters.Add(new OracleParameter("FirstName", employee.FirstName));
                    command.Parameters.Add(new OracleParameter("LastName", employee.LastName));
                    command.Parameters.Add(new OracleParameter("Division", employee.Division));
                    command.Parameters.Add(new OracleParameter("Building", employee.Building));
                    command.Parameters.Add(new OracleParameter("Title", employee.Title));
                    command.Parameters.Add(new OracleParameter("Room", employee.Room));

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
