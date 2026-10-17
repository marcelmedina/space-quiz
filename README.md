# Cosmic Quest

A space exploration quiz with 10 questions, colorful answer feedback, and a compact mission-control display. Built as a single, self-contained HTML file with no server, dependencies, or network requests.

## Play locally

Download or clone this repository, then open `index.html` directly in a web browser.

```powershell
git clone https://github.com/marcelmedina/space-quiz.git
Set-Location space-quiz
Start-Process .\index.html
```

## Features

- Ten multiple-choice questions with an explanatory fact after every answer.
- Live score, segmented progress bar, and completion percentage.
- Green feedback for correct answers and a red shake for incorrect selections.
- Results with an emoji reaction, accuracy, best streak, and replay.
- Optional 1–4 keyboard shortcuts for selecting answers.
- A centered, responsive layout with automatic light and dark themes.
- Native buttons, visible keyboard focus, accessible status updates, and reduced-motion support.

Each question accepts one answer. Progress advances when an answer is selected; Next moves to the following question. Replay or reloading the page starts a new quiz.

## Development

All markup, styles, question data, and JavaScript are in `index.html`. Edit it and reload the browser to see changes. There is no build step, configured linter, or automated test runner.

For a quick single-question smoke check, select Saturn on question 1: the score should become 1 point, progress should reach 10%, and green feedback should appear. Reload and select Jupiter to check red feedback, an unchanged score, and the correct answer reveal.

For a full smoke check, finish the quiz with mixed answers and verify the score, accuracy, best streak, results reaction, and replay reset. Check Tab/Shift+Tab navigation and Enter/Space activation, both color schemes, reduced motion, and narrow mobile layouts.

Repository-specific guidance is in [`.github/copilot-instructions.md`](.github/copilot-instructions.md).
