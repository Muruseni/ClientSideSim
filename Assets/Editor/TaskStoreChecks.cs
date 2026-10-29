using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ClientSideSim
{
    public static class TaskStoreChecks
    {
        [MenuItem("Quest List/Run task and persistence checks")]
        public static void Run()
        {
            var dir = Path.Combine(Path.GetTempPath(), "quest-" + Guid.NewGuid());
            var path = Path.Combine(dir, "tasks.json");
            try
            {
                var store = new TaskStore(path);
                Check(!store.Upsert(null, "  ", "", Priority.Low, "2026-01-01"), "Blank title");
                Check(!store.Upsert(null, "Bad", "", Priority.Low, "2026-02-30"), "Invalid date");
                Check(store.Upsert(null, "Low", "", Priority.Low, "2026-01-01"), "Create");
                Check(store.Upsert(null, "Later", "", Priority.High, "2026-02-02"), "Create");
                Check(store.Upsert(null, "Early", "", Priority.High, "2026-02-01"), "Create");
                Check(store.Ordered().First().title == "Early", "Priority/date sorting");
                Check(store.Ordered(true).First().title == "Low", "Date sorting");
                var id = store.Ordered().First().id;
                Check(store.Toggle(id, out int earned) && earned == 25, "XP award");
                Check(store.Ordered().Last().id == id, "Completed last");
                store.Toggle(id, out _); store.Toggle(id, out earned);
                Check(earned == 0 && store.Data.totalXp == 25, "No repeat XP");
                Check(store.Upsert(id, "Edited", "Details", Priority.Medium, "2026-03-01"), "Edit");
                var loaded = new TaskStore(path); loaded.Load();
                Check(loaded.Error == null && loaded.Data.tasks.Count == 3 && loaded.Data.totalXp == 25, "Persistence");
                Check(loaded.Data.tasks.Find(t => t.id == id).title == "Edited", "Edit persisted");
                Check(loaded.Delete(id) && loaded.Data.tasks.Count == 2, "Delete");
                loaded.Data.totalXp = 125;
                Check(loaded.Level == 2 && loaded.LevelXp == 25, "Level calculation");
                File.WriteAllText(path, "broken");
                var broken = new TaskStore(path); broken.Load();
                Check(broken.Error != null && !broken.Save() && File.ReadAllText(path) == "broken", "Corrupt save preserved");
                Debug.Log("Quest List: all task and persistence checks passed.");
            }
            finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
        }
        static void Check(bool success, string message)
        { if (!success) throw new Exception("Quest List check failed: " + message); }
    }
}
