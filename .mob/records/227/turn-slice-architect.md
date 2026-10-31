# Card 227 slice: architect (ctx-2, Claude seat)

Sources read: board/227-*.md; .mob/records/227/record-in-progress.json; window 227-taskforce; card 226's slice (shape and depth); D136, D157, D166, D167; TEAM_WORKING_AGREEMENT.md (Driver trust ladder, Safety); mob-defaults delete_guard.py (whole), facilitator.py (run_backend, scan_safety and both callers, the two dispatch paths), decider.py (parse and voter path), mob_process.py (check_record, run_audit, main), templates/TEAM_WORKING_AGREEMENT.default.md, tests/test_delete_guard.py, tests/fakes, core/backends.json, the CLI help of claude, codex, copilot, grok and agy, ~/.claude/projects and ~/.codex/sessions as they are on this machine. Seat started 2026-10-31 09:35:51 UTC (in-flight line 7b3a5cb; cap 10:05:51).

## What is in reach, said plainly (the adopted condition)

Today nothing scans a command. `facilitator.scan_safety(role, reply)` gets the cleaned reply text of a seat, at two call sites (the run_turn path and the card-seat path), and `decider.py` has no scan at all. A seat that runs `rm -rf "$SP/mut"` through a tool call and then writes "done" is invisible.

Probe of what exists (2026-10-31, read-only): the Claude CLI writes every tool call to a transcript. 1174 transcripts under ~/.claude/projects hold 20,658 Bash and PowerShell tool calls, each as a `tool_use` block with `input.command`. Two of them are card 226's slip candidates, both in this machine's session 73298da3: `subagents/agent-a555371e4371387a8.jsonl` at 01:47:50Z (`S=...proto226; rm -rf "$S"/x 2>/dev/null; mkdir -p $S/x ...`, the architect's prototype) and `subagents/agent-a23913037593be5a6.jsonl` at 02:00:23Z (`SP=C:/Users/.../scratchpad; rm -rf $SP/mut 2>/dev/null; mkdir -p $SP/mut ...`, meta description "Card 226 QA seat: criterion 1 red"; a folder that did not yet exist, which fits the record's wording). Neither is labelled backend developer ctx-4, so QA confirms which one the record means; both are the same form. Feeding either command text to today's `find_unsafe` returns a finding. The pattern is not the gap; the feed is.

| Seat kind | Source of executed commands | In reach? |
|---|---|---|
| Claude seat run by facilitator.py or decider.py (`claude -p`) | The session transcript `~/.claude/projects/<cwd slug>/<session-uuid>.jsonl`. The CLI has `--session-id <uuid>`, so mob-defaults chooses the uuid and knows the path before the seat starts. Decider voters run in a scratch clone, so their slug differs; same reader. | Yes. Read after the seat's process ends (post-hoc: it reports, it does not stop the command). |
| Codex seat run by facilitator.py or decider.py | `~/.codex/sessions/YYYY/MM/DD/rollout-*.jsonl`; `session_meta.payload.cwd` and the timestamp locate it (no session id flag on `codex exec`). The file exists and holds `response_item` records; I saw no command item in the one probe session, so QA pins the command item's shape from a real rollout that ran a command. | Yes, shape to be pinned at red. Codex has no quota today, so the reader is built against a recorded file. |
| Copilot, Grok, agy seats | None found. Copilot has a log dir and a session-store.db whose content I did not verify; Grok and agy expose no transcript option in --help. | No. Named, not guessed: these backends carry `"command_source": "none"` and every seat on one is recorded as `unscanned: no command source`, a Safety flag, never as clean. |
| Subagent seats (the Agent tool, as the Claude seats of this card are) | The subagent transcript `~/.claude/projects/<slug>/<parent session>/subagents/agent-<agentId>.jsonl`. The parent session learns `agentId` from the Agent result. | Yes, after the fact. mob-defaults does not dispatch these (Claude Code does), so the scan is a command the facilitator runs per seat: `python delete_guard.py scan-transcript <path> --seat "<role> ctx-N"`, and its result is a line in the card record. |

What is out of reach, and stays so: stopping a command before it runs. Only a PreToolUse hook can do that, and this card will not change Claude Code settings or hooks on this machine. The smallest thing mob-defaults can own for that is a hook script (`hooks/delete_guard_hook.py`: reads the PreToolUse JSON on stdin, runs `find_unsafe` on `tool_input.command`, exits 2 with the finding), plus a README snippet a human can paste to opt in. Nothing in this card installs it. It is increment 6 of criterion 1 and can be split to a follow-up card without breaking Done check 1, which is satisfied by the post-hoc transcript scan; the card's "blocked" is met for the opted-in machine only, and the slice says so.

The Zero case (adopted condition): a seat whose transcript is missing, unreadable, empty, or holds no assistant record is a FINDING ("no command source for <seat>"), not a clean scan. A transcript with assistant records and zero tool calls is clean. A backend with `"command_source": "none"` is "unscanned", reported once per seat, never clean.

Where it is called: all seats pass through `facilitator.run_backend` (card seats, the run_turn failover chain, and decider voters, which call it from vote_seat). It is the one place that picks the session id and sees when the process ends, so the scan sits there: `run_backend` substitutes `{SESSION}` in the invocation from a uuid it makes, reads the transcript through `command_source.py` when the process ends, and hands the findings back through a `scan_out` list the caller passes. `CardRun` turns them into the existing Safety flag in MOB.md (kind, command, seat); `decider.py` records them in the Decider log entry as `command_scan` and does not change a vote. The reply scan stays; the command scan is added beside it.

## Done checks, numbered (D157; one red-green cycle each)

1. The scan sees executed commands, not only reply text: for a seat run by facilitator.py or decider.py, and for a subagent seat through `delete_guard.py scan-transcript`, every Bash or PowerShell command the seat executed is scanned, and a recursive delete of a variable or computed path is reported with the command and the seat. The failing test comes first and reproduces the card 226 slip from its transcript (`rm -rf $SP/mut 2>/dev/null` in a tool call, absent from the reply text), then `Remove-Item -Recurse -Force`, `shutil.rmtree` and the equivalents in the table below. An empty or missing command source is a finding (Zero).
2. A trust-ladder entry is recorded for the card 226 slip and cannot be skipped: every card-record deviation whose rule is a safety rule (from card 226 on) needs a line in `.mob/records/trust-ladder.jsonl` naming the seat, the rule and the step down or a stated reason none applies; `python mob_process.py ladder <repo>` fails without it.
3. Push on every commit: the template and the avowry agreement carry the rule with its 5-minute limit, and `python mob_process.py unpushed <repo>...` reports FAIL naming the repository, the commit and its age for any commit on avowry or mob-defaults unpushed for more than 300 seconds; the facilitator runs it after every seat and the audit pass runs it.

