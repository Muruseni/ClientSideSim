using LifeManagementApp.Data;
using LifeManagementApp.Models;
using Microsoft.EntityFrameworkCore;
namespace LifeManagementApp.Services;
public class QuestService(IDbContextFactory<QuestDbContext> factory)
{
    public async Task InitializeAsync() { await using var db = await factory.CreateDbContextAsync(); await db.Database.EnsureCreatedAsync(); }
    public async Task<List<Quest>> ListAsync(bool byDate = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        var quests = await db.Quests.AsNoTracking().ToListAsync();
        var ordered = quests.OrderBy(q => q.Completed);
        return (byDate ? ordered.ThenBy(q => q.DueDate).ThenByDescending(q => q.Priority) : ordered.ThenByDescending(q => q.Priority).ThenBy(q => q.DueDate)).ToList();
    }
    public async Task<int> XpAsync() { await using var db = await factory.CreateDbContextAsync(); return (await db.Progress.FindAsync(1))?.TotalXp ?? 0; }
    public async Task AddAsync(string title, string description, QuestPriority priority, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Enter a quest title.");
        await using var db = await factory.CreateDbContextAsync();
        db.Quests.Add(new Quest { Title = title.Trim(), Description = description.Trim(), Priority = priority, DueDate = date.Date });
        await db.SaveChangesAsync();
    }
    public async Task ToggleAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var quest = await db.Quests.FindAsync(id);
        if (quest is null) return;
        quest.Completed = !quest.Completed;
        // Remember the award on the quest so reopening cannot award XP a second time.
        if (quest.Completed && !quest.XpAwarded)
        {
            var player = await db.Progress.FindAsync(1);
            if (player is null) { player = new PlayerProgress { Id = 1 }; db.Progress.Add(player); }
            player.TotalXp += 25; quest.XpAwarded = true;
        }
        await db.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id) { await using var db = await factory.CreateDbContextAsync(); var q = await db.Quests.FindAsync(id); if (q != null) { db.Quests.Remove(q); await db.SaveChangesAsync(); } }
}
