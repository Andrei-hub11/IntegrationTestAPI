using IntegrationTestAPI.Context;
using IntegrationTestAPI.Services;

namespace KeycloackTest.DependencyInjection;

public static class ServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<DapperContext>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddHttpClient();

        return services;
    }
}
