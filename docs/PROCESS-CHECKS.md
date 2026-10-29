# Mob process checks

`python3 scripts/mob_process.py check` is the independent D134 accountability gate. It reads Git history and GitHub issues, prints one `::error::mob-process:` line per violation, and exits 1 if any check fails. It verifies trail records and their evidence, per-card retros, the frozen legacy waiver, deterministic MOB.md freshness, issue timeline events, workflow wiring, and labels on all closed work items.

Dispatch runs it every 30 minutes against a fresh full-history clone with a read-only `GH_TOKEN`:

```sh
git clone <repository-url> audit
cd audit
GH_TOKEN=<read-only-token> python3 scripts/mob_process.py check
```

The checker obtains the audit inputs from GitHub alone with these commands (and reads the committed records from the fresh clone):

```sh
gh issue list --state closed --label card --limit 5000 --json number,title,labels,closedAt
gh issue list --state closed --limit 5000 --json number,title,labels,closedAt
gh repo view --json nameWithOwner
gh api repos/{owner}/{repo}/issues/{issue-number}/timeline --paginate -H 'Accept: application/vnd.github+json'
```

`python3 scripts/mob_process.py write-mob-md` is the only MOB.md writer. Its `generated_at` is the newest commit's Git date, never the wall clock, so identical repository and issue inputs produce identical bytes. `--root PATH` lets a checker loaded from another revision audit the new tree.

The push gate also loads the checker from `github.event.before`, the commit immediately before the push, and runs it against the new tree. A rule change must therefore still satisfy the old checker. The sanctioned path for tightening a rule, or fixing a checker bug that wrongly rejects legitimate state, is two steps: first land the checker fix in a commit the old checker accepts; then use the corrected behavior in a later commit. The previous checker receives `--skip-regeneration`, which skips only the byte-for-byte MOB.md regeneration comparison so a legitimate generator change can land; the state-comment, ancestry, freshness, and done-list checks remain active.

Residual limits are explicit: Git committer dates can be forged; records remain self-attested until the product owner completes D134's branch protection with required checks and code-owner review and establishes a separate auditor identity; and `git push --no-verify` bypasses the local hook. CI and Dispatch are therefore the authoritative detectors, not the hook alone.

## Residual limits (D135)

- A single commit can make the checker permissive and still pass: the previous-version run validates the new tree with the old checker, not the new checker source, and it is skipped on force pushes. The external audit (Dispatch, every 30 minutes, with its own pinned copy of the checker) is the cover until branch protection with a code-owner review by a second identity exists.
- Detection happens after the push. Prevention (pull requests, required checks, no direct or force pushes) is the required destination (D135, option C).
- Records are self-attested until an auditor identity posts an external review or status per card.
