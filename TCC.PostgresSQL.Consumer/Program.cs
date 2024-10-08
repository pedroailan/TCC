using Prometheus;
using TCC.Commons;
using TCC.PostgreSQL.Consumer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddLogs();
builder.Services.AddSingleton<Consumer>();
builder.Services.AddSingleton<Response>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<Consumer>());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();

app.Run();
