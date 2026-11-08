# Card 178 seat plan (facilitator, 2026-11-08 UTC): fresh probes and the spread

Guidance applied (the product owner's clarification): roles before models; lean on non-Claude drivers where reasonable; no cap; Claude a full member; review, sign-off and Decider seats can go to any driver; the retro reports the seat mix with Claude's share and the reason for every Claude seat.

Probes (one-line prompt "Reply with exactly the word OK", run in parallel from a scratch directory):
- Grok (`grok.cmd -p ... -m grok-4.6 --always-approve --permission-mode plan`): OK, 4.2 s. Available. Note: in plan mode a long deliverable can end after narrating its reading; the Ready draft was obtained without plan mode under the read-only instruction, and the repository stayed untouched.
- Codex: usage limit reached, "try again at Nov 11th, 2026 12:10 AM". Excluded: out of quota; re-probe at every seat assignment.
- Copilot: "You have no quota". Excluded.
- agy: RESOURCE_EXHAUSTED, "Resets in 137h27m". Excluded.
- Muse (`muse.cmd exec --prompt-file F`): OK, 22.4 s. Available on trial; not yet fitted to decider.py.
- Cursor (`cursor-agent.cmd -p --trust --mode ask --output-format text PROMPT`): OK, 7.9 s. Available on trial; no backends entry yet.
- Local Qwen: probed at the card 176 plan (OK for replies; CRLF writes); not re-probed (it loads a 35B model and runs only while the gate is idle).
- Claude: available.

Assignment: the PM draft Grok; the Decider seats and any confirmation step: Grok and Claude (the runner falls back from Grok to Claude and from Claude to Grok); slice, green, gate reviewer and sign-off: fresh Claude contexts; the red and the cold-reader: Grok (the cold-reader on Muse or Cursor if the harness allows without a delay, else Grok); the trial drivers get a low-stakes seat on this card only through the cold-reader seat; if the harness is not ready the reason is recorded and the probe repeated at the next plan.
