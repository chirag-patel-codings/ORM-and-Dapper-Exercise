using System.Data;
using Dapper;
using ORM_Dapper;

public class DapperProductRepository : IProductRepository
{
    private readonly IDbConnection _connection;
    private string _sql = "";
    public DapperProductRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    /// <summary>
    /// Gets all the records for Products.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Product> GetAllProducts()
    {
        _sql =  "SELECT * FROM products"; 
        return _connection.Query<Product>(_sql);
    }


    /// <summary>
    /// Gets the details about the product for the supplied 'productID'.
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    public IEnumerable<Product> GetProductsByID(int productID)
    {
        _sql = "SELECT * FROM products WHERE ProductID = @ProductID;";
        return _connection.Query<Product>(_sql, new { ProductID = productID });
    }
    
    /// <summary>
    /// Adds a new product record into the Database, with Name, Price and CategoryID values.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="price"></param>
    /// <param name="categoryID"></param>
    public void CreateProduct(string name, double price, int categoryID)
    {
        _sql = "INSERT INTO products (Name, Price, CategoryID) VALUES(@Name, @Price, @CategoryID)";
        _connection.Execute(_sql, new {Name = name, Price = price, CategoryID = categoryID});
    }    

    /// <summary>
    /// Deletes the record for the product in the products Parent table for supplied 'productID' by first deleting the referring records in the 'reviews' and 'sales' Child Tables. 
    /// </summary>
    /// <param name="productID"></param>
    public void DeleteProductByID(int productID)
    {
        // Delete the records from Child Tables first...
        _sql = "DELETE FROM reviews WHERE ProductID = @ProductID;";
        _connection.Execute(_sql, new { ProductID = productID});

        _sql = "DELETE FROM sales WHERE ProductID = @ProductID;";
        _connection.Execute(_sql, new { ProductID = productID});

        // Delete the record from Parent Table... 
        _sql = "DELETE FROM products WHERE ProductID = @ProductID;";
        _connection.Execute(_sql, new { ProductID = productID });
    }

    /// <summary>
    /// Updates the product record with the New values supplied.
    /// </summary>
    /// <param name="product"></param>
    public void UpdateProduct(Product product)
    {
        _sql = "UPDATE products SET Name = @NewName, Price = @NewPrice, CategoryID = @NewCategoryID, OnSale = @NewOnSale, StockLevel = @NewStockLevel WHERE ProductID = @ProductID";
        _connection.Execute(_sql, new {NewName = product.Name, NewPrice = product.Price, NewCategoryID = product.CategoryID, NewOnSale = product.OnSale, NewStockLevel = product.StockLevel, ProductID = product.ProductID});

    }

}