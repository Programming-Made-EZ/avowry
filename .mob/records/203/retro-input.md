# Retro input (card 203, written at the declaration): root cause of four failed vote attempts, 2026-11-08 UTC

Dispatch watchdog: no product commit between 14:32 and about 16:30. Process time lost to the declaration of card 203: about 75 minutes against no build time yet. Root causes, in order of cost:

1. The Ready brief was sent through decider.py (the vote wrapper): it reads any prompt as a proposal to vote on, so Cursor answered "Yes" and Muse hit the runner cap on a long reading brief (1,500 s, no reply). A Ready draft goes through cursor_reply.py (or a reply-only seat), never through the vote wrapper. About 30 minutes.
2. Concurrent decider runs in one checkout: each run's integrity check saw another run's prompt-*.txt in the repository root, and the seat-use history check failed ("not append-only", bug 365); a 35-second stagger did not help because each run lasts minutes. Muse was also out of quota (429) by then and Grok was still in the fallback chains, so every fallback added more concurrent runs. About 25 minutes.
3. The shared vote prompt template for the run_votes_par.py runner (the reply format and the CITE line, D171) lives in the CTX variable I set by hand in the D226 window; for D227 I launched the runner without it, so six seats answered a bare "Yes" and nothing counted. The cause is an ad-hoc launch after the seat swap, not the runner. About 20 minutes.
4. The Ready draft's section 6 still named Grok as the red seat; one Qwen attempt answered No on that. The declaration proposal now marks the section superseded.

Fixes: the shared vote prompt (reply format and CITE) is stored in a committed file and the launcher reads it (no hand-set variable); run_votes_par.py drops dead drivers from its fallback chains; windows are staggered but the integrity-check interference needs a repair (bug 365 covers the seat-use race; the stray prompt files in the repository root are a second cause to file); a Ready draft never goes through the vote wrapper.
