using LifeManagementApp.Views;
namespace LifeManagementApp;
public class App : Application
{
    public App(HomePage home, QuestListPage quests)
    {
        var tabs = new TabbedPage { Title = "Quest Life" };
        tabs.Children.Add(home);
        tabs.Children.Add(quests);
        MainPage = tabs;
    }
}
