using Dapper;
using IntegrationTestAPI.Context;
using IntegrationTestAPI.Contracts;
using IntegrationTestAPI.Models;

namespace IntegrationTestAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly DapperContext _context;

    public EmployeeService(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM Employees";

        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryAsync<Employee>(new CommandDefinition(query, cancellationToken: cancellationToken));
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM Employees WHERE Id = @Id";

        using (var connection = _context.CreateConnection())
        {
            return await connection.QuerySingleOrDefaultAsync<Employee>(new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));
        }
    }

    public async Task<bool> CreateEmployeeAsync(CreateEmployeeRequestDTO employee, CancellationToken cancellationToken)
    {
        var query = "INSERT INTO Employees (Name, Email, Position, Salary) VALUES (@Name, @Email, @Position, @Salary)";

        using (var connection = _context.CreateConnection())
        {
            var result = await connection.ExecuteAsync(new CommandDefinition(query, employee, cancellationToken: cancellationToken));
            return result > 0;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(UpdateEmployeeRequestDTO employee, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = "UPDATE Employees SET Name = @Name, Email = @Email, Position = @Position, Salary = @Salary WHERE Id = @Id";

        using (var connection = _context.CreateConnection())
        {
            var result = await connection.ExecuteAsync(query, employee);
            return result > 0;
        }
    }

    public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken)
    {
        var query = "DELETE FROM Employees WHERE Id = @Id";

        using (var connection = _context.CreateConnection())
        {
            var result = await connection.ExecuteAsync(new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));
            return result > 0;
        }
    }
}