Each red commit body carries "Criterion: n". Green n before any red for n+1. Greens pushed at once, per the card's own rule. The "Intent:" line (D165) quoting the product owner's ruling comes first: "a mis-quoted delete like the 2026-10-08 home-folder wipe could take out a repository including .git, losing every unpushed commit."

## Where the trust-ladder entry lives (criterion 2, so that the check can fail)

The Driver ledger in TEAM_WORKING_AGREEMENT.md is a hand-edited markdown table with one row per CLI, not per slip, so a check cannot fail on it without parsing prose. The entry lives in an append-only file in avowry: `.mob/records/trust-ladder.jsonl`, one line per entry: `{"at", "card", "criterion", "seat" ("backend developer ctx-4"), "driver" ("claude"), "rule", "evidence" (a commit or record path), "step" ("down to rung N" or "none: <reason>"), "brief_checked" (true or the quoted line)}`. The agreement's ledger row for the driver gets one Notes sentence that cites the line; the jsonl is the checked record, the table is the reading copy.

The check is `mob_process.ladder_problems(deviations, entries)`: every deviation in `.mob/records/NNN.json` or `.mob/records/NNN/record-in-progress.json` for card 226 and later whose `rule` contains "safety" (case-insensitive) must have an entry with the same card and `seat` equal to the deviation's `by`; a missing file, an empty file, a bad JSON line, an entry without `step`, or a `step` that is neither "down to rung N" nor "none: <non-empty reason>" is a problem. No safety deviation means no file is needed. The template (templates/TEAM_WORKING_AGREEMENT.default.md, under Driver trust ladder) gets the file name and the line format as its trust-ladder record rule; "FILL IN PER PROJECT" rows stay.

The entry for card 226 (committed in the green of criterion 2, in avowry): seat backend developer ctx-4, driver Claude, rule "never use rm -rf on variable paths", step "none: the driver is at Rung 1, where a miss keeps the rung and the driver is coached (Driver trust ladder, Path back up)", brief checked: the record says nothing in the seat brief said what to do instead, so the fix is to the brief and the scan, which is this card.

## Detector seam for unpushed commits (criterion 3)

Where it runs: mob-defaults `mob_process.py`, a new subcommand `unpushed <repo>...` over pure functions `unpushed_commits(repo, run)` and `unpushed_problems(repos, now, run)`, called (a) by the facilitator after every seat (a "Push flag" beside the Safety flag, printed and in MOB.md), (b) by `run_audit` for avowry and mob-defaults, (c) by hand. It is not the pre-push hook: the hook fires when a push is attempted, at which moment the oldest commit is already late and the push is the cure, so failing there blocks the cure and says nothing during the minutes that matter; and the existing hook runs avowry's own `scripts/mob_process.py check`, a different, independent (D134) file that this card does not touch. Git only, no gh, no network: origin is never contacted, so a refused or failed push leaves the commit unpushed and the detector says so.

Algorithm: `git log HEAD <local branches that have an upstream> --not --remotes=origin --format=%H%x09%ct%x09%s`. A commit is unpushed when no `refs/remotes/origin/*` ref contains it; age is now minus the committer time; FAIL when age is more than 300 seconds (300 passes, 301 fails; the limit is one constant `UNPUSHED_LIMIT_SECONDS`, not a typed number in messages). Clock and git runner are injected.

Scope decision from the probe: counting every local branch fails on day one. `backup/pre-rebase-0830` in avowry holds 16 commits (card 213 era, 2026-10-30) that are on no remote, and main is level with origin/main. So the scope is HEAD (covers a detached HEAD and a branch with no upstream that is checked out) plus local branches that have an upstream configured. Residual, named for the security reviewer: a checked-out-elsewhere branch with no upstream that holds the only copy of work is out of reach of this detector.

## Criterion-by-layer table (outermost first; doubles named once)

