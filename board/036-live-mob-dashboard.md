# Live mob dashboard: see every seat's status from a phone

- **Status:** Ready, 2nd in Ready (rank: see `board/MILESTONE-usable-v1.md`); do not interrupt the running mobbing trial on the baseline-fix card
- **Type:** tooling, run as process/tooling experiment E2 (`docs/experiments/E2-live-dashboard.md`)
- **Lives:** the card is on Phrase's board because mob-defaults has no board; **the code lives in mob-defaults** (`facilitator.py` plus `templates/`), not in phrase-lang
- **North star:** `VISION.md` (the mob's own effectiveness; LLMs as customers include the mob's operators)
- **Source:** Steve, 2026-10-25: "we don't have a lot of insight into what's happening after we hand off a task to another model. it just runs in a prompt and we wait to hear back." He chose a live dashboard, on his phone, and said it belongs in the default mob. Decider D7.
- **Story:** As Steve, I want one live dashboard on my phone that shows what every mob and every seat is doing, so that a stalled handoff is noticed in minutes and I do not have to ask for progress.
- **Hypothesis:** We believe a live dashboard fed by a per-seat event stream with stall detection will cut the time to notice a stalled or silent handoff, and cut Steve's 'progress?' check-ins, for the mob. We'll know when the median time from a seat going silent to a stall flag moves from about 40 minutes (the empty Grok review) to at most 5 minutes over the next 5 cards, and Steve's progress check-ins per card fall from the current count to at most half of it. Counter-metrics (Goodhart guard): tooling overhead (tokens and seconds per cycle, at most +3%), data exposure incidents (zero; the allow-list test never fails), and false stall stops (zero stops of a healthy seat).
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a mob with no events yet, When the snapshot is built, Then `status.json` shows the project as idle with no card, and the dashboard renders it; a paused mob is shown as paused
  - One: Given one seat turn, When it runs, Then `start` and `end` events are appended to `.mob/<card>/events.jsonl` with seat, role, play, pid and a token estimate, and no prompt or reply text; the snapshot shows that seat running with elapsed time, then idle
  - Many: Given a mobbed cycle with four seats, When it runs, Then `status.json` shows the card title and stage (red, green, refactor, checkpoint), driver, navigator and observers, per-seat status with last event and elapsed time, tokens per seat against its allotment, the last 20 events and flags, milestone progress from `board/MILESTONES.md`, the last commit and the gate status; and two projects publish into ONE dashboard page, each in its own section
  - Boundaries: Given a seat silent for N minutes (default 10: no output change and no CPU change while its process is alive), When detected, Then the seat is marked stalled, a `stall` event is logged, and only that process and its children are stopped by their own PIDs, never by name; a healthy long-running seat is never stopped
  - Interface: Given the page on a phone viewport (375 px wide), When opened, Then it is readable without zoom, shows the age of its data, refreshes itself about every 30 seconds, and marks a snapshot older than 3 minutes as stale
  - Exceptions: Given a field not on the allow-list (a prompt, code, a path, a secret-looking string, an error body), When the serializer runs, Then it is dropped, a test fails if any such field can appear, and a publishing failure is logged but never fails a card
  - Freshness: Given an event, When it happens, Then it is visible on the local dashboard within 90 seconds at the 95th percentile, measured over at least 30 events, and the figure is given to Steve with the hosting options
  - Gate: Given any hosting that is reachable from outside the laptop, When it is about to be used, Then Steve has given his explicit one-word OK on that one option, presented with the allow-list test results and the options' exposure (a privacy exposure under the escalation safety carve-out); until then the dashboard runs locally only

## Design (architect, ratified by Decider D7)

- **Build order:** (1) per-seat event stream plus stall detection in `facilitator.py`; (2) `status.json` snapshot per project derived from the stream; (3) one static dashboard page reading the snapshots.
- **One dashboard, many mobs:** each mob writes its own file (`status/<project>.json`) so mobs never conflict; the page lists every project, paused ones as paused. Any mob started from mob-defaults publishes automatically; the only per-project setting is a project name.
- **Hosting: NOT chosen by the mob (Decider D7, amended).** The first proposal (a public status-only repo with GitHub Pages) failed on Copilot's ABSOLUTELY NO: nothing should be public before the design is verified privacy-safe and Steve approves a secured hosting model. The amended proposal passed: the card builds only what exposes nothing (event stream, stall detection, the allow-list snapshot and its test, and the dashboard served LOCALLY), and no hosting option is chosen or used until Steve gives an explicit one-word OK on one specific option, shown with the allow-list test results and each option's exposure. Options to present to Steve: (a) a separate PUBLIC status-only repo with GitHub Pages (the org is on GitHub's Free plan, so Pages is not available for private repos, and a Pages site is publicly reachable); (b) the fallback host (unlisted, not access-controlled); (c) any option with real access control, such as a tunnel behind a login, if one is cheap. Freshness (90 s at the 95th percentile) is measured locally first. No URL exists yet.
- **What is published (allow-list only):** project name and state, card title, stage, seat names and roles, seat status, last-event kind and elapsed time, token counts against allotment, flag one-liners that contain no code or paths, milestone names and progress, the last commit subject line and the gate pass/fail. Never prompts, replies, diffs, file paths, error bodies or keys.
- **Stall rule:** silence means no output change and no CPU change for N minutes (default 10) on a live process; the stop is by that process's own PID (and its children), never by name or pattern. This is the same rule as the agreement's safety section.
- **Cost:** event writes are one line each; snapshots are rebuilt at most every 15 seconds; overhead is measured and capped (counter-metric).
- **Only if the retro keeps E2:** 'every mob publishes to the dashboard' is added to the default TWA template and the README.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands at pull time, including negative checks (the allow-list test fails on a seeded prompt string; the stall test fails if a healthy seat is stopped; no check may be satisfiable by fixtures).
