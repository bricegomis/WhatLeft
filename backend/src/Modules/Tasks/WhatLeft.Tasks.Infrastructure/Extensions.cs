using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhatLeft.Tasks.Domain.Repositories;
using WhatLeft.Tasks.Infrastructure.Persistence;

namespace WhatLeft.Tasks.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TasksDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Tasks")));

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
