using System.Net.Http.Headers;
using System.Data;
namespace ORM_Dapper;
interface IDepartmentRepository
{
    IEnumerable<Department> GetAllDepartments();
}