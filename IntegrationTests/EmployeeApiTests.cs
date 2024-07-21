using IntegrationTestAPI.Contracts;
using NerdCritica.Domain.Utils;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class EmployeeApiTests : IClassFixture<AppHostFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private AppHostFixture _fixture;

    public EmployeeApiTests(AppHostFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAllEmployees_ShouldReturnEmployeeList()
    {
        // Arrange
        var createEmployeeDto = new CreateEmployeeRequestDTO("John Doe", "john.doe@example.com", "Developer", 60000);

        var postResponse = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", createEmployeeDto);
        postResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("https://localhost:7033/api/v1");

        // Assert
        response.EnsureSuccessStatusCode();

        var employeeDtos = await response.Content.ReadFromJsonAsync<List<EmployeeResponseDTO>>();

        Assert.NotNull(employeeDtos);
        Assert.NotEmpty(employeeDtos);
        Assert.All(employeeDtos, dto =>
        {
            Assert.NotEmpty(dto.Name);
            Assert.NotEmpty(dto.Email);
            Assert.NotEmpty(dto.Position);
            Assert.True(dto.Salary > 0);
        });
    }

    [Fact]
    public async Task GetAllEmployees_ShouldReturnEmptyList_WhenNoEmployeesAreCreated()
    {
        // Act
        var response = await _client.GetAsync("https://localhost:7033/api/v1");

        // Assert
        response.EnsureSuccessStatusCode();

        var employeeDtos = await response.Content.ReadFromJsonAsync<List<EmployeeResponseDTO>>();

        Assert.NotNull(employeeDtos);
        Assert.Empty(employeeDtos); 
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var createEmployeeDto = new CreateEmployeeRequestDTO("Jane Doe", "jane.doe@example.com", "Designer", 55000);

        // Act
        var response = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", createEmployeeDto);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(result); 
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var invalidDto = new CreateEmployeeRequestDTO("", "", "", -1); // Dados inválidos

        // Act
        var response = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnNoContent_WhenDataIsValid()
    {
        // Arrange
        var createEmployeeDto = new CreateEmployeeRequestDTO("John Doe", "john.doe@example.com", "Developer", 60000);

        var postResponse = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", createEmployeeDto);
        postResponse.EnsureSuccessStatusCode();

        var employeesResponse = await _client.GetAsync("https://localhost:7033/api/v1");
        employeesResponse.EnsureSuccessStatusCode();
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeResponseDTO>>();

        ThrowHelper.ThrowIfNull(employees);

        var employee = employees.SingleOrDefault(e => e.Name == "John Doe");
        Assert.NotNull(employee);

        var updateEmployeeDto = new UpdateEmployeeRequestDTO(employee.Id, "John Smith", "john.smith@example.com", "Senior Developer", 80000);

        var putResponse = await _client.PutAsJsonAsync($"https://localhost:7033/api/v1/{employee.Id}", updateEmployeeDto);

        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var createEmployeeDto = new CreateEmployeeRequestDTO("Jane Doe", "jane.doe@example.com", "Manager", 75000);

        var postResponse = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", createEmployeeDto);
        postResponse.EnsureSuccessStatusCode();

        var employeesResponse = await _client.GetAsync("https://localhost:7033/api/v1");
        employeesResponse.EnsureSuccessStatusCode();
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeResponseDTO>>();

        ThrowHelper.ThrowIfNull(employees);

        var employee = employees.SingleOrDefault(e => e.Name == "Jane Doe");
        Assert.NotNull(employee);

        var invalidDto = new UpdateEmployeeRequestDTO(employee.Id, "", "", "", -1);

        var response = await _client.PutAsJsonAsync($"https://localhost:7033/api/v1/{employee.Id}", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task UpdateEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var updateEmployeeDto = new UpdateEmployeeRequestDTO(999, "Nonexistent Employee", "nonexistent@example.com", "Role", 100000);

        var response = await _client.PutAsJsonAsync("https://localhost:7033/api/v1/999", updateEmployeeDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ShouldReturnNoContent_WhenEmployeeExists()
    {
        // Arrange
        var createEmployeeDto = new CreateEmployeeRequestDTO("John Doe", "john.doe@example.com", "Developer", 60000);
        var postResponse = await _client.PostAsJsonAsync("https://localhost:7033/api/v1", createEmployeeDto);
        postResponse.EnsureSuccessStatusCode();

        var employeesResponse = await _client.GetAsync("https://localhost:7033/api/v1");
        employeesResponse.EnsureSuccessStatusCode();
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeResponseDTO>>();

        ThrowHelper.ThrowIfNull(employees);

        var employee = employees.SingleOrDefault(e => e.Name == "John Doe");
        Assert.NotNull(employee);

        // Act
        var deleteResponse = await _client.DeleteAsync($"https://localhost:7033/api/v1/{employee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("https://localhost:7033/api/v1/999"); // ID inexistente

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    public Task InitializeAsync()
       => Task.CompletedTask;

    public Task DisposeAsync()
        => _fixture.ResetAsync();
}
