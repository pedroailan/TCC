using Prometheus;
using System.Collections.Generic;
using TCC.PostgreSQL.Producer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Producer>();
builder.Services.AddSingleton<ConsumerResponse>();
builder.Services.AddHostedService<Job>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ConsumerResponse>());

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();


app.MapControllers();

app.Run();
