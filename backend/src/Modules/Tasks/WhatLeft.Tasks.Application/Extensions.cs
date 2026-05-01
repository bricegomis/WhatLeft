using Microsoft.Extensions.DependencyInjection;
using WhatLeft.Tasks.Application.UseCases;

namespace WhatLeft.Tasks.Application;

public static class Extensions
{
    public static IServiceCollection AddTasksApplication(this IServiceCollection services)
    {
        services.AddScoped<TaskService>();
        services.AddScoped<RecurringTemplateService>();
        services.AddScoped<RecurringTaskProcessor>();

        // Registers TaskCompletedHandler and any other INotificationHandlers in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Extensions).Assembly));

        return services;
    }
}