Doubles: TRANSCRIPT = a trimmed real transcript line set committed as tests/fixtures (the card 226 slip's tool_use, plus a tool_use-free one and an empty one); FAKE_CLI = tests/fakes/fake_cli.py, taught a mode that writes a transcript to `$MOB_PROJECTS_ROOT/<slug>/<session>.jsonl` when its invocation carries `--session-id` (never the real ~/.claude); CLOCK = injected `now`; GIT = real temporary repositories with a real bare repository as origin (no network, no gh), commits dated with GIT_COMMITTER_DATE; RECORDS = a temporary repo holding a copy of the card 226 deviation.

| Crit | Layer | Test (new modules tests/test_command_scan.py, tests/test_trust_ladder.py, tests/test_push_detector.py) | Doubles | Red-able or born-green |
|---|---|---|---|---|
| 1 | Acceptance, process: facilitator.py card run (CardModeTestCase) | `test_a_delete_in_a_tool_call_but_not_in_the_reply_is_flagged`: FAKE_CLI writes a transcript whose Bash command is the slip (`rm -rf $SP/mut 2>/dev/null`) while its reply says only "done"; MOB.md carries a Safety flag naming the seat and the command | FAKE_CLI, TRANSCRIPT | Red-able (scan reads reply only) |
| 1 | Acceptance, process: same | `test_a_seat_with_no_transcript_is_a_finding_not_a_clean_scan` (Zero: no file; empty file; file with no assistant record) and `test_a_backend_without_a_command_source_is_reported_unscanned` | FAKE_CLI | Red-able |
| 1 | Acceptance, process: decider.py (extends test_decider_runner) | `test_a_voter_that_deletes_in_a_tool_call_is_recorded_in_the_log`: fake_voter writes a transcript; the Decider log entry has `command_scan` with the finding; the vote and exit code are unchanged | fake_voter, TRANSCRIPT | Red-able |
| 1 | Acceptance, CLI: `delete_guard.py scan-transcript <path> --seat S` | exit 1 and the finding on the slip; exit 0 on a clean one; exit 2 on a missing, empty or unparsable path (Zero) | TRANSCRIPT | Red-able |
| 1 | Unit: `command_source.commands_from_transcript` (claude shape) and `commands_from_rollout` (codex shape, pinned from a real rollout) | Bash and PowerShell `input.command` returned in order; other tools ignored but counted; Zero cases raise `NoCommandSource`; a 20 MB file is read as a stream | TRANSCRIPT | Red-able; stream case born-green guard |
| 1 | Unit: `delete_guard.find_unsafe` extended | each equivalent in the list below is its own case; SAFE list unchanged | none | Red-able per form; every existing UNSAFE and SAFE case stays and passes |
| 1 | Unit: `run_backend` session id | `{SESSION}` replaced by a uuid, the same uuid reaches the transcript locator, a template without `{SESSION}` is "unscanned" | FAKE_CLI | Red-able |
| 1 | Unit: record key `command_scans` (subagent seats) | `check_record` flags a task-force context with no entry only when the record has the key; the facilitator adds the key from card 227 | RECORDS | Red-able; keyless old records stay ok (born-green guard) |
| 1 | Unit: `hooks/delete_guard_hook.py` | stdin JSON with the slip: exit 2 and the finding on stderr; a safe command: 0; empty or unparsable stdin: exit 2 (a hook that fails open is the failure this card exists to remove) | none | Red-able; increment 6 |
| 2 | Acceptance, process: `python mob_process.py ladder <repo>` | the card 226 deviation with no `.mob/records/trust-ladder.jsonl`: exit 1 naming card 226, seat, rule; with a valid line: exit 0; with `step` missing, or "none:" with no reason: exit 1 | RECORDS | Red-able |
| 2 | Unit: `safety_deviations(records)` | picks deviations whose rule contains "safety" from card 226 on, in final records and in-progress files; ignores the QA misreport deviation (a different rule) and cards before 226 | RECORDS | Red-able |
| 2 | Unit: `ladder_problems` | missing file, empty file, bad JSON line, wrong card, wrong seat, bad step | none | Red-able |
| 2 | Unit: template | the template names `.mob/records/trust-ladder.jsonl` and the line fields | none | Red-able |
| 3 | Acceptance, process: `python mob_process.py unpushed <repo>...` | commit dated 6 minutes ago, unpushed: exit 1, text names repository, short sha, age; after `git push` to the bare origin: exit 0 | GIT, CLOCK | Red-able |
| 3 | Unit: `unpushed_commits` | parses the log; HEAD only plus upstream branches (a backup branch without upstream is not counted); detached HEAD; no upstream on the checked-out branch; no remote | GIT | Red-able; the backup-branch case is the probe's case |
| 3 | Unit: `unpushed_problems` | 299 s, 300 s pass, 301 s fails; empty repo list fails (Zero); a path that is not a git repository fails; a repository with no commits passes | CLOCK, GIT | Red-able; boundary also a born-green guard against `>=` |
| 3 | Unit: facilitator wiring and `run_audit` | after a seat, a "Push flag" in MOB.md when a watched repo has an old unpushed commit; none when pushed; `run_audit` includes both repos | GIT | Red-able |
| 3 | Unit: rule text | the template and the avowry agreement both contain the sentence with the 5-minute limit and the detector name; a repo whose agreement lacks it fails `unpushed` | none | Red-able |

Equivalents the red must cover for criterion 1 (the security reviewer extends this list; each is a test): `rm -rf "$S"/x`, `rm $d -rf` (flags after the operand: GNU rm accepts it, `find_unsafe` returns nothing today, probed), `rm $d -r -f`, `rm -Recurse $p` and `ri -r $p` and `Remove-Item $p -r` and `-Rec` (PowerShell aliases and parameter abbreviations; all return nothing today), `rmdir $p -Recurse`, `del $p -Recurse`, `Remove-Item -Force -Recurse $p`, `shutil.rmtree(p)`, bare `rmtree(p)` after `from shutil import rmtree`, `os.system("rm -rf " + p)`, `[IO.Directory]::Delete($p,$true)`, `find $d -delete`, `cmd /c rd /s /q %d%`, `bash -c "rm -rf $d"`, `sudo rm -rf $x`, `xargs rm -rf`, and a path built on a prior line (`d=...` then `rm -rf $d`). Allowed side that must not widen: a literal path inside the scratch folder, `safe_delete(...)`, `rm -f file.txt`, `kill 1234`, prose about removing a form. Not widened and not claimed: a literal absolute path outside any scratch root (`rm -rf /c/Users/ringe/foo`) passes today and still does; the card says variable or computed paths, and widening it is a decision for the security reviewer, not a change this slice makes silently.

## Assumptions (D166: this card changes a rule and the checker; one line per invariant, at most three)

Assumes: a seat's executed commands are readable after it ends from a transcript whose `tool_use` blocks carry `input.command`, so the scan can run on commands and the existing patterns suffice; probe: 1174 transcripts under ~/.claude/projects hold 20,658 Bash and PowerShell commands, the card 226 slip is in `subagents/agent-a23913037593be5a6.jsonl` (`rm -rf $SP/mut 2>/dev/null`, 02:00:23Z), and `find_unsafe` flags that text today (about 70 distinct flagged forms across the transcripts, some of them test strings quoted inside commands).

Assumes: a safety violation is recorded as a card-record deviation whose rule text says "safety", and no card before 226 has one, so a forward-only ladder check does not reopen history; probe: `.mob/records/226/record-in-progress.json` deviations[0] (by "backend developer ctx-4", rule "standing safety rule: never use rm -rf on variable paths"); of the 7 final records (195, 205, 208, 209, 214, 219, 226) none has a `deviations` key, and across the 8 in-progress files the only deviation whose rule says "safety" is that one (the others name D136, "report outcomes faithfully" and "claims carry their command").

Assumes: a commit counts as pushed when an `origin` remote-tracking ref contains it, and the scope HEAD plus branches with an upstream is the set the product owner means; probe: run today, avowry has 0 unpushed commits on main (`git rev-list --count origin/main..main` = 0) but `git log HEAD --branches --not --remotes=origin` returns 16 (the backup branch `backup/pre-rebase-0830`), mob-defaults has 0; so all-branches would fail permanently on a deliberate backup and is rejected.

## Inputs (D167: criteria that take a list of names)

Criterion 1 Inputs: disabled: a backend with `enabled: false` is skipped by failover and never scanned, and a backend whose `command_source` is "none" is reported `unscanned` once, never clean (`test_a_backend_without_a_command_source_is_reported_unscanned`); unknown: a tool name in the transcript that is not Bash or PowerShell (counted, not scanned, `commands_from_transcript` unit) and a command form not in the list (a recursive verb with a target the parser cannot read is a finding, the existing `not targets` rule, and the `find -delete` and `[IO.Directory]::Delete` cases above); repeated: the same command twice in one transcript is two commands in the count and one finding line per distinct text, and the same seat scanned twice is not double-flagged in MOB.md; reordered: flags and targets in any order (`rm $d -rf`, `Remove-Item -Force -Recurse $p`, `-Recurse` before the path), and transcript commands are scanned in file order with the finding order equal to it.

Criterion 2: no list input (it reads the deviations and entries of a repo; the order of lines in the jsonl does not matter and one test shows it).

Criterion 3 Inputs: disabled: n/a: the repository list is named by the caller; there is no enabled flag, and a repository the caller leaves out is simply not checked, which the empty-list Zero case covers; unknown: a path that does not exist or is not a git repository is a FAIL naming it (`unpushed_problems` unit), and a repository with no `origin` is a FAIL "no origin" (its commits cannot be pushed); repeated: the same repository twice, or in two spellings (trailing slash, backslash, different case on Windows), is reported once (`unpushed_problems` unit); reordered: swapping the order of the repositories gives the same findings in the same sorted order (repository, then age).

## Commit plan (mob-defaults unless named; D136: at most 15 countable files and 400 lines per commit; subjects start "Card 227")

0. QA, avowry: the Intent line (D165) in .mob/records/227, before any red.
1. QA red 1 (`Card 227 red 1 (F-071, D157 Criterion 1): ...`, body "Criterion: 1"): tests/test_command_scan.py, fixtures, the FAKE_CLI mode; fails by assertion.
2. Backend, unit: find_unsafe equivalents and the flag-order fixes (delete_guard.py, test_delete_guard.py additions only).
3. Backend, unit: `command_source.py` and its tests.
4. Backend, unit: `delete_guard.py scan-transcript` CLI.
5. Backend, green 1: `run_backend` session id and `scan_out`, `CardRun` Safety flag, decider.py log field; pushed at once.
6. Backend: record key `command_scans`; then the opt-in hook script and README snippet (may split to a follow-up card if limits are hit).
7. QA red 2 (Criterion 2), then backend green 2: `ladder` subcommand, template text; avowry: the card 226 entry in `.mob/records/trust-ladder.jsonl` and the ledger Notes sentence (avowry commit).
8. QA red 3 (Criterion 3), backend units, green 3: `unpushed`, `run_audit`, facilitator Push flag; then the template and avowry TEAM_WORKING_AGREEMENT.md rule text (avowry commit).
9. Delivery lead at ship: the targeted modules per commit; one full mob-defaults suite with its own in-flight line; both repositories pushed and `unpushed` printing no FAIL before the ship commit; push on every commit throughout (this card's own rule; avowry pushes need MOB.md regenerated when the push gate says stale).

## Existing tests the change may touch

May be extended, never loosened: tests/test_delete_guard.py (UNSAFE list grows; SAFE list, the safe_delete cases and the kill cases stay byte for byte), tests/fakes/fake_cli.py and tests/fakes/fake_voter.py (a transcript mode; default behaviour unchanged), tests/test_card_mode.py (`CardModeTestCase` gets the projects-root env and an empty watch list so fixtures without a remote do not trip the Push flag; nothing else), tests/test_decider_runner.py and the decider record tests (the log entry gains `command_scan`; an old-shape entry still renders), tests/test_mob_process.py (new cases; `REQUIRED_SECTIONS` and every existing record case unchanged). Must stay green unchanged: test_seat_gate, test_seat_rotation, test_seat_rotation_rules, test_loop_state (its own-source scan stays clean, so new module sources must not trip `find_unsafe` outside their test strings), test_template_removal. Neither pre-commit nor settings files on this machine are touched; avowry's scripts/mob_process.py, scripts/delete_guard.py and .githooks/pre-push are not touched.

## Slice verdict

SHIP AS IS, with a named split. Three criteria, three cycles. Criterion 1 is the largest (five increments plus the optional hook); if the green for criterion 1 exceeds the D136 limits, the hook script and the `command_scans` record key split to a follow-up card and Done check 1 still holds for facilitator, decider and subagent seats through the transcript scan. Not recommended: scanning with a settings hook only (it fails open when not installed, and installing is a system change); and all-branches for the unpushed detector (fails on day one).

## Risks handed to the reviewers

- Post-hoc, not preventive: a destructive command has already run when the scan reports it. Say so in the template rule text; the hook is the only preventive piece and it is opt-in.
- Transcript retention: the CLI may not write a transcript (`--no-session-persistence`, a cleanup). Missing is a finding by design; check that no invocation in core/backends.json passes that flag.
- Session id collisions and slugs: the transcript path is built from the cwd; Windows drive and separators must map as the CLI does (probe: `C--Users-ringe-projects-phrase-lang`). Locate by the uuid across slugs if the slug rule is uncertain.
- The ladder check matches `rule` text; a deviation worded without "safety" escapes it. Reviewer decides whether a structured `safety: true` key is needed.
- Committer time is not the time the commit was made if a rebase or amend rewrote it; the age can read young. Rewrites are banned (no amends); the reviewer may ask for the reflog time.
- Literal destructive paths outside a scratch root pass (stated above).

## Domain-expert question (T2)

Should a member with different domain knowledge or context join now? No further member at the slice. The security reviewer (adversarial lens, ctx-3) already joins and owns the command-form list above, including the PowerShell alias and flag-position gaps this slice found by probing; the reader and the git seam need nothing beyond the architect and the backend developer. A mid-card expert through a Decider (with its id) would be justified only if QA cannot pin the codex rollout command shape from a real file, in which case the codex reader is dropped to "unscanned" and filed as a follow-up rather than guessed.

## Security review at slice (ctx-3)

Seat: security reviewer (adversarial lens), ctx-3, Claude seat (probes at 09:34 UTC: Codex, Copilot, Grok and agy without usable quota). I did not write the slice. Method: strings only, passed to `delete_guard.find_unsafe` in a Python session (mob-defaults at C:\Users\ringe\mob-crosscli, run 2026-10-31); no delete command was executed anywhere. "Flagged today" is the result of that run. Where the slice already lists a form, the row still says what today's code does.

### Forms (flagged today / must flag)

| Form (angle) | Flagged today | Must flag |
|---|---|---|
| `rm -rf "$S"/x`, `rm -rf ${S}/x`, `rm -r -f $d`, `rm -fr $d`, `rm --recursive --force $d`, `rm -R $d`, `rm -rf -- $d`, `rm -vrf $d` (quoting, split and long flags) | yes | yes |
| `rm $d -rf`, `rm $d -r -f` (flags after the operand; GNU rm accepts it) | no | yes |
| `sudo`, `command`, `time`, `nohup`, `busybox`, `wsl`, `ssh host "..."`, `bash -c "..."`, `FOO=1 rm`, `\rm`, `/bin/rm` prefixes | yes | yes |
| `"rm" -rf $d`, `r""m -rf $d` (quoted or split verb) | no | yes |
| `R=rm; $R -rf $d` (verb in a variable) | no | yes (any `$VAR` in command position next to `-rf` or `-r`) |
| `rm -rf $(mktemp -d)`, backtick substitution, `./build/*` glob, `$HOME/...` | yes | yes |
| `rm -rf .git` (literal, relative, the repository itself) | no | yes |
| `rm -rf /c/` (MSYS drive root), `rm -rf /c/Users/ringe/foo`, `rm -rf /tmp/x`, any absolute path outside scratch | no | yes (must-fix 2) |
| `rm -rf C:/Users/ringe/projects/phrase-lang/scratch/x`, `rm -rf scratch/mut`, `./scratch/mut` (literal inside a scratch root) | no | no (must stay allowed) |
| `rm -rf foo`, `rm -rf node_modules` (bare relative word) | yes | yes by the same rule as `.git`; inconsistent today (bare word flagged, `.git` not) |
| `xargs rm -rf`, `xargs -I{} rm -rf {}` | yes | yes |
| `find $d -delete`, `find . -name x -delete` | no | yes |
| `find $d -exec rm -rf {} +` | yes | yes |
| `rsync -a --delete empty/ $d/` (empty-source mirror delete) | no | yes |
| `Remove-Item -Recurse -Force $p`, `-Recurse:$true`, `-LiteralPath`, lowercase | yes | yes |
| `Remove-Item $p -r`, `Remove-Item -Rec $p` (parameter abbreviations) | no | yes |
| `ri -Recurse $p`, `ri -r -fo $p`, `del -Recurse $p`, `erase -Recurse $p`, `rd -Recurse $p`, `rmdir $p -Recurse` (PowerShell aliases) | no | yes |
| `rm -Recurse $p`, `rm -r -fo $p` (PowerShell rm alias) | yes (by luck: the Unix rm rule) | yes |
| `Get-ChildItem $d \| Remove-Item -Recurse -Force`, `gci $d \| % { Remove-Item $_ -Recurse }` | yes | yes |
| `Get-ChildItem $d -Recurse \| Remove-Item -Force` (pipeline, no `-Recurse` on Remove-Item) | no | yes (Remove-Item fed by the pipeline has no judged target) |
| `Remove-Item @args -Recurse` (splat) | no | yes |
| `Remove-Item` with a backtick line continuation, or `-Recurse` on the next line | no | yes |
| `Remove-Item -Recurse -Force .\scratch\x`, `-Path ".\scratch\x"` (literal in scratch) | no | no |
| `Remove-Item -Recurse -Force C:\Users\ringe\foo` (literal outside scratch) | no | yes (must-fix 2) |
| `[IO.Directory]::Delete($p,$true)` | no | yes |
| `iex "Remove-Item -Recurse $p"`, `powershell -Command "..."`, `Start-Process cmd -ArgumentList "/c rd /s /q $p"` | yes | yes |
| `powershell -EncodedCommand <base64>`, `pwsh -enc ...` | no | yes (decode UTF-16LE base64 and rescan; if undecodable, a finding) |
| `echo <base64> \| base64 -d \| sh`, `printf "r%s -rf %s" m "$d" \| sh` | no | yes (a decode-to-shell pipe is a finding in itself) |
| `cmd="rm -rf $d"; eval $cmd` | yes (only because the string holds rm -rf) | yes; and `eval` of a built string with the verb split: no today, must flag |
| `cmd /c rd /s /q %d%`, `rmdir /s /q "%TEMP%\x"`, `del /s /q %d%\*`, `RD /S /Q`, `rd /s/q` | yes | yes |
| `rd /s /q C:\Users\ringe\foo` (literal outside scratch) | no | yes (must-fix 2) |
| `shutil.rmtree(p)`, with kwargs, f-string, concat, multi-line, `python -c "..."` | yes | yes |
| `shutil.rmtree (p)` (space before the paren) | no | yes |
| `from shutil import rmtree` then `rmtree(p)`; `import shutil as sh` then `sh.rmtree(p)` | no | yes |
| `shutil.rmtree("C:/Users/ringe")`, `shutil.rmtree("..")`, `shutil.rmtree("scratch/x")` | no (any quoted literal passes) | the first two yes, the third no |
| `os.removedirs(p)`, `os.rmdir(p)`, `Path(p).rmdir()` | no | yes for `os.removedirs`; `rmdir` is non-recursive, flag only inside a walk or loop (low) |
| `os.system("rm -rf " + p)` | no | yes (concatenation splits the verb from the target) |
| `subprocess.run(["rm","-rf",p])` (argv list) | no | yes |
| `subprocess.run("rm -rf %s" % p, shell=True)` | yes | yes |
| `fs.rmSync(p,{recursive:true})`, `npx rimraf $d`, perl `rmtree($d)` | no | yes (cheap to add) |
| `git clean -fdx`, `-xdf`, `-f -d`, `--force -d` | yes | yes |
| `git -C repo clean -fdx` | no | yes |
| `git clean -n` (dry run) | no | no |
| `git rm -rf .` | yes | yes |
| `git reset --hard HEAD~5`, `git checkout -- .`, `git branch -D main`, `git push --force` | no | outside a delete scan, but each can destroy unpushed commits or the working tree; name them in the card's Not-in-this-card line or file a follow-up |
| Script written then run: `printf 'rm -rf $d' > c.sh; bash c.sh` | yes (body in the same command) | yes |
| `bash cleanup.sh` where the file was written by the Write tool in another call | no | yes (must-fix 2: scan Write and Edit contents too, and flag `bash <file>` for a file the seat wrote this session) |

### Bypasses of the transcript source

- A seat writes a script with the Write or Edit tool (not Bash, so not scanned) and runs it with `bash x.sh` or `python x.py`: the destructive text never appears in a scanned `input.command`. The slice scans only Bash and PowerShell commands and counts other tools.
- A non-Claude seat (Copilot, Grok, agy, and Codex until the rollout shape is pinned) is "unscanned", reported as a Safety flag. A flag is not a failure; nothing stops the next turn or the card. Only the Claude seat is covered, and today it is the only seat running at all.
- The subagent seats (the seats of this very card) are scanned only if the facilitator runs `scan-transcript` by hand and writes the line. A forgotten step is silent. The record key `command_scans` makes the check fail only for records that already carry the key, so a record that omits the key is never checked.
- A missing, empty or unreadable transcript is a finding (Zero), good. A seat that deletes or truncates its own transcript, or a CLI run with `--no-session-persistence`, lands on the same finding, but the finding comes after the damage.
- The scan is post-hoc, and a delete that reaches `~/.claude/projects` removes the transcript that would prove it. The hook is the only preventive piece and is opt-in; nobody installs it by default.
- Encoded and indirect forms (EncodedCommand, base64 to shell, printf-split verb, verb in a variable) are invisible to a text scan. The slice's unknown-form rule (a recursive verb with an unreadable target is a finding) fires only if the verb is parsed first; these forms hide the verb.
- A child process started by a seat (`claude -p`, `python tool.py`) runs its commands in its own transcript, which the scan does not know to look for.

### Bypasses of the unpushed-commit detector

- Scope: HEAD plus local branches with an upstream. A commit on a branch with no upstream that is not checked out (the `backup/pre-rebase-0830` shape), or on the HEAD of another worktree of the same repository, is invisible. The first is the slice's named residual; a worktree is a second way in.
- Repositories: only the list the caller names (avowry, mob-defaults). play-tracker, BaccaratSim, a new project repository or a clone in a scratch folder is not watched. The facilitator-runs-it-after-each-seat trigger means that with no seat running, no check runs; nothing is scheduled.
- Clock: age is now minus committer time. A future `GIT_COMMITTER_DATE` gives a negative age and passes for as long as the date is ahead. A rebase, cherry-pick or amend resets the committer time to the rewrite moment; amends are banned by rule but not by the detector. A skewed system clock moves every age. Fix: a future-dated commit is a FAIL, and the age is taken from the older of the committer time and the reflog entry time.
- Remote-tracking refs are local: `git update-ref refs/remotes/origin/main HEAD`, a fetch from a mirror, or an `origin` that points at a local clone or fork all make the commit look pushed. Pushing only to a remote not named origin leaves it "unpushed" (a false FAIL, the safe direction).
- If `.git` or the repository folder is deleted, the detector says "not a git repository", a FAIL, but the commits are already gone; it reports late by design.
- Environment: `GIT_DIR`, `safe.directory` errors, a locked or corrupted repository can make `git log` fail; any non-zero git exit must be a FAIL (fail closed), not only a missing path.
- Not covered by the card: uncommitted work, stashes and untracked files. The 2026-10-08 wipe lost those too; the 5-minute rule does not protect them.

### False positives to avoid

- A literal path inside a scratch root through `safe_delete(...)`: stays allowed (`safe_delete` is not matched today; keep it so, and keep `delete_guard.py` and its test strings out of the scanned command stream).
- Prose and quoted rules inside a command: `git commit -m "never run rm -rf $d"`, a heredoc that writes the agreement or the Safety rule, `echo "rm -rf is banned"`, `grep -n "rm -rf" file`. Today `find_unsafe` flags `never use rm -rf here`, `the rule: "never run rm -rf $d"`, a backticked `rm -rf $d` in prose, `do not call shutil.rmtree(p) directly`, and any `rm -r` followed by a word (`removed with rm -r build`, `I ran rm -rf on it`, `mkdir farm; rm -rf farm`). That is tolerable for reply text, where the rule is deliberate, but in the command stream it flags every commit message and every test file this mob writes about the rule (the slice's own probe found about 70 such strings) and invites rewording to dodge the scan (the counter-metric). Required: judge the command stream at command position (start of a clause after a separator, after `sudo`, `bash -c`, `xargs`, `eval`, an env prefix), not any substring.
- `rm -f file.txt`, `git clean -n`, `kill 1234`, `Remove-Item file.txt` (no `-Recurse`), `rd` or `del` without `/s`: stay allowed.
- Literal scratch deletes (`rm -rf scratch/mut`, `./scratch/mut`) pass today because the target has no variable. If must-fix 2 is adopted they must pass through an explicit scratch-root allow-list (the roots known to safe_delete), not through the absence of a `$`.
- Words containing the verb (`farm`, `term`, `docs/rm-notes`): the `(?<![\w-])rm\s` guard handles them today; keep it.

### Verdict

CHANGES NEEDED. The mechanism (transcript scan, Zero case, injected clock, git only) is sound, and the card 226 slip is flagged today. The scan still misses ordinary forms and one high-impact class (literal paths), and two safeguards fail open. At most three must-fix items:

1. Close the form gaps in the criterion 1 list, one red case each: `rm $d -rf` and `rm $d -r -f`; `"rm"`, `r""m` and `$R -rf`; `ri`, `del`, `erase`, `rd`, `rmdir` with `-Recurse` and the abbreviations `-r`, `-Rec`; `Remove-Item` fed by the pipeline, splatted, or continued across lines; `find ... -delete`; `rsync --delete`; `[IO.Directory]::Delete`; `os.removedirs`; `os.system("rm -rf " + p)` and `subprocess.run(["rm","-rf",p])`; bare `rmtree(p)`, aliased `sh.rmtree(p)` and `shutil.rmtree (p)`; `git -C repo clean -fdx`; `fs.rmSync` and `rimraf`. Judge the command stream at command position so the false positives above do not drown the signal.
2. Invert the literal-path rule to an allow-list. Today any recursive delete of a literal path passes, including `rm -rf .git`, `rm -rf /c/` (MSYS drive root), `rm -rf /c/Users/ringe/foo` and `shutil.rmtree("C:/Users/ringe")`. The card's invariant is "a literal path inside a known scratch root through safe_delete stays allowed": make that the only exemption, flag every other recursive delete, and rewrite the slice's "not widened and not claimed" paragraph (the 2026-10-08 wipe was a mis-quoted path that landed on a real directory, which is exactly this class). Also scan the contents of Write and Edit tool calls, flag `bash <file>` and `python <file>` for a file the seat wrote this session, and treat `-EncodedCommand` and `-enc`, `base64 -d | sh` and `eval` of a built string as findings (decode UTF-16LE base64 and rescan where possible).
3. Make the coverage gaps fail closed. An unscanned seat (Copilot, Grok, agy, or Codex before the rollout shape is pinned) and a subagent seat with no `command_scans` entry must fail the record check, not print a Safety flag, and `check_record` must require the `command_scans` key for every record of card 227 and later (keyless is a failure from 227 on, not ok). In the detector: a future-dated commit is a FAIL, the age is the older of committer time and reflog time, any non-zero git exit is a FAIL, and the HEADs of other worktrees are included.

## Amendment for the security review (ctx-2)

Seat: architect (slice), ctx-2, Claude seat (probes at 09:34 UTC: Codex, Copilot, Grok and agy without usable quota). Written 2026-10-31 after the security review (c6b831c, CHANGES NEEDED, three must-fix items). I read the review, the card, its in-progress record, D136, D157, D167 and the re-rank note (2026-10-31-13). Sizes below are estimates from a read of delete_guard.py (219 lines), tests/test_delete_guard.py (176), mob_process.py (603), facilitator.py (1650) and decider.py (327); they are not measured, and QA and the backend developer re-count at each commit.

### Answers to the three must-fix items

1. Command-form gaps. Accepted in part, into criterion 1; the rest moved out to a named follow-up card (below). Why in part: the reviewer's list holds about 45 forms and they are not one concern. One group varies a verb the scan already parses (flag position, aliases, abbreviations, import styles). The other group hides the verb (quoted or split verb, verb in a variable, base64, argv lists, string concatenation, other runtimes) and needs a decoder, a second parser and new fixtures. Putting both in one red would break D136 (see sizing). Accepted into criterion 1: judging the command stream at command position (the false-positive control the reviewer required, and a precondition for scanning commands at all, because the slice's own probe found about 70 prose strings in 20,658 commands); flags after the operand; all PowerShell aliases and parameter abbreviations; shutil.rmtree in its import styles; `find -delete`; `git -C repo clean`. Moved out: see the follow-up list.
2. Literal-path allow-list. Accepted into criterion 1 in full for the delete verbs, and the slice's "not widened and not claimed" paragraph is withdrawn. Moved out, with the reason: scanning the contents of Write and Edit calls, flagging `bash <file>` and `python <file>` for a file the seat wrote, and `-EncodedCommand` / `base64 -d | sh`. Reason: they add a second transcript input (a different tool_use shape), a base64 decoder and a session-wide "files written" state; none of them changes what a command at command position means, and each is its own red-green cycle. The residual is stated in the rule text (below), not left silent.
3. Fail-closed coverage. Accepted in full: into criterion 1 (record check) and criterion 3 (detector), including the reflog age and the worktree HEADs. Two consequences the facilitator should expect. Any seat on Copilot, Grok, agy, or a Codex without a pinned rollout shape makes the card record fail until the follow-up reader exists or the seat is moved to Claude. Every record of 227 or later without a `command_scans` entry per task-force context fails; the old "keyless is ok" rule applies only to records before 227.

### What stays in card 227 (part 1)

Command-form list for criterion 1, each form its own red case, all judged at command position (start of a clause after `;`, `&&`, `||`, `|`, newline, or after `sudo`, `command`, `time`, `nohup`, `env`/`VAR=1`, `bash -c`, `sh -c`, `xargs`, `eval`, `find -exec`, a backslash, a path prefix like `/bin/`):

- A. The card 226 slip and the quoting and flag family: `rm -rf $SP/mut 2>/dev/null`, `rm -rf "$S"/x`, `${S}`, `-r -f`, `-fr`, `-vrf`, `-R`, `--recursive --force`, `rm -rf -- $d`, command substitution and backticks, globs, `$HOME/...`.
- B. Flags after the operand: `rm $d -rf`, `rm $d -r -f`, `rm $d --recursive`.
- C. PowerShell: `Remove-Item` with `-Recurse`, `-r`, `-Rec`, `-Recurse:$true`, before or after the path, `-Path` / `-LiteralPath`, `-Force` in either order; the aliases `ri`, `rm`, `del`, `erase`, `rd`, `rmdir` with the same switches; lowercase.
- D. cmd: `rd /s /q`, `rd /s/q`, `rmdir /s`, `del /s /q`, `%VAR%` paths (existing, kept).
- E. Python: `shutil.rmtree(p)` with kwargs, f-string, multi-line, `python -c`; `shutil.rmtree (p)`; `from shutil import rmtree` then `rmtree(p)`; `import shutil as sh` then `sh.rmtree(p)`.
- F. `find ... -delete`; `xargs rm -rf`; `find -exec rm -rf {} +`; the `git clean -fdx` family and `git -C repo clean -fdx`; `git rm -rf .`; `git clean -n` stays allowed.
- G. Literal-path allow-list (must-fix 2). A recursive delete is allowed only when its target is a literal, lexically normalised strict child of a scratch root. The roots are an explicit input (`SCRATCH_ROOTS`: repo-relative `scratch/` plus any absolute scratch directory the caller passes, as `safe_delete` takes its roots). Flagged: `.git`, `/c/`, `C:\`, `/c/Users/ringe/foo`, `/tmp/x`, `~`, `..` anywhere in the path, `scratch/../.git`, `scratch` itself (mirrors `safe_delete` refusing the root), `scratch-evil/x` (a prefix is not a parent), bare words (`foo`, `node_modules`, `build`), `shutil.rmtree("C:/Users/ringe")`, `shutil.rmtree("..")`. Allowed: `rm -rf scratch/mut`, `./scratch/mut`, `Remove-Item -Recurse -Force .\scratch\x`, `shutil.rmtree("scratch/x")`, and an absolute path under an absolute root the caller passed. No roots at all means nothing is allowed (fail closed).
- H. Unchanged and must stay quiet: `rm -f file.txt`, `rm file`, `git clean -n`, `kill 1234`, `Remove-Item file.txt`, `rd` / `del` without `/s`, `safe_delete(...)`, words containing the verb (`farm`, `docs/rm-notes`), and prose at non-command position (`git commit -m "never run rm -rf $d"`, `echo "rm -rf is banned"`, `grep -n "rm -rf" file`, a heredoc that writes the rule). The reply-text scan keeps its current substring behaviour; the command-stream scan is the one that judges at command position.
- I. Fail-closed coverage (must-fix 3). A transcript that is missing, empty, unreadable or without an assistant record is a finding (the Zero case, kept). A backend with `command_source: "none"` and a subagent seat with no `command_scans` entry fail `check_record` for card 227 and later; the only exemption is a seat the record names as not run.

Criterion 3 additions (must-fix 3, in): a commit dated in the future is a FAIL (negative age); age is taken from the older of the committer time and the earliest reflog entry for that commit, where one exists; any non-zero git exit, including a locked or `safe.directory` failure, is a FAIL naming the command; the HEAD of every worktree (`git worktree list --porcelain`) is in scope beside HEAD and the branches with an upstream.

### What moves out (part 2): follow-up card

Draft title (the facilitator files it; I do not): Recursive-delete scan covers hidden, encoded and non-shell forms, and the contents of files a seat writes.

Draft scope, in the reviewer's order: hidden verbs (`"rm" -rf`, `r""m`, `$R -rf`, `printf` split verb piped to `sh`); `-EncodedCommand` / `-enc` (decode UTF-16LE base64 and rescan) and `base64 -d | sh`; `eval` of a string built with the verb split; Remove-Item fed by the pipeline, splatted or continued across lines; `[IO.Directory]::Delete`; `rsync --delete`; Python `os.removedirs`, `os.system("rm -rf " + p)`, `subprocess.run(["rm","-rf",p])`; Node `fs.rmSync`, `rimraf`, perl `rmtree`; scanning the contents of Write and Edit calls and flagging `bash <file>` / `python <file>` for a file the seat wrote this session; child processes that write their own transcript (`claude -p`, `python tool.py`); the Codex rollout reader and readers for Copilot, Grok and agy, if a command source exists; and the non-delete commands the reviewer named that can destroy unpushed work (`git reset --hard`, `git checkout -- .`, `git branch -D`, `git push --force`). It could also carry the repositories the detector does not watch (play-tracker, BaccaratSim, scratch clones) and a scheduled run of `unpushed`, so the check does not depend on a seat running.

Until that card is done, the rule text that the template and the avowry agreement get (the criterion 3 rule-text commit, extended to the scan) says it plainly: the scan sees commands at command position in a Claude transcript, after the fact; encoded, hidden-verb and written-file forms are not covered.

### Does the opt-in blocking hook leave the card (part 3)

Yes. `hooks/delete_guard_hook.py` and its README snippet leave card 227 and go to the same follow-up card (or one of its own; the facilitator decides). Reasons: it is the only preventive piece, but it is opt-in and this card installs nothing, so it cannot make Done check 1 true on any machine by itself; Done check 1, worded "through a hook or transcript", is met by the transcript scan; it is a different seam (stdin JSON and exit codes) and about 140 lines with its tests. Consequence for the card's title: with the hook out, card 227 detects and fails the record after the fact; it does not block. I recommend the facilitator say so to the product owner in one line and reword Done check 1 from "blocks" to "reports and fails the record (blocking: follow-up)". I do not edit the board file.

### Product owner's one-card ruling

It still holds. The scan and push-on-every-commit stay together in 227; the only spill is command-form coverage and the hook. The re-rank note's reversal ("227 splits at ready, scan first, push detector second, if it is too big") is not triggered: the push detector is the smaller half (about 300 lines of code and tests plus its wiring), and splitting it off would not make the scan fit; trimming the form classes does. What changes from the earlier text is that the over-limit counts are now stated (see the sizing).

### Sizing against D136 (honest)

Per commit (at most 15 countable files and 400 changed lines): every planned commit is estimated under both limits; the largest is QA red 1b (about 300 lines, 5 files). Per criterion (one criterion per red-green cycle, D157): three criteria, three cycles. Criterion 1 is the long one: red 1a and red 1b, then six backend commits before its green. That keeps D157's order (green 1 before red 2), but the red-to-green gap is long; the backend developer pushes each unit commit at once, and green 1 is the last of them (the record check).

Per card (D136: at most three criteria, three increments, one concern): criteria 3, within the limit. Increments 4 (scan engine; transcript source and wiring with the record check; trust-ladder record; push detector), one over. Concerns 2 (the scan, the push detector), one over. Both overruns come from the product owner's one-card ruling, not from the form list. The slice field in the trail record should read "ship-as-is, 4 increments, 2 concerns, over the per-card limit by one each, product owner ruling 2026-10-31", so the audit sees a stated overrun. The trust-ladder record could be folded into the detector's avowry commit to count three increments, but that is relabelling, not a smaller card, so I do not.

Estimated commits, mob-defaults unless named (lines are tests plus code):

| # | Commit | Criterion | Files | Lines |
|---|---|---|---|---|
| 0 | QA, avowry: Intent line (D165) in .mob/records/227 | none | 1 (not countable) | about 10 |
| 1 | QA red 1a: form table in tests/test_delete_guard.py (classes A to H, hostile-input answers per D167) | 1 | 1 | about 160 |
| 2 | QA red 1b: tests/test_command_scan.py, three transcript fixtures, FAKE_CLI transcript mode, decider voter case | 1 | 5 | about 300 |
| 3 | Backend unit: command-position parser and the scratch-root allow-list (delete_guard.py) | 1 | 1 | about 150 |
| 4 | Backend unit: classes B to F (flag position, PowerShell aliases and abbreviations, rmtree import styles, find -delete, git -C clean) | 1 | 1 | about 120 |
| 5 | Backend unit: command_source.py (claude transcript reader, streaming, Zero) | 1 | 2 | about 160 |
| 6 | Backend unit: `delete_guard.py scan-transcript` CLI | 1 | 1 | about 60 |
| 7 | Backend: run_backend session id and `scan_out`, CardRun Safety flag, decider `command_scan` field, `command_source` in core/backends.json | 1 | 4 | about 130 |
| 8 | Backend green 1: `check_record` requires `command_scans` from 227; unscanned and missing entries fail | 1 | 2 | about 130 |
| 9 | QA red 2: ladder tests | 2 | 1 | about 150 |
| 10 | Backend green 2: `ladder` subcommand, `safety_deviations`, template trust-ladder record rule | 2 | 3 | about 140 |
| 10a | Avowry: card 226 entry in .mob/records/trust-ladder.jsonl and the ledger Notes sentence | 2 | 2 | about 10 |
| 11 | QA red 3: tests/test_push_detector.py (boundary 299/300/301, future date, reflog age, worktree, git failure, backup-branch case) | 3 | 1 | about 280 |
| 12 | Backend unit: `unpushed_commits` (HEAD, upstream branches, worktrees, reflog age) | 3 | 1 | about 130 |
| 13 | Backend green 3: `unpushed_problems`, `unpushed` subcommand, fail on non-zero git exit | 3 | 1 | about 100 |
| 14 | Backend: facilitator Push flag, `run_audit` includes both repositories | 3 | 2 | about 70 |
| 15 | Backend: template rule text (5-minute limit, detector name, the command-scan residual sentence); avowry agreement text as a separate avowry commit | 3 | 1 + 1 | about 30 |
| 16 | Delivery lead at ship: targeted modules per commit, one full suite, both repositories pushed, `unpushed` prints no FAIL | none | 0 to 2 | small |

Total: about 18 commits across both repositories and about 2,100 changed lines (the first plan was about 14 commits and 1,800 lines; the extra is the allow-list, the fail-closed record check and the detector hardening, offset by dropping the hook and the hidden-verb group). If a commit goes over a limit the route is a smaller commit, not a Slice-exception (none of the three kinds fits); commit 2 is the likeliest to overrun and then splits fixtures from the test module. The card is within the per-commit and per-criterion limits and over the per-card increment and concern limits, by the product owner's ruling.

### Inputs (D167), replaced

Criterion 1 Inputs. disabled: a backend with `enabled: false` is skipped and never scanned; a backend with `command_source: "none"` is no longer "unscanned, flagged" but a `check_record` failure for the seat (`test_a_backend_without_a_command_source_fails_the_record`); an empty scratch-root list allows nothing. unknown: a tool name in the transcript that is not Bash or PowerShell is counted, not scanned (hidden-verb and written-file forms belong to the follow-up card and the rule text says so); a recursive verb with a target the parser cannot read is a finding; a root that is not absolute or not a directory is ignored, and if that leaves no roots nothing is allowed; an alias or switch spelling not in classes B to F is not claimed. repeated: the same command twice is two commands in the count and one finding line per distinct text; the same seat scanned twice is not double-flagged; the same scratch root twice, in two spellings (trailing slash, backslash, case), is one root. reordered: flags and targets in any order (`rm $d -rf`, `Remove-Item $p -Recurse -Force`, `-Force -Recurse $p`); several clauses in one command are each judged; findings come in file order; roots in any order give the same verdict.

Criterion 2: unchanged (no list input; one test shows the order of jsonl lines does not matter).

Criterion 3 Inputs. disabled: n/a (the caller names the repositories). unknown: a path that is not a git repository, a repository with no origin, a non-zero git exit, a locked or unsafe repository: each a FAIL naming it; a commit dated in the future: a FAIL; a worktree whose HEAD is detached or on a branch without an upstream is in scope. repeated: the same repository twice, or in two spellings, is reported once; a commit reachable from HEAD and from a worktree is reported once. reordered: swapping the repositories gives the same findings in the same sorted order (repository, then age).

### Slice verdict, amended

SHIP AS IS, with the named split (part 2 and the hook go to the follow-up card). Classes A to I plus the criterion 3 additions are the content of criteria 1 and 3; the over-limit counts for increments and concerns are stated, not hidden. If commit 2 cannot stay under 400 lines, split fixtures from the test module; if criterion 1's green is not reachable inside a seat's cap, the next cut is class F (find -delete and the git forms) into the follow-up, not the allow-list or the fail-closed check, which are the reviewer's high-impact items.
