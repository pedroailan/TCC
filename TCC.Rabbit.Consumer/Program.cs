using Prometheus;
using TCC.Commons;
using TCC.Rabbit.Consumer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddLogs();
builder.Services.AddSingleton<Consumer>();
builder.Services.AddSingleton<Response>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();

Consumer consumer = app.Services.GetRequiredService<Consumer>();
consumer.StartConsuming();

app.Run();
