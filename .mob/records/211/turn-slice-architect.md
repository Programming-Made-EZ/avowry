Intent: Deliver the MD-010 corrective as records-only evidence for commits 687410e..e3736d1, proving whole-team ruling, independent re-run, and non-author final-state review without rewriting history.
Assumes: identity-level independence is required for criterion 3 and cannot be satisfied by context rotation alone when commit identity is shared; probe: git evidence for range 687410e..e3736d1 shows 27 commits by Xipooo <xipooo@gmail.com> and checklist/proposal records state every seat currently commits under that same identity.

Criterion 1 (Done check 1): A Decider record and votes file in mob-defaults rule on 687410e..e3736d1 with one valid vote per declared role seat, each vote attributable to its seat context and carrying a D171-valid CITE; red: the committed record test fails while either file is missing or any seat vote/reason/CITE/context is missing. ZOMBIES: Z no votes file/Decider record; O one seat vote only; M all declared seats vote with reasons; B all but one seat still fails; E uncited, unread, or unattributed vote is refused; I record names the full range. Check plan: verify committed Decider+votes artifacts exist, enumerate declared seats vs votes 1:1, check CITE lines resolve to proposal text, and confirm each vote rules on 687410e..e3736d1.
Cap: criterion 1, at most 1 cycles

Criterion 2 (Done check 2): A non-author context independently re-runs the mob-defaults suite and audit checks on the range endpoints and on head, with commands, commit hashes, and results recorded; red: the committed check fails until that re-run record exists and reproduces range+head evidence. ZOMBIES: Z no independent re-run record; O only one commit/state checked; M whole range endpoints plus head checked; B first commit 687410e and last e3736d1 explicitly included; E failures are recorded as findings (not hidden); I recorded commands reproduce results. Check plan: from committed record, verify non-author context, verify commands and outputs for range and head, and verify audit checks are included and reproducible.
Cap: criterion 2, at most 1 cycles

Criterion 3 (Done check 3): A final-state review record is committed by an identity that authored none of the 27 commits, cites MD-010 and 687410e..e3736d1, and lists author identities from git; red: the committed check fails until a qualifying non-author-identity review commit exists. ZOMBIES: Z no review commit; O one reviewer recorded; M agreement/disagreement can both be recorded if identities qualify; B review by author identity does not count; E review missing explicit commit/range citation is refused; I git identity comparison proves reviewer identity differs from all 27 author identities. Check plan: compare reviewer author+committer identity to git author set for the 27 commits, verify MD-010 and range citation, and verify listed author identities match git log evidence.
Cap: criterion 3, at most 1 cycles

D167 hostile-input list:
- SEEN: Single-author identity across 27 commits is treated as independent due to seat/context differences. Pointer: board card 211 Given/Context and checklist-211-taskforce line naming all 27 by Xipooo <xipooo@gmail.com>.
- SEEN: Shared committing identity for all seats is mistaken for non-author review eligibility. Pointer: checklist-211-taskforce and taskforce-proposal-pm blocker text.
- INSURANCE: Co-authored-by trailers or role labels are misread as committing-identity evidence for criterion 3.
- INSURANCE: Re-run evidence omits one boundary commit while claiming full range coverage.

Domain-expert (T2) question: Can Done check 3 be satisfied now under shared seat identity, and what do git facts say about the range and commit contents?
Domain-expert (T2) answer from git-backed records: No, not as written. The reviewed range is 687410e..e3736d1 with 27 commits by one identity (Xipooo <xipooo@gmail.com>), and current seat commits use that same identity, so identity-level non-author review cannot be proven by context rotation. The range content is process/template work in mob-defaults (facilitator/rules/tests/docs paths), including recorded observations that b33793a rewrote tests/test_card_mode.py and the range includes facilitator.py line-ending changes; therefore criterion 3 requires either a true non-author committing identity or a product-owner ruling that changes/waives the identity requirement.

Criterion 3 before product-owner ruling:
- Can prepare now: review template text, git-log identity extraction script/commands, MD-010/range citation block, and final-state checklist so execution is immediate once ruling lands.
- Waits on ruling: the actual final-state review commit and any pass/fail claim for criterion 3, because acceptance depends on approved non-author identity path (or explicit waiver/replacement evidence) from the product owner.

D176 note: every gate/suite/audit run used as acceptance evidence must state environment (shell and launch form, working directory, commit hash, command, and timing/test-count context) in the recorded report; runs without environment lines are not counted as gate evidence.
Size: 5088 bytes
