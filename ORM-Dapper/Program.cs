using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using MySqlX.XDevAPI.Common;
using System.Collections.Frozen;
using System.Text;

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

                Console.WriteLine("What data you want to work with?");
                int resultUserSelection = 0;
                string userResponse = "";
                do
                {
                    Console.WriteLine("\nPlease enter:\n1 - For Departments.\n2 - For Products");
                    userResponse = Console.ReadLine().Trim();
                } while (!(int.TryParse(userResponse, out resultUserSelection) && (resultUserSelection == 1 || resultUserSelection == 2)));

                if (resultUserSelection == 1)
                {
                    goto DEPARTMENTS;
                }
                if (resultUserSelection == 2)
                {
                    goto PRODUCTS;
                }

            #region Departments

            DEPARTMENTS:

                DapperDepartmentRepository dapperDepartmentRepository = new DapperDepartmentRepository(conn);

                // Insert a new record in the database
                Console.WriteLine("Would you like to view existing departments data or Insert a new department record?");
                resultUserSelection = 0;
                userResponse = "";
                do
                {
                    Console.WriteLine("\nPlease enter:\n3 - To Insert New Record.\n4 - To View Existing Records");
                    userResponse = Console.ReadLine().Trim();

                } while (!(int.TryParse(userResponse, out resultUserSelection) && (resultUserSelection == 3 || resultUserSelection == 4)));

                userResponse = "";
                if (resultUserSelection == 3)
                {
                    do
                    {
                        Console.WriteLine("\nPlease provide a non-blank/empty value Name for the Department!");
                        userResponse = Console.ReadLine().Trim();
                    } while (userResponse.Trim() == "");
                    dapperDepartmentRepository.InsertDepartment(userResponse);
                    Console.WriteLine("New Dapartment added successully");
                    userResponse = "";
                    resultUserSelection = 4; // Retrieve the records from the database

                }

                if (resultUserSelection == 4)
                {
                    // Retrieve the data from database
                    var departments = dapperDepartmentRepository.GetAllDepartments();
                    Console.WriteLine("\nDisplaying data from database...\n");
                    DisplayResults(departments);
                }

                return;

            #endregion Departments


            #region Products

            PRODUCTS:

                DapperProductRepository dapperProductRepository = new DapperProductRepository(conn);

                Console.WriteLine("\nWould you like to view existing products data -or- Insert a new product record -or- Update any existing product details -or- Delete a product record?");
                resultUserSelection = 0;
                userResponse = "";
                do
                {
                    Console.WriteLine("\nPlease enter:\n3 - To Insert a New Record.\n4 - To View Existing Records.\n5 - To Update Existing Record.\n6 - To Delete Existing Record.");
                    userResponse = Console.ReadLine().Trim();

                } while (!(int.TryParse(userResponse, out resultUserSelection) && (resultUserSelection > 2 && resultUserSelection < 7)));

                // To Insert a New Record
                if (resultUserSelection == 3)
                {
                    Console.WriteLine("\nThis will automatically insert a new record into the products table");
                    string randomProductNameExt = "New Custom Product - " + long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff")).ToString();
                    Random random = new Random();
                    double productPrice = Math.Round(100 * random.NextDouble(), 2);
                    dapperProductRepository.CreateProduct(randomProductNameExt, productPrice, 10);

                    Console.WriteLine($"\nNew Product with Name: {randomProductNameExt}, and Price: {productPrice} has been successfully added");
                    resultUserSelection = 4;
                }

                // To View Existing Records
                if (resultUserSelection == 4)
                {
                    // Retrieve the data from database
                    Console.WriteLine("\nDisplaying data from database...\n");
                    Thread.Sleep(1000);
                    var products = dapperProductRepository.GetAllProducts();
                    DisplayResults(products);
                }

                int productID = 0;
                IEnumerable<Product>? product = null;
                // To Update Existing Record
                if (resultUserSelection == 5)
                {
                    userResponse = "";
                    do
                    {
                        Console.WriteLine("\nPlease enter ProductID as number for the product you want to update!");
                        userResponse = Console.ReadLine().Trim();
                        int.TryParse(userResponse, out productID);
                        if (productID != 0)
                        {
                            product = dapperProductRepository.GetProductsByID(productID);
                            if (product.Count() > 0)
                            {
                                Console.WriteLine("\nBelow are the details found for the product with " + productID + ":");
                                DisplayResults(product);

                                userResponse = "";
                                (Product? product, string errorMessages) productOrValidationErrors;
                                do
                                {
                                    Console.WriteLine("\nTo update: please provide new value(s) of *Name (string)|*Price (double)|*CategoryID (int)|OnSale (int)|StockLevel (string)");
                                    Console.WriteLine("in exact same order and data type each separated with (Pipe Character) '|'...\n '*' - Required Fields and other fields can be supplied blank with '|' separated!!!\n");
                                    userResponse = Console.ReadLine().Trim();
                                    productOrValidationErrors = ValidateAndMakeProduct(userResponse);
                                    if (productOrValidationErrors.product == null)
                                    {
                                        var consoleForegroundColor = Console.ForegroundColor;
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine(productOrValidationErrors.errorMessages);
                                        Console.ForegroundColor = consoleForegroundColor;
                                    }
                                } while (productOrValidationErrors.product == null);

                                productOrValidationErrors.product.ProductID = productID;
                                dapperProductRepository.UpdateProduct(productOrValidationErrors.product);
                                Console.WriteLine("Product Updated Successfully!!!\nCheck the details below!!!\n");
                                DisplayResults(dapperProductRepository.GetProductsByID(productID));
                                Console.WriteLine();

                            }
                            else
                            {
                                Console.WriteLine($"\nThe product with id: {productID} does not exists!!!");
                                productID = 0;
                            }
                        }
                    } while (productID == 0);
                }

                // To Delete Existing Record
                if (resultUserSelection == 6)
                {
                    Console.WriteLine("Are you sure you want to delete a product record?\nThis action can't be undo and it will permenantly delete that record from database!");

                    productID = 0;
                    userResponse = "";
                    product = null;
                    do
                    {
                        Console.WriteLine("\nPlease enter ProductID as number!");
                        userResponse = Console.ReadLine().Trim();
                        int.TryParse(userResponse, out productID);
                        if (productID != 0)
                        {
                            product = dapperProductRepository.GetProductsByID(productID);
                            if (product.Count() > 0)
                            {
                                Console.WriteLine("\nAre you sure, you want to delete below record???\n");
                                DisplayResults(product);
                            }
                            else
                            {
                                Console.WriteLine($"\nThe product with id: {productID} does not exists!!!");
                                productID = 0;
                            }
                        }
                    } while (productID == 0);

                    resultUserSelection = 0;
                    userResponse = "";
                    do
                    {
                        Console.WriteLine("\nenter: 99 - To Delete a record.\n100 - To Cancel this action");
                        userResponse = Console.ReadLine().Trim();
                    } while (!(int.TryParse(userResponse, out resultUserSelection) && (resultUserSelection == 99 || resultUserSelection == 100)));

                    if (resultUserSelection == 99)
                    {
                        dapperProductRepository.DeleteProductByID(productID);
                        Console.WriteLine($"\nDELETED: The product with id: {productID}!!!");
                    }

                }

                return;

            #endregion Products

            }
        }

        // Validates all the values supplied for the product. If all the values are correctly validated, it returns the 'Product' object with values and blank for errors, otherwise returns null for
        // the product object and errormessages
        static (Product?, string) ValidateAndMakeProduct(string pipeSeparatedProductDetails)
        {
            Product product = new Product();
            StringBuilder sbErrorMessage = new StringBuilder("Validation Error(s)!!!");
            double productPrice = 0;
            int categoryID = 0;
            int onSale = 0;
            int stockLevel = 0;

            if (!pipeSeparatedProductDetails.Contains("|"))
            {
                sbErrorMessage.Append("\nData supplied in incorrect format!!!");
                return (null, sbErrorMessage.ToString());
            }
            else
            {
                // *Name (string)|*Price (double)|*CategoryID (int)|OnSale (int)|StockLevel (string)
                string[] productDetails = pipeSeparatedProductDetails.Split("|");
                Array.ForEach(productDetails, pd => pd.Trim());

                if (productDetails.Count() > 4)
                {
                    bool isValidProduct = true;

                    if (productDetails[0] == ""
                            || productDetails[1] == ""
                            || productDetails[2] == "")
                    {
                        isValidProduct = false;
                        sbErrorMessage.Append("\nName (string), Price (double) and CategoryID (int) are required fields!!!");
                        //return (null, sbErrorMessage.ToString());
                    }

                    if (productDetails[0] != "")
                    {
                        product.Name = productDetails[0];
                    }

                    // Price (double)
                    if (productDetails[1] != "" && double.TryParse(productDetails[1], out productPrice))
                    {
                        product.Price = productPrice;
                    }
                    else
                    {
                        isValidProduct = false;
                        sbErrorMessage.Append("\nInvalid Field Value. Product price must be of Numeric (Decimals) type!!!");
                        //return (null, sbErrorMessage.ToString());
                    }

                    // CategoryID (int)
                    if (productDetails[2] != "" && int.TryParse(productDetails[2], out categoryID) && categoryID > 0)
                    {
                        product.CategoryID = categoryID;
                    }
                    else
                    {
                        isValidProduct = false;
                        sbErrorMessage.Append("\nInvalid Field Value. CategoryID must be of Numeric (Integer) type, greater than zero!!!");
                        // return (null, sbErrorMessage.ToString());
                    }

                    // OnSale (int)
                    if (productDetails[3] != "" && int.TryParse(productDetails[3], out onSale) && (onSale == 0 || onSale == 1))
                    {
                        product.OnSale = onSale;    // '0' bydefault
                    }
                    else
                    {
                        isValidProduct = false;
                        sbErrorMessage.Append("\nInvalid Field Value. OnSale value must be either '0' or '1'!!!");
                        // return (null, sbErrorMessage.ToString());
                    }

                    // StockLevel (string)
                    if (productDetails[4] != "" && int.TryParse(productDetails[4], out stockLevel))
                    {
                        product.StockLevel = stockLevel.ToString();    // if any correct value supplied
                    }
                    else
                    {
                        if (productDetails[4] != "")        // If the values supplied is incorrect then only as in database it allows nulls...
                        {
                            isValidProduct = false;
                            sbErrorMessage.Append("\nInvalid Field Value. StockLevel must be of Numeric (Integer) type!!!");
                            //return (null, sbErrorMessage.ToString());
                        }
                    }

                    if (!isValidProduct)
                    {
                        return (null, sbErrorMessage.ToString());
                    }
                }
                else
                {
                    sbErrorMessage.Append("\nInsufficient details (or Incorrect Format)!!! Cannot update product as not all (*) required fields are provided!!!");
                    return (null, sbErrorMessage.ToString());
                }
            }

            return (product, "");
        }

        // If the function definition: [static void DisplayResults<T>(T objects) where T : IEnumerable]  then 
        // ==> 'T' is 'IEnumerable<Department>' or 'IEnumerable<Product>' itself and 'objects' is the collection of them. 
        // Different way of implementation...Need to iterate through the object first to get 'IEnumerable<T>' from it and then get values from it...NOT SURE BUT RESEARCH HINT...
        
        // Below function Displays the values of the supplied IEnumerable<T> (Strongly typed for 'T')
        // T is 'Department' OR 'Product' in this scenario, 'objects' is the 'List<T> (IEnumerable<T>)'.
        // Values are directly accessed here...
        
        static void DisplayResults<T>(IEnumerable<T> objects)
        {
            // 1. Get the type of the object
            Type type = objects.First().GetType();

            // 2. Get all public instance properties
            PropertyInfo[] properties = type.GetProperties();

            // 3. Display Properties Names First
            Console.Write("|");
            foreach (var property in properties)
            {
                Console.Write(property.Name + "|");
            }
            Console.WriteLine();

            // 4. Display Properties Values
            foreach (var obj in objects)
            {
                Console.Write("|");
                foreach (var property in properties)
                {
                    Console.Write(property.GetValue(obj) + "|");
                }
                Console.WriteLine();
            }
        }
    }
}
