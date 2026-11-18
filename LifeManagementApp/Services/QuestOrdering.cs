using LifeManagementApp.Models;

namespace LifeManagementApp.Services;

public static class QuestOrdering
{
    // Filtering before taking three prevents completed quests filling empty dashboard slots.
    public static List<Quest> TopUnfinished(IEnumerable<Quest> quests) => quests
        .Where(quest => !quest.Completed)
        .OrderByDescending(quest => quest.Priority)
        .ThenBy(quest => quest.DueDate)
        .Take(3)
        .ToList();
}
