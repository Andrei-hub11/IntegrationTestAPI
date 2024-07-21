using IntegrationTestAPI.Contracts;
using IntegrationTestAPI.Models;

namespace KeycloackTest.DTOMappers;

public static class MappingExtensions
{
    public static EmployeeResponseDTO ToDTO(this Employee employee)
    {
        return new EmployeeResponseDTO(employee.Id, employee.Name, employee.Email, employee.Position, employee.Salary);
    }

    public static IReadOnlyList<EmployeeResponseDTO> ToDTO(this IEnumerable<Employee> employees)
    {
        return employees.Select(employee => employee.ToDTO()).ToList();
    }
}

