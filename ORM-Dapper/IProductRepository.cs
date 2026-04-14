using System.Runtime.CompilerServices;
using ORM_Dapper;

public interface IProductRepository
{
    IEnumerable<Product> GetAllProducts();
    void CreateProduct(string name, double price, int categoryID);
    void DeleteProductByID(int productID);
    void UpdateProduct(Product product);

}
