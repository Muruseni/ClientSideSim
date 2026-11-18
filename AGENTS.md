# Project guidance for future work

Read `LifeManagementApp/README.md` and the GitHub issues before changing the app. Quest Life is a gamified life-management course project; meeting the course requirements and helping the student explain the code take priority over cosmetic additions. The Unity files are an earlier prototype; the MAUI app is in `LifeManagementApp/`.

## Requirements
- Target .NET 9 / MAUI 9, XAML and course conventions, including one class per file. The current .NET 8 C# UI is an unverified prototype, not the finished solution.
- Home: up to three **unfinished** items, priority descending then due date ascending; current temperature and wind in m/s; alert strictly above 5 m/s. Open-Meteo requests must include `wind_speed_unit=ms`.
- To-do page: CollectionView, sort by priority or due date, add with nonblank title; fields include ID, title, priority, due date and completion.
- Use MVVM, CommunityToolkit.Mvvm, XAML compiled bindings and a converter, async EF Core Code First SQLite, REST with HttpClient, DI and service interfaces. Aim for Shell navigation with parameters and a category relationship. Add xUnit coverage for business rules.
- Explain meaningful design decisions and keep documentation, AI use, limitations and validation honest. The student must be able to explain the whole app.

## Git workflow
- For each feature, first find or create a descriptive GitHub issue, then create a new feature branch from up-to-date main. Keep work scoped and use descriptive commits and PR descriptions linking the issue (e.g. `Closes #N`). Push the branch and use a PR to merge. Do not claim a merge, board move or branch deletion occurred without verifying it.
- Move issues through the GitHub Project as work progresses when access permits; close completed issues through the linked merge and delete feature branches after merge. If automation or access is unavailable, report it and keep the issue open. Preserve existing uncommitted work.
- Run relevant tests/builds when tooling exists; clearly report any unavailable checks. Never commit credentials.
