using Dapper;
using Org.BouncyCastle.Asn1.X509;
using System.Data;

namespace ORM_Dapper;

public class DapperDepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnection _connection;
    public DapperDepartmentRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public IEnumerable<Department> GetAllDepartments()
    {
        return _connection.Query<Department>("SELECT * FROM Departments");
    }

    public void InsertDepartment(string departmentName)
    {
        string sql = "INSERT INTO departments(Name) VALUES(@Name);";
        _connection.Execute(sql, new {Name = departmentName});
    }

}