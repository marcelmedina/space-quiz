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

- A welcome screen that requires your name before starting the quiz and mission timer.
- Ten multiple-choice questions with an explanatory fact after every answer.
- Live score, segmented progress bar, and completion percentage.
- Green feedback for correct answers and a red shake for incorrect selections.
- Results with an emoji reaction, accuracy, best streak, total mission time, and original-order or shuffled replay.
- An optional public leaderboard that saves your name, score, and mission time through an Azure Functions API.
- Optional 1–4 keyboard shortcuts for selecting answers.
- A centered, responsive layout with automatic light and dark themes.
- Native buttons, visible keyboard focus, accessible status updates, and reduced-motion support.

Enter your name and select Start quiz to begin. Your name is displayed throughout the quiz and results, stays only in memory, and is retained for replay. Reloading returns to the welcome screen and clears the name and quiz state.

Each question accepts one answer. Progress advances when an answer is selected; Next moves to the following question. Replay starts a new quiz with the same name.

## Leaderboard

The results screen offers **Save to leaderboard**, which sends your name, score, mission time, and question order to the Cosmic Quest API. Nothing is saved unless you select it, and each completed attempt can be saved once. The results screen then shows the top 10 explorers by score, then fastest time. No keys or secrets are embedded in the page.

The API is a .NET 10 Azure Functions app in [`api/`](api/), deployed to the `space-quiz` Function App by [`.github/workflows/main_space-quiz.yml`](.github/workflows/main_space-quiz.yml) on every push to `main`:

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/scores` | Validates and records `{ "name", "score", "timeMs", "mode" }`. Returns `201` with the stored entry, or `400` with `{ "error" }`. |
| `GET` | `/api/scores` | Returns the top 10 entries. |
| `GET` | `/api/scores/all` | Read-only admin feed. Returns `{ "total", "generatedAt", "entries" }` with the latest submissions first (`?limit=`, default 200, max 1000). |

Entries are stored in Azure Table Storage (table `scores`) in the Function App's `AzureWebJobsStorage` account.

### Admin dashboard

Open [`/admin/`](https://marcelmedina.github.io/space-quiz/admin/) (or `admin/index.html` locally) to watch every submitted score. The page opens with a password prompt; the password's SHA-256 hash is hard-coded in the page, so this is only a casual gate and the underlying API stays public and read-only. Scores are only ever written by the quiz's **Save to leaderboard** action. Once unlocked, the page refreshes automatically every 10 seconds (pausing while the tab is hidden), highlights new submissions, and offers a manual **Refresh now** button.

To run the API locally, start [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) and then:

```powershell
Set-Location api
func start
```

## Development

All markup, styles, question data, and JavaScript are in `index.html`; the leaderboard API lives in `api/`. Edit and reload the browser to see changes. There is no build step, configured linter, or automated test runner.

First check that an empty or whitespace-only name cannot start the quiz, then enter a name and start using Enter or the Start quiz button. The timer should start only after submission, and focus should move to the first question.

For a quick single-question smoke check, select Saturn on question 1: the score should become 1 point, progress should reach 10%, and green feedback should appear. Reload, enter a name, start, and select Jupiter to check red feedback, an unchanged score, and the correct answer reveal.

For a full smoke check, finish the quiz with mixed answers and verify the score, accuracy, best streak, results reaction, and replay reset. Check Tab/Shift+Tab navigation and Enter/Space activation, both color schemes, reduced motion, and narrow mobile layouts.

Repository-specific guidance is in [`.github/copilot-instructions.md`](.github/copilot-instructions.md).
