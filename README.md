# Cosmic Quest

A space exploration quiz with 10 questions, colorful answer feedback, and a compact mission-control display. Built as a single, self-contained HTML file with no server or dependencies; the only network request is an optional leaderboard fetch on the hosted site.

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
- An optional public leaderboard that saves your name, score, and mission time to `scores.json` in this repository.
- Optional 1–4 keyboard shortcuts for selecting answers.
- A centered, responsive layout with automatic light and dark themes.
- Native buttons, visible keyboard focus, accessible status updates, and reduced-motion support.

Enter your name and select Start quiz to begin. Your name is displayed throughout the quiz and results, stays only in memory, and is retained for replay. Reloading returns to the welcome screen and clears the name and quiz state.

Each question accepts one answer. Progress advances when an answer is selected; Next moves to the following question. Replay starts a new quiz with the same name.

## Leaderboard

The results screen offers **Save to leaderboard**, which opens a prefilled GitHub issue titled `[score] …` containing your name, score, mission time, and question order as JSON. Nothing is saved unless you submit that issue with your GitHub account.

When a `[score]` issue is opened, [`.github/workflows/record-score.yml`](.github/workflows/record-score.yml) validates the data, appends it to [`scores.json`](scores.json) on `main`, comments, and closes the issue. The Pages workflow then redeploys so the online results screen shows the top 10 explorers by score, then fastest time. No tokens or secrets are embedded in the page. When opened from a local file, the quiz works fully and the leaderboard is simply not loaded.

## Development

All markup, styles, question data, and JavaScript are in `index.html`; leaderboard data lives in `scores.json`. Edit and reload the browser to see changes. There is no build step, configured linter, or automated test runner.

First check that an empty or whitespace-only name cannot start the quiz, then enter a name and start using Enter or the Start quiz button. The timer should start only after submission, and focus should move to the first question.

For a quick single-question smoke check, select Saturn on question 1: the score should become 1 point, progress should reach 10%, and green feedback should appear. Reload, enter a name, start, and select Jupiter to check red feedback, an unchanged score, and the correct answer reveal.

For a full smoke check, finish the quiz with mixed answers and verify the score, accuracy, best streak, results reaction, and replay reset. Check Tab/Shift+Tab navigation and Enter/Space activation, both color schemes, reduced motion, and narrow mobile layouts.

Repository-specific guidance is in [`.github/copilot-instructions.md`](.github/copilot-instructions.md).
