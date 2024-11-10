using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SdnnGa.Database;
using SdnnGa.Database.Models;
using SdnnGa.Database.Repository;
using SdnnGa.Infrastructure.RebbitMq;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Infrastructure.Interfaces;
using SdnnGa.Model.Services;
using SdnnGa.Services.AutoMapper;
using SdnnGa.Services.SessionService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(typeof(DtoProfile));

// RebbitMQ
builder.Services.AddScoped<IRebbitMqClient, RebbitMqClient>(provider => new RebbitMqClient("rabbitmq-service", "request_queue", "response_queue"));

// DbContext
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));

// DbRepository
builder.Services.AddScoped<IDbRepository<DbSession>, DbRepository<DbSession>>();
builder.Services.AddScoped<IDbRepository<DbFitConfig>, DbRepository<DbFitConfig>>();
builder.Services.AddScoped<IDbRepository<DbGeneticConfig>, DbRepository<DbGeneticConfig>>();
builder.Services.AddScoped<IDbRepository<DbNeuralNetworkModel>, DbRepository<DbNeuralNetworkModel>>();
builder.Services.AddScoped<IDbRepository<DbEpoch>, DbRepository<DbEpoch>>();

builder.Services.AddScoped<ISessionService, SessionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
