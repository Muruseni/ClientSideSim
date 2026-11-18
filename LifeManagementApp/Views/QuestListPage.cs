using LifeManagementApp.Models;
using LifeManagementApp.ViewModels;
namespace LifeManagementApp.Views;
public class QuestListPage : ContentPage
{
    readonly QuestListViewModel vm;
    public QuestListPage(QuestListViewModel viewModel)
    {
        vm = viewModel; BindingContext = vm; Title = "Quests";
        var title = new Entry { Placeholder = "Quest title" }; title.SetBinding(Entry.TextProperty, nameof(vm.Title));
        var description = new Entry { Placeholder = "Description (optional)" }; description.SetBinding(Entry.TextProperty, nameof(vm.Description));
        var priority = new Picker { Title = "Priority", ItemsSource = vm.Priorities }; priority.SetBinding(Picker.SelectedItemProperty, nameof(vm.Priority));
        var date = new DatePicker(); date.SetBinding(DatePicker.DateProperty, nameof(vm.DueDate));
        var add = new Button { Text = "Add quest" }; add.SetBinding(Button.CommandProperty, nameof(vm.AddCommand));
        var status = new Label(); status.SetBinding(Label.TextProperty, nameof(vm.Status));
        var sort = new Picker { Title = "Sort quests", ItemsSource = vm.SortOptions }; sort.SetBinding(Picker.SelectedItemProperty, nameof(vm.Sort));
        var list = new VerticalStackLayout { Spacing = 8 }; BindableLayout.SetItemsSource(list, vm.Quests);
        BindableLayout.SetItemTemplate(list, new DataTemplate(() => {
            var name = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold }; name.SetBinding(Label.TextProperty, nameof(Quest.Title));
            var details = new Label(); details.SetBinding(Label.TextProperty, nameof(Quest.Details));
            var descriptionLabel = new Label(); descriptionLabel.SetBinding(Label.TextProperty, nameof(Quest.Description));
            var complete = new Button(); complete.SetBinding(Button.TextProperty, nameof(Quest.ActionLabel)); complete.Command = vm.ToggleCommand; complete.SetBinding(Button.CommandParameterProperty, new Binding("."));
            var delete = new Button { Text = "Delete", Command = vm.DeleteCommand }; delete.SetBinding(Button.CommandParameterProperty, new Binding("."));
            return new Border { Padding = 12, Content = new VerticalStackLayout { Children = { name, details, descriptionLabel, complete, delete } } };
        }));
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 24, Spacing = 10, Children = { new Label { Text = "QUEST BOARD", FontSize = 32, FontAttributes = FontAttributes.Bold }, new Label { Text = "Create a quest", FontSize = 22 }, title, description, priority, date, add, status, new Label { Text = "All quests", FontSize = 22 }, sort, list } } };
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.LoadAsync(); }
}
