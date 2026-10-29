using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ClientSideSim
{
    public enum Priority { Low, Medium, High }

    [Serializable]
    public class TodoTask
    {
        public string id;
        public string title;
        public string description;
        public Priority priority;
        public string dueDate;
        public bool completed;
        public bool xpAwarded;
    }

    [Serializable]
    public class SaveData
    {
        public List<TodoTask> tasks = new List<TodoTask>();
        public int totalXp;
    }

    // Owns application rules; the UI only calls these operations.
    public class TaskStore
    {
        public SaveData Data { get; private set; } = new SaveData();
        public string Error { get; private set; }
        public int Level => Data.totalXp / 100 + 1;
        public int LevelXp => Data.totalXp % 100;
        readonly string path;
        bool saveBlocked;

        public TaskStore(string path) { this.path = path; }

        public static bool ValidDate(string value) => DateTime.TryParseExact(value,
            "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

        public bool Upsert(string id, string title, string description, Priority priority, string dueDate)
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(title)) { Error = "Please enter a task title."; return false; }
            if (!ValidDate(dueDate)) { Error = "Use a valid due date: yyyy-MM-dd."; return false; }
            var task = Data.tasks.Find(t => t.id == id);
            if (task == null)
            {
                task = new TodoTask { id = Guid.NewGuid().ToString() };
                Data.tasks.Add(task);
            }
            task.title = title.Trim();
            task.description = description == null ? "" : description.Trim();
            task.priority = priority;
            task.dueDate = dueDate;
            return Save();
        }

        public bool Toggle(string id, out int earned)
        {
            earned = 0;
            var task = Data.tasks.Find(t => t.id == id);
            if (task == null) return false;
            task.completed = !task.completed;
            if (task.completed && !task.xpAwarded)
            {
                earned = 25;
                task.xpAwarded = true;
                Data.totalXp += earned;
            }
            return Save();
        }

        public bool Delete(string id) { Data.tasks.RemoveAll(t => t.id == id); return Save(); }

        public IEnumerable<TodoTask> Ordered(bool byDate = false)
        {
            var unfinishedFirst = Data.tasks.OrderBy(t => t.completed);
            return byDate
                ? unfinishedFirst.ThenBy(t => t.dueDate, StringComparer.Ordinal).ThenByDescending(t => t.priority)
                : unfinishedFirst.ThenByDescending(t => t.priority).ThenBy(t => t.dueDate, StringComparer.Ordinal);
        }

        public void Load()
        {
            if (!File.Exists(path)) return;
            try
            {
                var loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (loaded == null || loaded.tasks == null || loaded.totalXp < 0 ||
                    loaded.tasks.Any(t => t == null || string.IsNullOrEmpty(t.id) ||
                    string.IsNullOrWhiteSpace(t.title) || !ValidDate(t.dueDate) ||
                    !Enum.IsDefined(typeof(Priority), t.priority)) ||
                    loaded.tasks.Select(t => t.id).Distinct().Count() != loaded.tasks.Count)
                    throw new InvalidDataException("Invalid save contents.");
                Data = loaded;
            }
            catch (Exception e)
            {
                saveBlocked = true;
                Error = "Cannot load saved data. Original file preserved; saving disabled. " + e.Message;
            }
        }

        public bool Save()
        {
            if (saveBlocked) { Error = "Saving disabled to preserve an unreadable save file."; return false; }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(Data, true));
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
                Error = null;
                return true;
            }
            catch (Exception e) { Error = "Changes remain in memory but could not be saved: " + e.Message; return false; }
        }
    }
}
