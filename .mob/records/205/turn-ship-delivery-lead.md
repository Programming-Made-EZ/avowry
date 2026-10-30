# Card 205, ship seat: delivery lead (ctx-6, Claude Sonnet subagent)
- Ship checks, all PASS: the six commits 945c409, ec15cd5, d13bbe4, 97226b9, 2290341, 46864d0 are on mob-defaults origin/main (46864d0), each names MD-001, MD-002, MD-003 and D145, each within D136 (at most 3 files and 90 lines), none touches .mob/.
- Persona demo (D150), in a throwaway clone with its own demo findings and audit log: an issue naming F-900 gave exit 0; an issue naming nothing gave exit 5 with the printed refusal; a card with no issue gave exit 5 (fail closed).
- Independent review before ship: the independent reviewer (audit lens, ctx-5) APPROVE, no defects; tests.test_mob_process 52 OK, tests.test_card_mode 102 OK.
- Reason for a Claude seat: probes at 06:35 UTC show no other model with usable quota.
