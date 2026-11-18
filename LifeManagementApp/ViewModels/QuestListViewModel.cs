using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifeManagementApp.Models;
using LifeManagementApp.Services;
namespace LifeManagementApp.ViewModels;
public partial class QuestListViewModel(QuestService service) : ObservableObject
{
    public ObservableCollection<Quest> Quests { get; } = new();
    public Array Priorities => Enum.GetValues(typeof(QuestPriority));
    public string[] SortOptions { get; } = ["Priority", "Due date"];
    [ObservableProperty] string title = "";
    [ObservableProperty] string description = "";
    [ObservableProperty] QuestPriority priority;
    [ObservableProperty] DateTime dueDate = DateTime.Today;
    [ObservableProperty] string sort = "Priority";
    [ObservableProperty] string status = "";
    partial void OnSortChanged(string value) { _ = LoadAsync(); }
    [RelayCommand] public async Task LoadAsync()
    {
        try { await service.InitializeAsync(); var items = await service.ListAsync(Sort == "Due date"); Quests.Clear(); foreach (var q in items) Quests.Add(q); }
        catch (Exception) { Status = "Could not load quests. Please try again."; }
    }
    [RelayCommand] async Task AddAsync()
    {
        try { await service.InitializeAsync(); await service.AddAsync(Title, Description ?? "", Priority, DueDate); Title = ""; Description = ""; Priority = QuestPriority.Low; DueDate = DateTime.Today; Status = "Quest added!"; await LoadAsync(); }
        catch (ArgumentException e) { Status = e.Message; }
        catch (Exception) { Status = "Could not save your quest. Please try again."; }
    }
    [RelayCommand] async Task ToggleAsync(Quest quest) { try { await service.ToggleAsync(quest.Id); await LoadAsync(); Status = "Quest updated."; } catch (Exception) { Status = "Could not update your quest."; } }
    [RelayCommand] async Task DeleteAsync(Quest quest) { try { await service.DeleteAsync(quest.Id); await LoadAsync(); Status = "Quest deleted."; } catch (Exception) { Status = "Could not delete your quest."; } }
}
