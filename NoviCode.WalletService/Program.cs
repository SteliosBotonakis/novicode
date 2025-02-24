using Microsoft.EntityFrameworkCore;
using NoviCode.EcbGateway.Services;
using NoviCode.WalletService.Configuration;
using NoviCode.WalletService.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDb")));

// Register the ECB Gateway from Task 1.
builder.Services.AddHttpClient<IEcbGateway, EcbGateway>();

// Register the hosted service for periodic updates.
builder.Services.AddHostedService<EcbRatesUpdateHostedService>();

builder.Services.AddSingleton<IStrategyFactory, StrategyFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();