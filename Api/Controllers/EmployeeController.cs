using IntegrationTestAPI.Contracts;
using IntegrationTestAPI.Services;
using KeycloackTest.DTOMappers;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationTestAPI.Controllers;

[Route("api/v1")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(cancellationToken);
        var employeeDtos = employees.ToDTO();
        return Ok(employeeDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return NotFound();
        }

        var employeeDto = employee.ToDTO();
        return Ok(employeeDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDTO employeeDto, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeDto.Name) ||
         string.IsNullOrWhiteSpace(employeeDto.Email) ||
         string.IsNullOrWhiteSpace(employeeDto.Position) ||
         employeeDto.Salary <= 0)
        {
            return BadRequest("Invalid employee data.");
        }

        var success = await _employeeService.CreateEmployeeAsync(employeeDto, cancellationToken);

        if (!success)
        {
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return Ok(success);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequestDTO employee, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employee.Name) ||
         string.IsNullOrWhiteSpace(employee.Email) ||
         string.IsNullOrWhiteSpace(employee.Position) || employee.Id != id)
        {
            return BadRequest();
        }

        var result = await _employeeService.UpdateEmployeeAsync(employee, cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
