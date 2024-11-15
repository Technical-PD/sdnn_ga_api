using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SdnnGa.Database.Models;
using SdnnGa.Database.Repository;
using SdnnGa.Database;
using SdnnGa.Infrastructure.AzureBlobStorage;
using SdnnGa.Infrastructure.Quartz.Scheduler;
using SdnnGa.Infrastructure.RabbitMq;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Services;
using SdnnGa.Services.AutoMapper;
using SdnnGa.Services.Service;
using SdnnGa.Services.SessionService;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Core.Classes;
using SdnnGa.Core.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = new SdnnGa.Model.Configuration.ConfigurationProvider();
builder.Configuration.Bind("ConnectionStrings", configuration);
builder.Services.AddSingleton(configuration);

// AutoMapper
builder.Services.AddAutoMapper(typeof(DtoProfile));

// RebbitMQ
builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>(provider => new RabbitMqClient("rabbitmq-service", "request_queue", "response_queue"));

// Blob Storage
builder.Services.AddScoped<IAzureBlobProvider, AzureBlobProvider>();
builder.Services.AddScoped<IStorage, AzureBlobStorage>();

// Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);
builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
builder.Services.AddSingleton<ISchedulerService, SchedulerService>();

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

// Services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IEpochService, EpochService>();
builder.Services.AddScoped<IGeneticService, GeneticService>();
builder.Services.AddScoped<INeuralNetworkModelService, NeuralNetworkModelService>();

// Core
builder.Services.AddScoped<IModelGenerator, ModelGenerator>();

builder.Services.AddScoped<ICreateModelJob, CreateModelJob>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
