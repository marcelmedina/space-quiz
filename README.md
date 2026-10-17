# Cosmic Quest

A space exploration quiz with 10 questions, colorful answer feedback, and a compact mission-control display. Built as a single, self-contained HTML file with no server or dependencies; the only network use is an optional leaderboard backed by an Azure Function.

## Play online

Play the live version at <https://marcelmedina.github.io/space-quiz/>. It is deployed automatically to GitHub Pages by [`.github/workflows/pages.yml`](.github/workflows/pages.yml) on every push to `main`.

## Play locally

Download or clone this repository, then open `index.html` directly in a web browser.

```powershell
git clone https://github.com/marcelmedina/space-quiz.git
Set-Location space-quiz
Start-Process .\index.html
```

## Features

- A welcome popup that requires your name and email before starting the quiz and mission timer.
- Ten multiple-choice questions with an explanatory fact after every answer.
- Live score, segmented progress bar, and completion percentage.
- Green feedback for correct answers and a red shake for incorrect selections.
- Results with an emoji reaction, accuracy, best streak, total mission time, and original-order or shuffled replay.
- An optional leaderboard submission that saves your name, email, score, and mission time through an Azure Functions API. The public top-10 feed excludes email.
- Optional 1–4 keyboard shortcuts for selecting answers.
- A centered, responsive layout with automatic light and dark themes.
- Native buttons, visible keyboard focus, accessible status updates, and reduced-motion support.

Enter your name and a valid email address and select Start quiz to begin. Both fields are mandatory. Your name is displayed throughout the quiz and results; your email is not. Details stay only in memory until you save a result and are retained for replay. Reloading returns to the popup and clears the details and quiz state.

Each question accepts one answer. Progress advances when an answer is selected; Next moves to the following question. Replay starts a new quiz with the same name.

## Leaderboard

The results screen offers **Save to leaderboard**, which sends your name, email, score, mission time, and question order to the Cosmic Quest API. Nothing is saved unless you select it, and each completed attempt can be saved once. The results screen then shows the top 10 explorers by score, then fastest time, without email addresses. No keys or secrets are embedded in the page.

The API is a .NET 10 Azure Functions app in [`api/`](api/), deployed to the `space-quiz` Function App by [`.github/workflows/main_space-quiz.yml`](.github/workflows/main_space-quiz.yml) on every push to `main`:

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/scores` | Validates and records `{ "name", "email", "score", "timeMs", "mode" }`. Name (1–80 characters) and a valid email (up to 254 characters, no whitespace/control characters) are required. Returns `201` with the leaderboard entry (excluding email), or `400` with `{ "error" }`. |
| `GET` | `/api/scores` | Returns the top 10 entries, excluding email addresses. |
| `GET` | `/api/scores/all` | Password-protected, read-only admin feed including email. Requires an `X-Admin-Password` header; missing/incorrect passwords return `401` before storage is accessed. Returns `{ "total", "generatedAt", "entries" }` ranked by highest score, then shortest quiz time, then earliest submission, before applying `?limit=` (default 200, max 1000). Older entries without email return an empty email string. |

Entries are stored in Azure Table Storage (table `scores`) in the Function App's `AzureWebJobsStorage` account.

### Admin dashboard

Open [`/admin/`](https://marcelmedina.github.io/space-quiz/admin/) (or `admin/index.html` locally) to watch the scoreboard. Higher scores always rank above lower scores; equal scores favor shorter quiz times (using full millisecond precision), and equal scores and times favor earlier submissions. The page opens with a password prompt and sends the entered password in the `X-Admin-Password` header over HTTPS. The API hashes it with SHA-256 and uses a constant-time comparison against its hard-coded hash before returning results. The frontend contains no password hash. The password stays only in browser memory for subsequent refreshes and is cleared on reload or authorization failure. Cross-origin requests preflight with `OPTIONS`; the API allows the password header. Ensure Function App platform CORS settings also allow this header if platform CORS is enabled. Never put the password in a URL or log request headers.

Scores are only ever written by the quiz's **Save to leaderboard** action. Once unlocked, the page refreshes automatically every 10 seconds (pausing while the tab is hidden), highlights new submissions, and offers a manual **Refresh now** button. Authorization failure hides the dashboard, clears displayed results, stops polling, and reopens the password prompt.

The Email column starts masked. Click its button (or activate with Enter/Space) to reveal that row's email, and activate it again to hide it. Refreshing masks emails again. Older submissions show **Not provided**. Masking reduces accidental on-screen disclosure; API access is independently checked server-side. This shared-password approach has no per-user identity or built-in rate limiting; a short password and its unsalted hash provide limited resistance to guessing.

To run the API locally, start [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) and then:

```powershell
Set-Location api
func start
```

## Development

All markup, styles, question data, and JavaScript are in `index.html`; the leaderboard API lives in `api/`. Edit and reload the browser to see changes. There is no build step, configured linter, or automated test runner.

First check that an empty or whitespace-only name, missing email, or malformed email cannot start the quiz, then enter both fields and start using Enter or the Start quiz button. The popup should trap focus and remain open on Escape. The timer should start only after submission, and focus should move to the first question.

For a quick single-question smoke check, select Saturn on question 1: the score should become 1 point, progress should reach 10%, and green feedback should appear. Reload, enter name and email, start, and select Jupiter to check red feedback, an unchanged score, and the correct answer reveal.

For a full smoke check, finish the quiz with mixed answers and verify the score, accuracy, best streak, results reaction, and replay reset. Check Tab/Shift+Tab navigation and Enter/Space activation, both color schemes, reduced motion, and narrow mobile layouts.

Repository-specific guidance is in [`.github/copilot-instructions.md`](.github/copilot-instructions.md).
