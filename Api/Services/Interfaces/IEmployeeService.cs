using IntegrationTestAPI.Contracts;
using IntegrationTestAPI.Models;

namespace IntegrationTestAPI.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken);
    Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> CreateEmployeeAsync(CreateEmployeeRequestDTO employee, CancellationToken cancellationToken);
    Task<bool> UpdateEmployeeAsync(UpdateEmployeeRequestDTO employee, CancellationToken cancellationToken);
    Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken);
}
