namespace LifeManagementApp.Models;
public enum QuestPriority { Low, Medium, High }
public class Quest
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public QuestPriority Priority { get; set; }
    public DateTime DueDate { get; set; } = DateTime.Today;
    public bool Completed { get; set; }
    public bool XpAwarded { get; set; }
    public string Details => $"{Priority} priority  ?  Due {DueDate:yyyy-MM-dd}";
    public string Status => Completed ? "Completed" : "Active";
    public string ActionLabel => Completed ? "Reopen" : "Complete +25 XP";
}
public class PlayerProgress { public int Id { get; set; } public int TotalXp { get; set; } }

