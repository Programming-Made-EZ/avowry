# Retro pick proposal (agile coach seat), re-vote: adopt the product-owner inbox as the decision channel, with a mechanical log check (experiment)

Card 303 retro, 2026-11-06, re-proposal after Decider D191 failed the strict count. Proposer: agile coach seat. For a Decider (number set at recording: D192), a working-agreement experiment (D183: not a card).

WHY THIS IS A NEW VOTE: D191 (the same idea with a committed log) got, in round 2, Yes 1, Support-it 4 and one No from the architect, whose one condition was "record each boundary and immutable entry identity in git, then mechanically reject missing, duplicate, late, or destination-less acknowledgements". Under the Decider protocol an outlier still unresolved after the amendment means the proposal fails, and the product owner ruled (2026-11-06) that a No is never reinterpreted. D191 is recorded as not adopted. This proposal resolves that condition and nothing else changes, so by D145 it adds no new demands. The round 1 and round 2 votes of D191 are in .mob/records/deciders/D191-votes.json.

PICK: adopt the product-owner inbox file (outside the repo, C:/Users/ringe/.mob-inbox/avowry.md) as the channel for product-owner decisions, questions and process proposals, for three cards, with a committed log and a script that checks the log mechanically.

RULE UNDER TRIAL (C1 to C5 and the tunes are unchanged from D191's revision 2; C6 is the amendment):
C1. The facilitator reads the inbox at every card and cycle boundary and at every retro.
C1b. An entry arrives when the facilitator first reads it at a boundary (`read_at`, UTC) and is acknowledged in the same boundary's cycle, before the next card or seat starts. The baseline is the earlier incident the inbox proposal names: product-owner decisions that sat undelivered for more than an hour.
C2. Each entry is moved under ACKNOWLEDGED in the file with a UTC time and where it went (a card or closure record for a decision; docs/retros/experiments.md for a process proposal or question, weighed at the next retro through Decider).
C2b. Each acknowledgement is also appended, as one line, to the committed file .mob/records/inbox-log.jsonl in the same boundary's commit, with these fields: `entry_id` (the first 12 hex characters of the sha256 of the entry's heading line), `title`, `filed` (the heading's date), `read_at`, `acknowledged_at`, `boundary` (the card number and a stage word, for example "303/close"), and `destination`.
C3. GitHub issues stay bugs and stories only (D183); an inbox item is never filed as an issue.
C4. A product-owner decision that changes a seat or rule mid-window is a polled amendment (D187), not a quiet edit of a polled text.
C5. Not changed: the board, the windows, the Decider protocol (which is followed exactly as written, per the product owner's ruling), and the product owner's right to message the facilitator directly in chat.
C6 (the amendment, resolving the architect's condition). The first piece of work of the trial is a committed script scripts/check-inbox-log.py with tests that fail first (red, then green). The script rejects: a log line with a missing field; a duplicate `entry_id`; `acknowledged_at` earlier than `read_at`; `acknowledged_at` more than 60 minutes after `read_at` (late); a `destination` that is empty or names a path that does not exist in the repository; and, when the inbox file is present, an entry the file shows as acknowledged that has no log line. The facilitator runs it before each boundary commit that touches the log, and a rejection is fixed or explained in the commit message. The three-card trial counts from the card after the script lands. The 60 minutes is a number to review at the trial's retro, not a standing rule.

TUNES CARRIED (not conditions): the retro separates boundary-handling reliability from end-to-end delivery latency, because "arrival" is the facilitator's first read; the log's `filed` field lets lateness against the product owner's own date show.

HYPOTHESIS: We believe a file read at boundaries, with a written acknowledgement that a script checks, gets product-owner decisions to the team within one cycle instead of hours. We will know when, over the three trial cards, every entry has a log line that the script accepts.

METRIC: entries acknowledged within one cycle of first read, with a log line the script accepts (card 303: 4 of 4 acknowledged in the file at the boundary they were read: the Codex build-seat decision, the card-format question, the Muse proposal, and the Decider ruling; the log did not exist before D191). Target: all of them over the three trial cards.

COUNTER-METRICS (evidence, not targets):
1. Script rejections per trial card (a rising number means the log is being skipped or the rule is unclear).
2. Unacknowledged entries at each retro (target 0).
3. Mid-card policy changes after a window opened (card 303: one, handled as a polled amendment).
4. Minutes spent on the log and the script per boundary (more than 5 means ceremony).

TRIAL SCOPE: three cards, counted from the card after the script lands. The third card's retro decides keep, revise or drop. Owner: agile coach.

User benefit: the product owner's decisions reach the team within a cycle, and the product owner can see in one file what was decided and where it went, with a check that no entry was skipped.
