using LifeManagementApp.Data;
using LifeManagementApp.Services;
using LifeManagementApp.ViewModels;
using LifeManagementApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeManagementApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        // Each database operation creates a short-lived context; the factory can be shared.
        builder.Services.AddDbContextFactory<QuestDbContext>(options => options.UseSqlite(
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "quests.db")}"));
        builder.Services.AddSingleton<QuestService>();
        builder.Services.AddHttpClient<WeatherService>(client => client.Timeout = TimeSpan.FromSeconds(15));
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<QuestListViewModel>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<QuestListPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
