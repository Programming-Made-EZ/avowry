# Sign-off, card 227 (product manager, signoff seat) — run wiki-signoff-1

## Pages relied on
- docs/process-wiki/INDEX.md (route row `product-manager-signoff`: core/index, role/product-manager-signoff, rules/decisions, rules/stand-downs)
- docs/process-wiki/roles/product-manager-signoff.md (D154 metrics are evidence not targets; D157 keep/revise/drop by Decider; D161 lock down in tooling and save to template)
- docs/process-wiki/rules/decisions.md (D153 objection window vs full Decider; D170 product owner ruling recorded as a decision, not voted)
- docs/process-wiki/rules/stand-downs.md (D151, D155 review prior adjustments before new ones)
- board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md

Not read: TEAM_WORKING_AGREEMENT.md, .mob/records/227*, decisions/227*, card 227 git history.

## Limit of this review
The code and detector live in mob-defaults (delete_guard.py, scan, push detector; range 524171f..f8a45da), which this repository does not contain. I can check each Done check against the card text only. D154 requires git evidence for each check, so the evidence below is what I require, not what I have seen.

## Done checks against the card text
1. **Scan blocks variable/computed-path recursive deletes in executed commands, failing test first (tonight's slip from card 226, then Remove-Item -Recurse -Force, shutil.rmtree, equivalents).** Status line claims: command scan reads every seat's executed commands; record check fails a variable-path recursive delete or an unscanned seat. This covers the adopted condition (D145: executed commands, Zero case of empty/missing source). **Claimed met, wording deviation:** the check says "blocks", the Status says "detected, not blocked". Accepted only via the product owner ruling D169 (detection after the fact acceptable). Evidence needed: red commit (Criterion: 1) with the failing test of the exact slip, a test on a destructive command absent from reply text, the Zero-case test, a green commit, QA ZOMBIES, and the hostile-input list (D167).
2. **Trust-ladder entry for the slip (seat, rule, step down or stated reason none applies).** Status: "trust-ladder entries for both slips". Claimed met. Evidence needed: the entry in the template's trust-ladder record naming seat, rule and step down or stated reason. Note the card says "the slip" and the Status says "both slips"; confirm which two and that each is complete.
3. **Push on every commit: working agreement and template say so; detector FAILs on commits unpushed more than about 5 minutes on avowry and mob-defaults.** Status: push on every commit with unpushed-commit detector. Claimed met. Evidence needed: rule text in both the working agreement and mob-defaults template; detector test that fails for a commit older than ~5 minutes, naming repository, commit and age; passes when pushed; git only, no gh calls (invariant); QA ZOMBIES.

Invariants to confirm: literal scratch path through safe_delete still allowed; process-kill rules unchanged.

## Verdict
**Sign off, conditional on the git evidence above being present and matching.** On card text all three checks are claimed met, and the "detected, not blocked" gap is covered by D169. If any of the listed evidence is missing (in particular the red-first failing test for check 1 or the detector's age test for check 3), I do not sign off for that check, because D154 ties sign-off to evidence from git. D169 condition 2 (recursive deletes never leave the working tree) is guaranteed by card 228, not by this card; I record that dependency and do not count it as delivered here.

## Metrics and counter-metric evidence required (evidence, not targets — D154)
- Unflagged recursive deletes on variable paths: observed 0 across the scan's seats, with a non-empty command-scan source for every seat (empty or missing source = FAIL, not 0).
- Commits unpushed more than ~5 minutes on avowry and mob-defaults: observed 0, with detector runs shown over a real period, not one run.
- Counter-metric: false positives on literal scratch paths through safe_delete (count and examples) and replies reworded to dodge the scan (count; check the stand-down and transcripts for evasions).
Report the numbers as observed; a zero obtained by a scan that cannot see commands does not count.

## What the decision rules require me to record, and who decides
- Record this sign-off as a routine per-card decision through an objection window (D153); a full Decider is needed only on an objection, a one-way door, or a standing-rule change.
- The "blocks" to "detects" change in check 1 is a product owner ruling (D169), recorded as a decision and not voted (D170). It must be written with its id and its two conditions (commit and push regularly, with the 5-minute detector called; recursive deletes only inside the working tree, via card 228).
- The push-on-every-commit rule and the scan are standing rules added to the working agreement and template: that is a rule change, so a **full Decider** decides it, not this sign-off and not an objection window. Product work resumes only once the process is locked in tooling and saved to the template (D161), so I confirm the template carries both.
- Stand-down: open by reviewing each previous adjustment (D155) — card 226's stand-down named the slip but adjusted nothing — then record what worked, what did not and one adjustment (D151).
- Any keep/revise/drop verdict on the experiments touched (push-per-commit, command scan) is decided by Decider at the trial retro (D157), not by me.

Size: 5571 bytes
