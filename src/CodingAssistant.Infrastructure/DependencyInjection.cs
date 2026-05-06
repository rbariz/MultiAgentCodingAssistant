using CodingAssistant.Application.Abstractions;
using CodingAssistant.Application.AI.Services;
using CodingAssistant.Infrastructure.AI.Ollama;
using CodingAssistant.Infrastructure.Persistence;
using CodingAssistant.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<CodingAssistantDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"))
                    .UseSnakeCaseNamingConvention();
             });

            services.Configure<OllamaOptions>(
    configuration.GetSection("Ollama"));

            services.AddHttpClient<ILlmClient, OllamaClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });


            services.AddScoped<IProjectGenerationRepository, ProjectGenerationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
