using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.RabbitMq;
using OrdersService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Online shop orders service API",
        Version = "v1",
        Description = "API для сервиса Orders"
    });
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null));
});

builder.Services.AddScoped<IOrdersService, OrderService>();
builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddHostedService<RabbitMqListener>();
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    MigrateDatabase(app);
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run("http://0.0.0.0:8080");

static void MigrateDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }   
}
