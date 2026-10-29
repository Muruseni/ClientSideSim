# ClientSideSim — Quest List

Unity/C# gamified to-do application for the Client-Side Programming course.

## Open and run

1. Install Unity **2022.3.62f1 LTS** through Unity Hub with desktop build support.
2. In Hub, select **Add project from disk**, then select this repository folder.
3. Allow asset import and Unity UI package resolution. Unity generates remaining default settings and metadata on first import.
4. Open `Assets/Scenes/Main.unity`, select a square or portrait Game view (ideally 1000 × 1000 or taller), and press Play.
5. For a desktop executable, open File → Build Settings, select your desktop platform, and build. Main is already registered.

No Inspector assignments are required. `TodoApp` creates a Todo Application object (TodoApp and WeatherService), Todo Canvas (Canvas, CanvasScaler, GraphicRaycaster), and EventSystem (EventSystem and StandaloneInputModule) at runtime. The Canvas contains navigation, weather controls, XP Slider, ScrollRect list, and task form. Edit `TodoApp.cs` to change the generated UI permanently.

Standard Unity UI Text/InputField is used to avoid TextMeshPro asset import and extra setup. The built-in font may lack the kite emoji; the visible Kitesurf weather! words remain. For exact emoji rendering, import and assign a suitable font in TodoApp. No server or API key is needed. Weather requires Internet access to `api.open-meteo.com`.

## Features and usage

- Enter a title, optional description, and valid due date (`yyyy-MM-dd`). Click Priority to cycle Low/Medium/High, then Save task. Blank titles are rejected.
- All tasks is scrollable; priority and date sorting keep unfinished tasks first. Home shows at most three tasks, unfinished first, priority descending, then due date ascending.
- Cards support Complete/Reopen, Edit, and Delete (immediate). New / cancel edit clears the form.
- Each task awards 25 XP once. Each 100 XP advances a level. Reopening/recompleting cannot farm XP; deleting does not remove earned XP.
- Weather defaults to Kyiv. Enter decimal latitude/longitude to change location for this session and refresh. Units are °C and m/s. Wind strictly greater than 5 m/s displays 🪁 Kitesurf weather! Network errors do not block tasks.
- Every task change saves to `Application.persistentDataPath/tasks.json`. Level derives from saved total XP; a previous save is retained in `tasks.json.bak`. Unreadable data is preserved with saving disabled for that session. Close the app and move a damaged save aside or restore its backup before restarting.

## Code guide

- `TaskStore.cs`: task/progression data, validation, ordering, task operations, XP, and JSON persistence. ISO dates sort in calendar order.
- `TodoApp.cs`: Canvas construction, form and task cards, button handlers, progress display.
- `WeatherService.cs`: timed UnityWebRequest and weather response/error handling.
- `Assets/Editor/TaskStoreChecks.cs`: checks under Quest List → Run task and persistence checks. Uses temporary files, not player saves.

## Verification

Run the Editor checks, then manually verify:

1. Blank titles and impossible dates are rejected. Create five tasks with different priorities/dates and long descriptions.
2. Scroll the list and switch sorting. Complete tasks and confirm Home shows at most three with unfinished first.
3. Edit, cancel an edit, delete, and verify the expected changes.
4. Complete four distinct tasks: level 2 and 0/100 XP. Reopen/recomplete: no extra XP.
5. Stop/restart Play mode: tasks, flags, descriptions, and XP survive.
6. Refresh weather, test invalid coordinates and offline behavior. Compare returned units with Open-Meteo. Verify the alert under wind greater than 5 m/s and clearing under lower wind.
7. Build a desktop player and verify input, layout, and save/restart.

The cloud environment has no Unity Editor or C# compiler. JSON and repository whitespace checks can run here; compilation, Editor checks, UI rendering/input, weather requests, and player builds remain unverified until run in Unity.
