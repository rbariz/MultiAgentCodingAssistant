using CodingAssistant.Application.AI.Services;
using CodingAssistant.Application.Generations.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProjectGenerationService, ProjectGenerationService>();
            services.AddScoped<IProjectGenerationOrchestrator, ProjectGenerationOrchestrator>();
            services.AddScoped<IProjectExportService, ProjectExportService>();

            services.AddScoped<ICodeGenerationAgent, CodeGenerationAgent>();

            services.AddScoped<IProjectPlannerAgent, ProjectPlannerAgent>();

            services.AddScoped<IFileGenerationAgent, FileGenerationAgent>();

            services.AddScoped<IProjectReviewerAgent, ProjectReviewerAgent>();

            services.AddSingleton<IAiJsonParser, AiJsonParser>();

            services.AddSingleton<IAiRetryPolicy, AiRetryPolicy>();

            return services;
        }
    }
}
