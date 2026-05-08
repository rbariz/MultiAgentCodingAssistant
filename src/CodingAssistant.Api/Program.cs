
using CodingAssistant.Api.GitHub;
using CodingAssistant.Api.Hubs;
using CodingAssistant.Api.Realtime;
using CodingAssistant.Application;
using CodingAssistant.Application.GitHub;
using CodingAssistant.Application.Realtime;
using CodingAssistant.Infrastructure;
using CodingAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddScoped<IGenerationRealtimeNotifier,
    SignalRGenerationRealtimeNotifier>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IGitHubExportService, GitHubExportService>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("WebCors", policy =>
    {
        policy
            .WithOrigins("https://localhost:7051", "http://localhost:5220")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpsCenter", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CodingAssistantDbContext>();
    await db.Database.MigrateAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}
app.UseHttpsRedirection();

app.UseCors("OpsCenter");
app.UseCors("WebCors");
app.MapControllers();
app.MapHub<GenerationHub>("/hubs/generations");
app.Run();