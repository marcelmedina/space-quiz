# Cosmic Quest

## Running and browser checks

Open `index.html` directly in a browser using a `file://` URL, preferably in the integrated browser. The quiz must remain usable without a server, dependencies, or network requests. The only network use is the optional leaderboard on the results screen, which calls the Azure Functions API (`SCORE_API`); if it is unreachable, the quiz still works and a status message explains the failure.

Use browser-level smoke checks for changes:
- For a single-question check, reload, select Saturn on question 1, and verify 1 point, green feedback, 10% completion, and an enabled Next button. Reload and select Jupiter to verify 0 points, red shake feedback, and the correct answer revealed.
- Complete all 10 questions with mixed answers. Verify one score update per question, locked answers after selection, results score, accuracy, best consecutive-correct streak, and emoji reaction. Result tiers are 0-4, 5-7, and 8-10 correct.
- Verify replay resets score, streak, progress, and question index. Reloading also starts a fresh quiz; state is only in memory.
- Exercise actual Tab, Shift+Tab, Enter, and Space navigation. Programmatic `focus()` and `click()` checks do not establish keyboard activation coverage.
- Check light and dark color schemes, reduced motion, and widths down to 320px. Keep normal-text contrast at least 4.5:1, including small labels on tinted surfaces.

## Architecture and state

The complete front end lives in `index.html`: inline CSS, semantic quiz/results markup, and a classic inline script after the markup. Keep front-end changes self-contained in this file. The leaderboard backend is the .NET 10 Azure Functions project in `api/`.

The `questions` array contains 10 entries with `category`, `icon`, `question`, four `answers`, a zero-based `correct` index, and an explanatory `fact`. Questions and choices remain in their defined order.

The script uses `$` as an ID lookup helper. `current`, `score`, `answered`, `streak`, and `bestStreak` hold the quiz state:
- `renderQuestion()` rebuilds answer buttons and clears question-level feedback.
- `selectAnswer()` guards against repeat scoring, disables all choices, reveals correctness and the fact, and enables Next.
- `updateProgress()` synchronizes the visual fill, percentage, and accessible progress values. Progress counts answered questions, not the currently displayed question.
- Next advances the question or switches from `#quiz` to `#results`; replay resets state and calls `renderQuestion(true)`.

Question-count assumptions span the data, copy, progressbar maximum, ten-segment CSS overlay, results denominator, accuracy multiplier, and reaction thresholds. Keep these aligned if changing the quiz length.

## Leaderboard persistence

`api/` is a .NET 10 isolated-worker Azure Functions app deployed to the `space-quiz` Function App by `.github/workflows/main_space-quiz.yml` on pushes to `main`. `ScoresFunction` exposes an anonymous `/api/scores` route: `POST` validates and stores a `{ name, score, total, timeMs, mode }` entry (name 1-80 chars with control characters removed, integer score 0-10, time 1s-24h), and `GET` returns the top 10 by score, then time. Entries are stored in the `scores` table of the Function App's `AzureWebJobsStorage` account (connection string or identity-based). The function sets permissive CORS headers because the page is static; never embed keys or tokens in the page. The total of 10 is also hard-coded as `ScoresFunction.QuestionCount`.

In `index.html`, `saveScore()` posts the result as a `text/plain` JSON body (a CORS simple request) when the user chooses "Save to leaderboard", disables the button after a successful save, and moves focus to the mission log heading. `resetSaveScore()` re-enables saving for each completed attempt. `loadLeaderboard()` renders the top 10 with `textContent`.

For local API work, run Azurite and `func start` in `api/` (see `local.settings.json`), and point `SCORE_API` at `http://localhost:7071/api/scores` temporarily.

## UI and accessibility conventions

Preserve the centered, maximum-520px column, 15px system sans-serif body, and compact mission-control telemetry panel. Monospace, tabular numeric readouts are confined to mission data; the rest of the interface remains sans-serif.

Colors are CSS custom properties in `:root`, with dark defaults and light overrides inside `prefers-color-scheme: light`. Use the existing semantic tokens for new surfaces and feedback so both themes stay coherent. The score label uses `--text` rather than `--muted` to maintain contrast on `--accent-bg`.

Correct answers use `.correct` with `correct-pop`; wrong selections use `.wrong` with `shake`. Feedback also supplies text, symbols, and accessible answer labels rather than relying on color alone. Preserve the `prefers-reduced-motion` override.

Answer controls, navigation, and "Save to leaderboard" are native buttons. Focus moves to Next after an answer, to the question heading after advancing or replaying, and to the results heading on completion. Headings use `tabindex="-1"` for programmatic focus, and interactive controls have visible focus outlines.

Keep score and feedback live-region semantics, the named progressbar with `aria-valuenow` and `aria-valuetext`, and decorative icons hidden from assistive technology. Quiz and results visibility uses `hidden` with the global `[hidden]` CSS rule.

Dynamic answer and fact text is assigned through `textContent` and DOM creation. Existing `innerHTML` use is limited to application-controlled counter and navigation templates.
