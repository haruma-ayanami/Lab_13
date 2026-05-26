using Lab_13.Data;
using Lab_13.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Lab_13;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                string connectionString =
                    "Server=dbsrv\\VIP2025;Database=PhoneBookDB_Lab_12_Tkachev_AD_2407CA1;Trusted_Connection=True;TrustServerCertificate=True;";

                services.AddDbContext<ApplicationContext>(
                    options => options.UseSqlServer(connectionString),
                    contextLifetime: ServiceLifetime.Transient);

                services.AddTransient<ContactEditViewModel>();
                services.AddTransient<ContactsListViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}