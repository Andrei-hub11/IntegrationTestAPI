namespace IntegrationTestAPI.Contracts;

public record UpdateEmployeeRequestDTO(int Id, string Name, string Email, string Position, decimal Salary);
