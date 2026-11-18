using LifeManagementApp.Models;
using Microsoft.EntityFrameworkCore;
namespace LifeManagementApp.Data;
public class QuestDbContext(DbContextOptions<QuestDbContext> options) : DbContext(options)
{
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<PlayerProgress> Progress => Set<PlayerProgress>();
}
