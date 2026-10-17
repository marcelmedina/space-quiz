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

The complete quiz front end lives in `index.html`: inline CSS, semantic quiz/results markup, and a classic inline script after the markup. Keep quiz changes self-contained in this file. The separate admin dashboard lives in `admin/index.html` (also self-contained, copied to `_site/admin/` by the Pages workflow). The leaderboard backend is the .NET 10 Azure Functions project in `api/`.

The `questions` array contains 10 entries with `category`, `icon`, `question`, four `answers`, a zero-based `correct` index, and an explanatory `fact`. Questions and choices remain in their defined order.

The script uses `$` as an ID lookup helper. `current`, `score`, `answered`, `streak`, and `bestStreak` hold the quiz state:
- `renderQuestion()` rebuilds answer buttons and clears question-level feedback.
- `selectAnswer()` guards against repeat scoring, disables all choices, reveals correctness and the fact, and enables Next.
- `updateProgress()` synchronizes the visual fill, percentage, and accessible progress values. Progress counts answered questions, not the currently displayed question.
- Next advances the question or switches from `#quiz` to `#results`; replay resets state and calls `renderQuestion(true)`.

Question-count assumptions span the data, copy, progressbar maximum, ten-segment CSS overlay, results denominator, accuracy multiplier, and reaction thresholds. Keep these aligned if changing the quiz length.

## Leaderboard persistence

`api/` is a .NET 10 isolated-worker Azure Functions app deployed to the `space-quiz` Function App by `.github/workflows/main_space-quiz.yml` on pushes to `main`. `ScoresFunction` exposes an anonymous `/api/scores` route: `POST` validates and stores a `{ name, email, score, total, timeMs, mode }` entry (required name 1-80 chars with control characters removed, required valid email up to 254 chars without whitespace/control characters, integer score 0-10, time 1s-24h), and `GET` returns the top 10 by score, then time. Public leaderboard responses and POST responses exclude email. Entries are stored in the `scores` table of the Function App's `AzureWebJobsStorage` account (connection string or identity-based). The function sets permissive CORS headers because the page is static; never embed keys or tokens in the page. The total of 10 is also hard-coded as `ScoresFunction.QuestionCount`.

In `index.html`, `saveScore()` posts the result as a `text/plain` JSON body (a CORS simple request) when the user chooses "Save to leaderboard", disables the button after a successful save, and moves focus to the mission log heading. `resetSaveScore()` re-enables saving for each completed attempt. `loadLeaderboard()` renders the top 10 with `textContent`.

The welcome `<dialog>` requires name and email before starting the timer, cannot be dismissed with Escape, and retains details in memory for replay only. Reloading clears them. The admin feed includes email; its Email column uses native toggle buttons to reveal/hide one value, starts masked after every refresh, and shows "Not provided" for legacy entries. Masking is cosmetic; server-side password validation controls access to the admin feed.

For local API work, run Azurite and `func start` in `api/` (see `local.settings.json`), and point `SCORE_API` at `http://localhost:7071/api/scores` temporarily.

`AdminScoresFunction` exposes a read-only `GET /api/scores/all` (the `admin` route prefix is reserved by the Functions host). The Functions trigger is anonymous, but the handler requires `X-Admin-Password`, hashes its value with SHA-256, and compares against the server-only hard-coded hash with `CryptographicOperations.FixedTimeEquals`. Missing/incorrect passwords return 401 before querying storage. `OPTIONS` is unauthenticated and allows the custom password header for CORS. Never log the password or put it in a URL. Authorized requests return `{ total, generatedAt, entries }`, ranked by highest score, then shortest quiz time, then earliest submission before applying `?limit=` (default 200, max 1000). The quiz's `POST /api/scores` is the only write path. `admin/index.html` opens a modal `<dialog>` password prompt (Escape cannot dismiss it), sends the entered password over HTTPS, and reveals the dashboard only after the API accepts it. It retains the password only in memory for polling every 10 seconds while the tab is visible. Authorization failure clears credentials and displayed results, stops polling, and reopens the prompt. It highlights new rows and renders entries with `textContent`.

## UI and accessibility conventions

Preserve the centered, maximum-520px column, 15px system sans-serif body, and compact mission-control telemetry panel. Monospace, tabular numeric readouts are confined to mission data; the rest of the interface remains sans-serif.

Colors are CSS custom properties in `:root`, with dark defaults and light overrides inside `prefers-color-scheme: light`. Use the existing semantic tokens for new surfaces and feedback so both themes stay coherent. The score label uses `--text` rather than `--muted` to maintain contrast on `--accent-bg`.

Correct answers use `.correct` with `correct-pop`; wrong selections use `.wrong` with `shake`. Feedback also supplies text, symbols, and accessible answer labels rather than relying on color alone. Preserve the `prefers-reduced-motion` override.

Answer controls, navigation, and "Save to leaderboard" are native buttons. Focus moves to Next after an answer, to the question heading after advancing or replaying, and to the results heading on completion. Headings use `tabindex="-1"` for programmatic focus, and interactive controls have visible focus outlines.

Keep score and feedback live-region semantics, the named progressbar with `aria-valuenow` and `aria-valuetext`, and decorative icons hidden from assistive technology. Quiz and results visibility uses `hidden` with the global `[hidden]` CSS rule.

Dynamic answer and fact text is assigned through `textContent` and DOM creation. Existing `innerHTML` use is limited to application-controlled counter and navigation templates.
