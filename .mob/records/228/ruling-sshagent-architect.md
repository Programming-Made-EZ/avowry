# Card 228 architect ruling: eval "$(ssh-agent -s)"

Intent (D165): decide whether eval of ssh-agent output is cleaned by a later green or recorded as a finding, so criterion 1d does not teach the scanner a hole.

Ruling: (b) recorded finding, fail closed (D169). No clean twin; no green teaches an allow in card 228.

Reasons
- Slice line 63 says it "keeps its card 227 result". The code (delete_guard.py, eval branch near line 694, KIND_DELETE) reports it today, and no tests/test_command_scan*.py test pins it. The only mention is the 1d docstring, which says "NOT pinned either way". So "keeps" claims a pin that does not exist.
- eval of $(X) runs whatever X prints; the scanner cannot see that output. Any allow keyed on the command name is only as safe as PATH and the binary, and a one-form allow invites widening to eval "$(X)".
- The narrowest allow (the exact string) is brittle: spacing, quoting and `ssh-agent -s` variants each need a twin. It also bypasses via a shadowed ssh-agent or a prefix like `x; eval ...`.
- False-positive cost: one rare line in agent commands, easily rewritten (run ssh-agent -s, source the output from a file). It is not the counter-metric's driver; measure it first.

Action: the next turn record corrects the slice's wording: line 63 twin list drops this form; it is reported as "recursive delete on a variable or unresolved path" and recorded as a known finding. Do not edit the slice. Revisit as a separate card only if the false-positive counter shows real cost.
