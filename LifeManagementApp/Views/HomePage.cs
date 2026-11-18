using LifeManagementApp.ViewModels;
using LifeManagementApp.Models;
namespace LifeManagementApp.Views;
public class HomePage : ContentPage
{
    readonly HomeViewModel vm; bool weatherLoaded;
    public HomePage(HomeViewModel viewModel)
    {
        vm = viewModel; BindingContext = vm; Title = "Home";
        var progress = new Label { FontSize = 20 }; progress.SetBinding(Label.TextProperty, nameof(vm.Progress));
        var bar = new ProgressBar { ProgressColor = Colors.MediumSeaGreen }; bar.SetBinding(ProgressBar.ProgressProperty, nameof(vm.ProgressValue));
        var weather = new Label { FontSize = 18 }; weather.SetBinding(Label.TextProperty, nameof(vm.WeatherText));
        var alert = new Label { TextColor = Colors.DarkOrange, FontAttributes = FontAttributes.Bold }; alert.SetBinding(Label.TextProperty, nameof(vm.WeatherAlert));
        var refresh = new Button { Text = "Refresh weather" }; refresh.SetBinding(Button.CommandProperty, nameof(vm.RefreshWeatherCommand));
        var status = new Label(); status.SetBinding(Label.TextProperty, nameof(vm.Status));
        var list = new VerticalStackLayout { Spacing = 8 };
        BindableLayout.SetItemsSource(list, vm.TopQuests);
        BindableLayout.SetItemTemplate(list, new DataTemplate(() => { var title = new Label { FontAttributes = FontAttributes.Bold, FontSize = 18 }; title.SetBinding(Label.TextProperty, nameof(Quest.Title)); var details = new Label(); details.SetBinding(Label.TextProperty, nameof(Quest.Details)); var state = new Label(); state.SetBinding(Label.TextProperty, nameof(Quest.Status)); return new Border { Padding = 12, Content = new VerticalStackLayout { Children = { title, details, state } } }; }));
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 24, Spacing = 12, Children = { new Label { Text = "QUEST LIFE", FontSize = 32, FontAttributes = FontAttributes.Bold }, new Label { Text = "Make everyday progress an adventure." }, progress, bar, new Label { Text = "Weather ? Kyiv", FontSize = 22 }, weather, alert, refresh, new Label { Text = "Your top 3 quests", FontSize = 22 }, status, list } } };
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.LoadAsync(); if (!weatherLoaded) { weatherLoaded = true; await vm.RefreshWeatherAsync(); } }
}
