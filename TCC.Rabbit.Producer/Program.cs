using Prometheus;
using TCC.Commons;
using TCC.Rabbit.Producer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddLogs();
builder.Services.AddSingleton<Producer>();
builder.Services.AddSingleton<ConsumerResponse>();
builder.Services.AddHostedService<Job>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();

ConsumerResponse listen = app.Services.GetRequiredService<ConsumerResponse>();
listen.StartConsuming();

app.Run();
