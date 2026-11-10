# Card 374 second read-only review (Grok, reply-only, plan mode; the decider.py diff and probe_backend inlined, scrubbed of the account name; hand count on the sent prompt 0)

Tested green: mob-defaults 396e6434b345c256903f1e260611be9b21bc9cc5. Reply verbatim:

## FINDINGS

1. **`probe_backend` has no cwd besides `repo`.** Both `run_backend(..., repo, ...)` and `command_capability_probe(..., repo, ...)` treat that argument as the working tree. A lambda that forwards CardRun’s `repo` still drops `prompt-XXXX.txt` at the real root. Failing input: invocation containing `{PROMPT_FILE}`, two concurrent decision runs, `git status --untracked-files=all` → exit 5. The lambda must be `facilitator.probe_backend(name, cfg, scratch, timeout, stall)` (scratch in the `repo` slot).

2. **Scratch that is not a git work tree is cwd for the command probe too.** Failing input: `"command_capable": true` whose command probe is `git rev-parse` / reads `./gate.lock` / a CLI that only starts in a trusted git folder (typical Claude/Codex default). Start probe then marks the backend down or stalls. Same break for an invocation with a relative binary or `--add-dir .`.

3. **Out of scope, still live:** `probe_all` and card-mode still call this function with the real repository. Failing input: rotation/`probe_all` while another run’s integrity check is running; same `{PROMPT_FILE}` file in the root; exit 5.

## NOT A PROBLEM

- Late lookup of `facilitator.probe_backend` inside the lambda is the correct patch point; tests that `setattr(facilitator, "probe_backend", ...)` see it at call time.
- Arity matches CardRun `(name, cfg, repo, timeout, stall)` → positional `stall_seconds`.
- Once scratch is passed as `repo`, both the availability `{PROMPT_FILE}` write and the command probe follow it. Integrity without `--ignored` will not see files in a gitignored scratch.

## VERDICT

**ACCEPT WITH NOTES** — the concurrent integrity failure is fixed only if scratch is passed as `repo`; confirm git/trust CLIs still probe from that directory, and that other `probe_backend` callers remain a known leak.

## Facilitator ruling

ACCEPT WITH NOTES. Finding 1 rests on a misreading: the lambda passes the run's scratch directory in the repo slot (`facilitator.probe_backend(name, cfg, run_dir, timeout, stall)`), exactly as the review asks, and test 374-3a proves the prompt file lands under the run directory. Finding 2 (a scratch directory that is not a git work tree) was checked live: the real probe of every backend from a non-git scratch directory passed for agy, claude, cursor, grok and qwen (gate-baseline.md). Finding 3 (probe_all and card-mode probes still use the real repository) is out of scope and named in the slice (finding 9 of the card 365 slice); it is carried as a follow-up candidate.
