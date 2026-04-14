using Dapper;
using Org.BouncyCastle.Asn1.X509;
using System.Data;

namespace ORM_Dapper;

public class DapperDepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnection _connection;
    private string _sql = "";
    public DapperDepartmentRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public IEnumerable<Department> GetAllDepartments()
    {
        _sql = "SELECT * FROM Departments";
        return _connection.Query<Department>(_sql);
    }

    public void InsertDepartment(string departmentName)
    {
        _sql = "INSERT INTO departments(Name) VALUES(@Name);";
        _connection.Execute(_sql, new {Name = departmentName});
    }

}