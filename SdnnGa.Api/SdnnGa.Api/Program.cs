using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Photino.NET;
using Quartz;
using SdnnGa.Api.Desktop;
using SdnnGa.Core.Classes;
using SdnnGa.Core.Jobs;
using SdnnGa.Core.Listeners;
using SdnnGa.Database;
using SdnnGa.Database.Models;
using SdnnGa.Database.Repository;
using SdnnGa.Infrastructure.Quartz.Scheduler;
using SdnnGa.Infrastructure.Storage;
using SdnnGa.Infrastructure.Worker;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Infrastructure.Interfaces.Storage;
using SdnnGa.Model.Infrastructure.Interfaces.Worker;
using SdnnGa.Model.Services;
using SdnnGa.Services.AutoMapper;
using SdnnGa.Services.Service;
using SdnnGa.Services.SessionService;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace SdnnGa.Api;

/// <summary>
/// Десктопний застосунок SDNN-GA: локальний ASP.NET Core сервер (127.0.0.1) + нативне вікно (Photino / WebView2),
/// яке показує React-клієнт з wwwroot. Працює без інтернету: SQLite, файли на диску, локальний Python-воркер.
/// </summary>
public static class Program
{
    private const string SingleInstanceMutexName = "Local\\SdnnGa.Desktop.SingleInstance";

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex) when (ex is not HostAbortedException) // HostAbortedException — штатна зупинка інструментом dotnet ef
        {
            Console.WriteLine($"Fatal error: {ex}");
            ShowError($"SDNN-GA failed to start:\n\n{ex.Message}\n\nSee the log file for details: {FileLog.CurrentLogFile}");
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            // Шлях до appsettings.json і wwwroot не повинен залежати від поточної теки (запуск з ярлика).
            ContentRootPath = AppContext.BaseDirectory,
        });

        var desktopOptions = builder.Configuration.GetSection("Desktop").Get<DesktopOptions>() ?? new DesktopOptions();
        if (args.Contains("--headless", StringComparer.OrdinalIgnoreCase))
        {
            desktopOptions.Headless = true;
        }

        var appPaths = new AppPaths(desktopOptions.DataDirectory);
        FileLog.Initialize(appPaths.LogDirectory);
        Console.WriteLine($"Data directory: {appPaths.DataDirectory}");

        using var singleInstance = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!desktopOptions.Headless && !isFirstInstance)
        {
            ShowError("SDNN-GA is already running.");
            return 0;
        }

        // Сервер доступний лише з цього комп'ютера.
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.Listen(IPAddress.Loopback, desktopOptions.Port));

        ConfigureServices(builder, appPaths);

        var app = builder.Build();

        PrepareDatabase(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // React-клієнт (зібраний у wwwroot) + fallback для клієнтського роутингу (/session/...).
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.StartAsync().GetAwaiter().GetResult();

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();
        Console.WriteLine($"SDNN-GA is listening on {address}");

        if (desktopOptions.Headless)
        {
            Console.WriteLine("Headless mode: press Ctrl+C to stop.");
            app.WaitForShutdown();
            return 0;
        }

        RunWindow(address, desktopOptions, appPaths, app.Environment.IsDevelopment());

        Console.WriteLine("Window closed, shutting down...");
        app.StopAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        ((IDisposable)app).Dispose();

        return 0;
    }

    private static void ConfigureServices(WebApplicationBuilder builder, AppPaths appPaths)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton(appPaths);

        // AutoMapper
        builder.Services.AddAutoMapper(typeof(DtoProfile));

        // Локальний Python-воркер (замість RabbitMQ + окремих сервісів)
        var workerOptions = builder.Configuration.GetSection("Worker").Get<WorkerOptions>() ?? new WorkerOptions();
        builder.Services.AddSingleton(workerOptions);
        builder.Services.AddSingleton<IModelWorkerClient, PythonWorkerClient>();
        builder.Services.AddHostedService<WorkerWarmupService>();

        // Файли на локальному диску (замість Azure Blob Storage)
        builder.Services.AddSingleton<IStorage>(new FileSystemStorage(appPaths.StorageDirectory));

        // Quartz (in-memory, джоби створюються через DI). Fit-джоби чекають на створення своїх моделей,
        // тому потрібен запас паралельності.
        builder.Services.AddQuartz(q => q.UseDefaultThreadPool(threadPool => threadPool.MaxConcurrency = 100));

        // Не чекаємо завершення навчання при закритті вікна — воркер просто зупиняється.
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);
        builder.Services.AddSingleton<IJobScheduler, JobScheduler>();
        builder.Services.AddSingleton<ISchedulerService, SchedulerService>();

        // SQLite (замість PostgreSQL)
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = appPaths.DatabasePath,
            DefaultTimeout = 60,
        }.ToString();

        builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connectionString));

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
        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.AddScoped<IFitConfigService, FitConfigService>();
        builder.Services.AddScoped<IStatisticService, StatisticService>();
        builder.Services.AddScoped<IGeneticConfigService, GeneticConfigService>();

        // Core
        builder.Services.AddScoped<IModelGenerator, ModelGenerator>();

        builder.Services.AddScoped<ICreateModelJob, CreateModelJob>();
        builder.Services.AddScoped<IFitModelJob, FitModelJob>();
        builder.Services.AddScoped<IGeneticEpochJob, GeneticEpochJob>();

        builder.Services.AddScoped<ParentJobCompletionListenerFactory>();
    }

    private static void PrepareDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        dbContext.Database.Migrate();

        // WAL дозволяє читати базу (UI) під час запису результатів навчання.
        dbContext.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

        // Планувальник Quartz живе в пам'яті, тож навчання, яке йшло під час закриття застосунку, вже не завершиться.
        var interrupted = dbContext.NeuralNetworkModels
            .Where(model => !model.IsTrained && model.FitError == null)
            .ExecuteUpdate(setters => setters.SetProperty(
                model => model.FitError,
                "Interrupted: the application was closed before this model finished training."));

        if (interrupted > 0)
        {
            Console.WriteLine($"Marked {interrupted} unfinished model(s) from a previous run as interrupted.");
        }
    }

    private static void RunWindow(string address, DesktopOptions options, AppPaths appPaths, bool isDevelopment)
    {
        var window = new PhotinoWindow()
            .SetTitle(options.WindowTitle)
            .SetUseOsDefaultSize(false)
            .SetSize(options.WindowWidth, options.WindowHeight)
            .Center()
            .SetResizable(true)
            .SetDevToolsEnabled(isDevelopment)
            .SetContextMenuEnabled(isDevelopment)
            // WebView2 за замовчуванням пише профіль поруч з exe — у Program Files це заборонено.
            .SetTemporaryFilesPath(appPaths.WebViewDataDirectory);

        var iconPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "favicon.ico");
        if (File.Exists(iconPath))
        {
            window.SetIconFile(iconPath);
        }

        window.Load(new Uri(address));
        window.WaitForClose();
    }

    private static void ShowError(string message)
    {
        if (OperatingSystem.IsWindows())
        {
            MessageBoxW(IntPtr.Zero, message, "SDNN-GA", 0x00000010 /* MB_ICONERROR */);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
