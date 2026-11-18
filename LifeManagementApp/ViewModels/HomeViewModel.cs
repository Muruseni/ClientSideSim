using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifeManagementApp.Models;
using LifeManagementApp.Services;
namespace LifeManagementApp.ViewModels;
public partial class HomeViewModel(QuestService quests, WeatherService weather) : ObservableObject
{
    public ObservableCollection<Quest> TopQuests { get; } = new();
    [ObservableProperty] string progress = "Loading progress?";
    [ObservableProperty] double progressValue;
    [ObservableProperty] string weatherText = "Loading weather?";
    [ObservableProperty] string weatherAlert = "";
    [ObservableProperty] string status = "";
    // Pages invoke the commands while the view model owns the dashboard state.
    [RelayCommand] public async Task LoadAsync()
    {
        try
        {
            await quests.InitializeAsync();
            var list = await quests.ListAsync(); TopQuests.Clear(); foreach (var quest in list.Take(3)) TopQuests.Add(quest);
            var xp = await quests.XpAsync(); Progress = $"Level {xp / 100 + 1}  ?  {xp % 100} / 100 XP"; ProgressValue = (xp % 100) / 100d;
            Status = list.Count == 0 ? "No quests yet. Add one on the Quests tab!" : "";
        }
        catch (Exception) { Status = "Could not load quests. Please restart the app and try again."; }
    }
    [RelayCommand] public async Task RefreshWeatherAsync()
    {
        WeatherText = "Loading weather?"; WeatherAlert = "";
        try { var result = await weather.GetAsync(50.45, 30.52); WeatherText = result.Summary; WeatherAlert = result.Alert; }
        catch (Exception) { WeatherText = "Weather unavailable. Check your connection and try again."; }
    }
}
