# Amusing Mobile Design

## Goal

Build a separate mobile demo app for Android and iOS named `Amusing.Mobile`. The app lets festival visitors view the active Amusing Hengelo festival planning, search for choirs, select one or more choirs, and switch between the complete planning and their own selection.

The existing Blazor management app remains the source of truth and should not become the mobile UI. It will expose a small public read-only API for the mobile app.

## Chosen Approach

Use a new .NET MAUI Blazor Hybrid project named `Amusing.Mobile`.

This keeps the app deployable as a real Android/iOS app while allowing the UI to be built with Razor components, HTML, and CSS. It also leaves room for later native phone features such as notifications or navigation to stages, because the app is still a MAUI app.

The mobile app will retrieve current festival data through public read-only API endpoints added to the existing `Amusing` web app.

## Alternatives Considered

### Pure .NET MAUI

Pure MAUI is a strong fit for apps that need native controls and phone features from the start. It is not selected for the first demo because the current scope is mostly lists, search, filters, and local selection state. Those screens can be built faster with Blazor Hybrid.

### Blazor Web App or PWA

A web or PWA version would be quick to build and would avoid store distribution, but it would not demonstrate as a real Android app on the Samsung test phone. It also does not align as well with the later option to package the app for the Google Play Store and Apple App Store.

### Separate API Project

A separate API project would provide clean infrastructure separation, but for this demo it adds hosting and deployment overhead. The existing `Amusing` web app already has the database configuration and service layer needed to expose the read-only data safely.

## Public API

Add mobile-only read-only endpoints to the existing `Amusing` web app.

Initial endpoints:

- `GET /api/mobile/current-festival`
- `GET /api/mobile/current-performances`

The endpoints require no login because the same performance information is already publicly visible on the Amusing Hengelo website.

The API must remain limited to public festival data:

- festival id
- festival name
- festival date
- choir id
- choir name
- stage id
- stage name
- performance start time
- performance end time

The API must not expose personal data, management data, write operations, database connection details, or authentication state.

## Mobile App Behavior

On startup, the app tries to fetch the current festival planning from the API.

If fetching succeeds:

- store the planning locally as the latest cached planning;
- display the fresh planning;
- apply the locally saved choir selection.

If fetching fails and cached planning exists:

- display the cached planning;
- show a clear but non-blocking message that the last saved planning is being shown.

If fetching fails and no cached planning exists:

- show a clear offline message;
- provide a retry action.

## Selection Behavior

The app stores selected choir ids locally on the device.

The selected choir ids are kept separately from the cached planning. When showing "My selection", the app intersects the saved ids with the choirs available in the current or cached planning.

This means:

- choirs that do not participate in the active festival are ignored in the display;
- those saved ids do not cause errors;
- if a choir participates again in a later festival, it will appear again automatically in the user's selection.

## User Interface

The first demo version contains one primary screen:

- festival title and date;
- data status indicator, such as live or last saved planning;
- search field for choir names;
- segmented switch between `Alle koren` and `Mijn selectie`;
- list grouped by choir;
- each choir shows its planned performances with time and stage;
- selection control per choir, such as a checkbox or star;
- retry action when online data could not be loaded.

The UI should be touch-friendly and optimized for a Samsung phone demo. It should avoid advertisements, tracking prompts, login prompts, and unnecessary onboarding.

## Local Storage

Use local device storage:

- selected choir ids in MAUI preferences;
- cached planning as a JSON file in app data storage.

SQLite is intentionally not part of the first version because the expected dataset is small: historically about 120 choirs with 2 performances each, and currently around 80 choirs with about 160 performance rows.

## Error Handling

The app handles these situations:

- API unavailable;
- no internet connection;
- empty planning;
- invalid or incomplete API response;
- selected choir ids not present in the active planning.

The app must never fail because a previously selected choir is missing from the current festival.

## Testing

Initial verification should cover:

- API returns only public current festival performance data;
- app loads and displays fresh API data;
- app stores and reuses cached planning;
- app handles first launch without internet;
- app filters by search text;
- app switches between all choirs and selected choirs;
- saved selection survives app restart;
- missing selected choir ids are ignored safely.

Android deployment verification should include building and deploying an ARM64 Android debug build to the connected Samsung phone.

## Out of Scope for First Demo

The first demo does not include:

- App Store or Play Store publication;
- user login;
- advertisements;
- push notifications;
- route/navigation to stages;
- user accounts or cloud-synced selections;
- editing festival data from the mobile app.

These can be considered later after the organization has seen the demo and approved the direction.
