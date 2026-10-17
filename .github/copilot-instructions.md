# Cosmic Quest

## Running and browser checks

Open `index.html` directly in a browser using a `file://` URL, preferably in the integrated browser. The application must remain usable without a server, dependencies, or network requests. The only network use is the optional leaderboard: on `http(s)` the results screen fetches same-origin `scores.json`; on `file://` it skips the fetch and shows a notice.

Use browser-level smoke checks for changes:
- For a single-question check, reload, select Saturn on question 1, and verify 1 point, green feedback, 10% completion, and an enabled Next button. Reload and select Jupiter to verify 0 points, red shake feedback, and the correct answer revealed.
- Complete all 10 questions with mixed answers. Verify one score update per question, locked answers after selection, results score, accuracy, best consecutive-correct streak, and emoji reaction. Result tiers are 0-4, 5-7, and 8-10 correct.
- Verify replay resets score, streak, progress, and question index. Reloading also starts a fresh quiz; state is only in memory.
- Exercise actual Tab, Shift+Tab, Enter, and Space navigation. Programmatic `focus()` and `click()` checks do not establish keyboard activation coverage.
- Check light and dark color schemes, reduced motion, and widths down to 320px. Keep normal-text contrast at least 4.5:1, including small labels on tinted surfaces.

## Architecture and state

The complete application lives in `index.html`: inline CSS, semantic quiz/results markup, and a classic inline script after the markup. Keep application changes self-contained in this file.

The `questions` array contains 10 entries with `category`, `icon`, `question`, four `answers`, a zero-based `correct` index, and an explanatory `fact`. Questions and choices remain in their defined order.

The script uses `$` as an ID lookup helper. `current`, `score`, `answered`, `streak`, and `bestStreak` hold the quiz state:
- `renderQuestion()` rebuilds answer buttons and clears question-level feedback.
- `selectAnswer()` guards against repeat scoring, disables all choices, reveals correctness and the fact, and enables Next.
- `updateProgress()` synchronizes the visual fill, percentage, and accessible progress values. Progress counts answered questions, not the currently displayed question.
- Next advances the question or switches from `#quiz` to `#results`; replay resets state and calls `renderQuestion(true)`.

Question-count assumptions span the data, copy, progressbar maximum, ten-segment CSS overlay, results denominator, accuracy multiplier, and reaction thresholds. Keep these aligned if changing the quiz length.

## Leaderboard persistence

`scores.json` in the repository root is the leaderboard database; the Pages workflow deploys it alongside `index.html`. The results screen's "Save to leaderboard" link (built by `scoreIssueUrl()`) opens a prefilled GitHub issue titled `[score] …` with a fenced JSON block of `name`, `score`, `total`, `timeMs`, and `mode`. Never embed tokens in the page.

`.github/workflows/record-score.yml` runs on opened `[score]` issues, validates the JSON (name 1-80 chars, integer score 0-10, time 1s-24h, no duplicate issue number, one entry per GitHub account), then appends, sorts by score then time, and pushes to the default branch, retrying against the latest commit so parallel submissions are never lost. It then comments on and closes the issue. Its completion triggers `pages.yml` via `workflow_run` because `GITHUB_TOKEN` pushes do not trigger push workflows. `loadLeaderboard()` renders the top 10 with `textContent`. The total of 10 is also hard-coded in the workflow.

## UI and accessibility conventions

Preserve the centered, maximum-520px column, 15px system sans-serif body, and compact mission-control telemetry panel. Monospace, tabular numeric readouts are confined to mission data; the rest of the interface remains sans-serif.

Colors are CSS custom properties in `:root`, with dark defaults and light overrides inside `prefers-color-scheme: light`. Use the existing semantic tokens for new surfaces and feedback so both themes stay coherent. The score label uses `--text` rather than `--muted` to maintain contrast on `--accent-bg`.

Correct answers use `.correct` with `correct-pop`; wrong selections use `.wrong` with `shake`. Feedback also supplies text, symbols, and accessible answer labels rather than relying on color alone. Preserve the `prefers-reduced-motion` override.

Answer controls and navigation are native buttons; the external "Save to leaderboard" control is a link styled with `.next`. Focus moves to Next after an answer, to the question heading after advancing or replaying, and to the results heading on completion. Headings use `tabindex="-1"` for programmatic focus, and interactive controls have visible focus outlines.

Keep score and feedback live-region semantics, the named progressbar with `aria-valuenow` and `aria-valuetext`, and decorative icons hidden from assistive technology. Quiz and results visibility uses `hidden` with the global `[hidden]` CSS rule.

Dynamic answer and fact text is assigned through `textContent` and DOM creation. Existing `innerHTML` use is limited to application-controlled counter and navigation templates.
