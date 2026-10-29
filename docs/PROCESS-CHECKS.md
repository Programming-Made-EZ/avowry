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
gh issue list --state closed --label card --limit 1000 --json number,title,labels,closedAt
gh issue list --state closed --limit 1000 --json number,title,labels,closedAt
gh repo view --json nameWithOwner
gh api repos/{owner}/{repo}/issues/{issue-number}/timeline --paginate -H 'Accept: application/vnd.github+json'
```

`python3 scripts/mob_process.py write-mob-md` is the only MOB.md writer. Its `generated_at` is the newest commit's Git date, never the wall clock, so identical repository and issue inputs produce identical bytes. The records remain self-attested until D134's product-owner actions establish branch protection and a separate auditor identity.
