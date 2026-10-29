using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClientSideSim
{
    // Builds a Canvas at runtime: no Inspector references or prefab wiring required.
    public class TodoApp : MonoBehaviour
    {
        TaskStore store;
        WeatherService weather;
        Transform root;
        Font font;
        Text progress, status, weatherLabel, alert;
        Transform list;
        InputField title, description, due, latitude, longitude;
        Dropdown priority;
        Slider xp;
        string editingId;
        bool home = true, byDate;
        Button refresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindObjectOfType<TodoApp>() == null) new GameObject("Todo Application").AddComponent<TodoApp>();
        }

        void Start()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            store = new TaskStore(Path.Combine(Application.persistentDataPath, "tasks.json"));
            store.Load();
            weather = gameObject.AddComponent<WeatherService>();
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var canvas = new GameObject("Todo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1000, 1000);
            scaler.matchWidthOrHeight = 1;
            root = Panel("Application", canvas.transform);
            var rect = (RectTransform)root;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(24, 24); rect.offsetMax = new Vector2(-24, -24);
            Label(root, "QUEST LIST", 30);
            progress = Label(root, "", 22);
            var bar = new GameObject("XP progress", typeof(RectTransform), typeof(LayoutElement), typeof(Slider));
            bar.transform.SetParent(root, false); bar.GetComponent<LayoutElement>().preferredHeight = 16;
            var background = Image("Background", bar.transform, new Color(.15f, .18f, .25f)); Stretch(background.rectTransform);
            var fill = Image("Fill", bar.transform, new Color(.2f, .8f, .5f)); Stretch(fill.rectTransform);
            xp = bar.GetComponent<Slider>(); xp.fillRect = fill.rectTransform; xp.interactable = false;
            var nav = Row(root); Button(nav, "Home", () => { home = true; Render(); });
            Button(nav, "All tasks", () => { home = false; Render(); });
            var weatherPanel = Panel("Weather", root);
            Label(weatherPanel, "Weather location (default: Kyiv)", 18);
            var location = Row(weatherPanel);
            latitude = Input(location, "Latitude", "50.45"); longitude = Input(location, "Longitude", "30.52");
            refresh = Button(location, "Refresh weather", RefreshWeather);
            weatherLabel = Label(weatherPanel, "", 18);
            alert = Label(weatherPanel, "", 24); alert.color = new Color(1f, .8f, .15f);
            var sorts = Row(root);
            Button(sorts, "Sort: priority", () => { byDate = false; Render(); });
            Button(sorts, "Sort: due date", () => { byDate = true; Render(); });
            var scrollObject = new GameObject("Task list", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(LayoutElement), typeof(RectMask2D));
            scrollObject.transform.SetParent(root, false);
            scrollObject.GetComponent<Image>().color = new Color(.09f, .11f, .16f);
            scrollObject.GetComponent<LayoutElement>().flexibleHeight = 1;
            scrollObject.GetComponent<LayoutElement>().minHeight = 100;
            list = Panel("Content", scrollObject.transform);
            var content = (RectTransform)list;
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
            list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = scrollObject.GetComponent<ScrollRect>(); scroll.content = content;
            scroll.viewport = (RectTransform)scrollObject.transform; scroll.horizontal = false;
            Label(root, "Add / edit task", 22);
            title = Input(root, "Title (required)"); description = Input(root, "Description (optional)");
            var fields = Row(root);
            priority = PriorityDropdown(fields);
            due = Input(fields, "Due date: yyyy-MM-dd", DateTime.Today.ToString("yyyy-MM-dd"));
            var actions = Row(root); Button(actions, "Save task", SaveTask); Button(actions, "New / cancel edit", ResetForm);
            status = Label(root, store.Error ?? "Ready. Complete a task to earn 25 XP.", 18);
            Render(); RefreshWeather();
        }

        void Render()
        {
            progress.text = "Level " + store.Level + "  •  " + store.LevelXp + " / 100 XP";
            xp.value = store.LevelXp / 100f;
            foreach (Transform child in list) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var tasks = home ? store.Ordered().Take(3) : store.Ordered(byDate);
            Label(list, home ? "Your top 3 tasks" : "All tasks", 24);
            if (!tasks.Any()) Label(list, "No tasks yet. Add your first task below!", 20);
            foreach (var task in tasks)
            {
                var card = Panel("Task " + task.id, list);
                Label(card, (task.completed ? "✓ " : "○ ") + task.title, 22);
                Label(card, task.priority + " priority • Due " + task.dueDate, 18);
                if (!string.IsNullOrEmpty(task.description)) Label(card, task.description, 18);
                var buttons = Row(card);
                Button(buttons, task.completed ? "Reopen" : "Complete", () =>
                {
                    bool saved = store.Toggle(task.id, out int earned);
                    status.text = saved ? (earned > 0 ? "+25 XP! Great work!" : "Task updated. XP is awarded once per task.") : store.Error;
                    Render();
                });
                Button(buttons, "Edit", () =>
                {
                    editingId = task.id; title.text = task.title; description.text = task.description;
                    due.text = task.dueDate; priority.value = (int)task.priority;
                    status.text = "Editing: " + task.title;
                });
                Button(buttons, "Delete", () =>
                {
                    bool saved = store.Delete(task.id);
                    if (editingId == task.id) ResetForm();
                    status.text = saved ? "Task deleted." : store.Error; Render();
                });
            }
        }

        void SaveTask()
        {
            if (!store.Upsert(editingId, title.text, description.text, (Priority)priority.value, due.text))
            { status.text = store.Error; Render(); return; }
            ResetForm(); status.text = "Task saved."; Render();
        }
        void ResetForm()
        {
            editingId = null; title.text = ""; description.text = ""; priority.value = 0;
            due.text = DateTime.Today.ToString("yyyy-MM-dd"); status.text = "Creating a new task.";
        }
        void RefreshWeather()
        {
            if (!double.TryParse(latitude.text, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ||
                !double.TryParse(longitude.text, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) ||
                double.IsNaN(lat) || double.IsNaN(lon) || Math.Abs(lat) > 90 || Math.Abs(lon) > 180)
            { weatherLabel.text = "Enter latitude −90 to 90 and longitude −180 to 180."; return; }
            refresh.interactable = false; weatherLabel.text = "Loading weather…"; alert.text = "";
            StartCoroutine(weather.Fetch(lat, lon, (message, windy) =>
            { weatherLabel.text = message; alert.text = windy ? "🪁 Kitesurf weather!" : ""; refresh.interactable = true; }));
        }

        Transform Panel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup)); go.transform.SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>(); layout.spacing = 8;
            layout.padding = new RectOffset(8, 8, 8, 8); layout.childControlHeight = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return go.transform;
        }
        Transform Row(Transform parent)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup)); go.transform.SetParent(parent, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>(); layout.spacing = 8;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return go.transform;
        }
        Text Label(Transform parent, string value, int size)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size;
            text.color = Color.white; text.supportRichText = false;
            return text;
        }
        Image Image(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.color = color; return image;
        }
        void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        Button Button(Transform parent, string caption, UnityEngine.Events.UnityAction clicked)
        {
            var image = Image(caption, parent, new Color(.18f, .3f, .45f));
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(clicked);
            var element = image.gameObject.AddComponent<LayoutElement>(); element.preferredHeight = 42; element.flexibleWidth = 1;
            var text = Label(image.transform, caption, 18); text.alignment = TextAnchor.MiddleCenter; Stretch(text.rectTransform);
            return button;
        }
        InputField Input(Transform parent, string hint, string value = "")
        {
            var image = Image(hint, parent, new Color(.16f, .19f, .24f));
            var field = image.gameObject.AddComponent<InputField>(); field.targetGraphic = image;
            var element = image.gameObject.AddComponent<LayoutElement>(); element.preferredHeight = 40; element.flexibleWidth = 1;
            var text = Label(image.transform, "", 18); Stretch(text.rectTransform); text.rectTransform.offsetMin = new Vector2(8, 4);
            var placeholder = Label(image.transform, hint, 18); Stretch(placeholder.rectTransform); placeholder.color = Color.gray;
            field.textComponent = text; field.placeholder = placeholder; field.text = value; field.characterLimit = 1000;
            return field;
        }
        Dropdown PriorityDropdown(Transform parent)
        {
            // Cycle button avoids prefab/template dependencies while using a Dropdown as the selection model.
            var button = Button(parent, "Priority: Low", () => { });
            var dropdown = button.gameObject.AddComponent<Dropdown>();
            dropdown.options = new System.Collections.Generic.List<Dropdown.OptionData>
            { new Dropdown.OptionData("Low"), new Dropdown.OptionData("Medium"), new Dropdown.OptionData("High") };
            dropdown.enabled = false;
            dropdown.onValueChanged.AddListener(value => button.GetComponentInChildren<Text>().text = "Priority: " + (Priority)value);
            button.onClick.AddListener(() => dropdown.value = (dropdown.value + 1) % 3);
            return dropdown;
        }
    }
}
