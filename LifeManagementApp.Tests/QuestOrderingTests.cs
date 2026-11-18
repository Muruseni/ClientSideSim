using LifeManagementApp.Models;
using LifeManagementApp.Services;
using Xunit;

namespace LifeManagementApp.Tests;

public class QuestOrderingTests
{
    [Fact]
    public void TopUnfinished_ExcludesCompletedAndOrdersByPriorityThenDueDate()
    {
        var quests = new[] {
            new Quest { Id = 1, Priority = QuestPriority.Low, DueDate = new DateTime(2026, 1, 1) },
            new Quest { Id = 2, Priority = QuestPriority.High, DueDate = new DateTime(2026, 2, 2) },
            new Quest { Id = 3, Priority = QuestPriority.High, DueDate = new DateTime(2026, 2, 1) },
            new Quest { Id = 4, Priority = QuestPriority.High, DueDate = new DateTime(2025, 1, 1), Completed = true },
            new Quest { Id = 5, Priority = QuestPriority.Medium, DueDate = new DateTime(2026, 1, 1) }
        };
        Assert.Equal(new[] { 3, 2, 5 }, QuestOrdering.TopUnfinished(quests).Select(q => q.Id));
    }

    [Fact]
    public void TopUnfinished_DoesNotFillRemainingSlotsWithCompletedQuests()
    {
        var quests = new[] { new Quest { Id = 1 }, new Quest { Id = 2, Completed = true } };
        Assert.Equal(new[] { 1 }, QuestOrdering.TopUnfinished(quests).Select(q => q.Id));
    }

    [Fact]
    public void TopUnfinished_ReturnsEmptyWhenAllCompletedOrNoQuests()
    {
        Assert.Empty(QuestOrdering.TopUnfinished(new[] { new Quest { Completed = true } }));
        Assert.Empty(QuestOrdering.TopUnfinished(Array.Empty<Quest>()));
    }
}
