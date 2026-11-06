# Card 217 scope proposal turn (architect, Codex ctx-2, read-only; commands through cmd.exe; reply extracted)

Facilitator notes (facts): the seat wrote nothing (its reply is the text below, with the Codex CLI banner, prompt echo and a trailing shell-cwd line trimmed). Its Size line says 4809 bytes; the facilitator measured 4808 before that line. Trigger: criterion 3 needs a checker cycle and a CI-evidence cycle (a seat cannot push or wait for CI); the same shape as card 264 criterion 1. The seat ran on HEAD e9f708f. The facilitator holds a with-floor local TRX of the full acceptance project on 3c5712e (316 tests, 313 passed, 3 failed, 11 m 1 s; TEMP and TMP set to a scratch folder) under the scratch run folder, not yet committed.

---

# D175 scope proposal — card 217

## Premises

All observations are from committed HEAD `e9f708f`.

- `git log --oneline -12` includes `197cdb4 Card 217 green 1`, `3c5712e Card 217 green 2`, and `da3c8bd Card 217 red 3`: criteria 1 and 2 are green and pushed; criterion 3 is red.
- `type tests\test_run_pairs_217.py` specifies the checker and record, one head, two runs each for `local` and `ci`, complete outcome JSON, allowed failures with owners, TEMP/TMP, CI identity, and a `removed|kept` floor decision. Its real-record test stays red until the record exists.
- `type tests\Phrase.Acceptance.Tests\Support\ThreadPoolFloor.cs` shows `[ModuleInitializer]` and worker/I/O minima of 128.
- `type .github\workflows\ci.yml` shows job `test` runs `dotnet test Phrase.sln --no-build -c Release --filter "Status!=Pending" --logger "console;verbosity=normal"`; no TRX/artifact is uploaded. CI outcomes therefore come from the acceptance-project block of the Test-step console log. A workflow re-run is the second execution at the same head.
- `type .mob\records\217\turn-slice-architect-r2.md` and the slice decision show criterion 3 capped at one cycle and caps 1+1+1.
- Local full run on `3c5712e`: unrun by this read-only seat; facilitator output is 316 tests, 313 passed, the three F-065 tests failed, 11 m 1 s.
- Current CI outcomes/duration: unrun: network is blocked. Planning datum: about 16 minutes per execution.
- `type .mob\records\264\scope-proposal-architect-r2.md` and its decision establish the precedent: split a long-job criterion into checker and record cycles, approving one extra cycle above D175.

## Amendment

Split criterion 3, ordered **3a then 3b**.

**3a — checker and experiment.** A seat implements the checker plus strict TRX-to-outcomes and CI-console-to-outcomes extractors. Each emits `{fully-qualified-test: outcome}`, rejects missing/duplicate/malformed names, and records source identity. The CI extractor accepts only the acceptance-project section and checks its count against that section's summary.

Establish a with-floor baseline first, avoiding another round if removal fails. Reuse the `3c5712e` local run only if its TRX, complete outcomes, TEMP/TMP and exact 40-hex head survive; obtain its second local run and the same-head push CI run plus one facilitator-started workflow re-run. Otherwise obtain missing pair members.

Then remove `ThreadPoolFloor.cs` in the 3a commit and take two consecutive full local runs on that tree (about 22 machine minutes), retaining TRXs and derived lists. Fixture tests pass; the real-record test intentionally stays red because no no-floor CI pair exists yet.

**3b — remote evidence and decision.** The facilitator pushes 3a, waits for its push workflow, starts one workflow re-run at that head, and waits. A seat obtains both attempts with `gh run view <id> --log` or `gh api` job logs and extracts outcomes. Since a re-run retains the run ID, the record/checker also distinguishes `run_attempt` while retaining `github_run_id` and `head_sha`.

The seat writes `.mob/records/217/run-pairs.json` and four no-floor outcome lists with common head, identities, allowed failures/owners, TEMP/TMP and floor decision. The checker and real-record test pass.

New caps: criterion 1 one, criterion 2 one, criterion 3 two; sum **4**. The window is asked to approve exactly one cycle above D175's limit of 3 and the staged real-record red between 3a and 3b.

## Failure semantics

A pair is green only if its maps are identical and every non-passing outcome is one of the three F-065 tests owned by card 162 or 303. Never replace a failed execution with a cleaner one.

If either no-floor pair differs or has a non-allowed failure, restore the floor in a commit and record `kept`, the reason, and the evidence. The card closes on `kept` only when the with-floor local and CI pairs are green. If the baseline was incomplete, obtain a new pair on the restored-floor head; never combine heads.

If both no-floor pairs are green, record `removed`. Log truncation, missing boundaries, count mismatch, wrong head, or indistinguishable attempts is an evidence defect, not a test result: retain it and rerun only that collection with the reason recorded.

## Reversal triggers and cost

Reverse to one criterion-3 cycle only if all four same-head records exist without waiting. If CI logs lack complete outcomes, re-slice to add TRX logging/artifact upload in 3a. Return to findings if owners are stale, inventory changes, or restoration changes outcomes.

No-floor cost: about **22 local machine minutes**, **32 CI minutes**, and **2 seat cycles**, with no seat waiting. A missing with-floor baseline adds up to **22 local** and **32 CI minutes**; the surviving run can save **11 minutes**. The facilitator owns pushes, CI waits and re-runs.
Size: 4809 bytes
