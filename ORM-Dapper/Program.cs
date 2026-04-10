using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;


namespace ORM_Dapper
{
    public class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connString = config.GetConnectionString("DefaultConnection");
            
            IDbConnection conn = new MySqlConnection(connString);

            using (conn)
            {
                DapperDepartmentRepository dapperDepartmentRepository = new DapperDepartmentRepository(conn);

                // Insert a new record in the database
                Console.WriteLine("Would you like to view existing data or Insert a new record?");
                int resultUserSelection = 0;
                string userResponse = "";
                do
                {
                    Console.WriteLine("\nPlease enter:\n1 - To Insert New Record.\n2 - To View Existing Records");
                    userResponse = Console.ReadLine().Trim();

                }while(!(int.TryParse(userResponse, out resultUserSelection) && (resultUserSelection == 1 || resultUserSelection == 2)));
                
                userResponse = "";
                if (resultUserSelection == 1)
                {
                    do
                    {
                        Console.WriteLine("\nPlease provide a non-blank/empty value Name for the Department!");
                        userResponse = Console.ReadLine().Trim();
                    }while(userResponse.Trim() == "");
                    dapperDepartmentRepository.InsertDepartment(userResponse);
                    Console.WriteLine("New Dapartment added successully");
                    userResponse = "";
                    resultUserSelection = 2; // Retrieve the records from the database

                }

                if (resultUserSelection == 2)
                {
                    // Retrieve the data from database
                    var departments = dapperDepartmentRepository.GetAllDepartments();
                    Console.WriteLine("\nDisplaying data from database...\n");
                    Console.WriteLine("DepartmentID\t\tDepartmentName");
                    foreach(var department in departments)
                    {
                        Console.WriteLine($"{department.DepartmentID}\t\t\t{department.Name}");
                    }
                    Console.WriteLine();
                }

            }
        }

    }
}
