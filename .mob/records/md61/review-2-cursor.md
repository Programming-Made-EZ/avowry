VERDICT: FLAG; BLOCKING: 2; MINOR: 2

**1. BLOCKING — wrong rotation (sure)**  
Minimal history (same card/context/seat, two roles):  
`launch grok card=1 ctx=x role=driver` → `launch grok card=1 ctx=x role=navigator` → `outcome success` written via `append_seat_outcome(..., role=driver)` (or `seat_log outcome --role driver ...`).  
Append picks the driver launch (role filter). `last_seating` key is only `(seat, card, context)` and LIFO-pops the navigator launch, so rotation treats navigator as seated and driver as never seated.

**2. BLOCKING — lost outcome (sure)**  
`seat_turn` calls `gate_seat` (writes `outcome=launched`) then returns with no outcome on `--dry-run` and on disabled/missing backend (`not cfg or not enabled`). Launch stays forever unpaired; no end line.

**3. MINOR — corrupt second outcome (sure)**  
`seat_log outcome` twice after one launch: append does not require an unpaired launch, so it rebinds to the same launch. History gains an extra outcome; `last_seating` orphans the second.

**4. MINOR — seat_log accepts bad reason/model (sure)**  
`run_launch` empty-checks card/role/context/service/work_type only. `--declared-reason ""` (or whitespace) and `--declared-model "   "` are accepted; reason can be blank, model can be whitespace.

**Held:** `last_seating` is a single linear pass (pending stack; no forward scan). Writes stay append-only (`"a"` + prefix check); nothing rewrites lines. Same-key LIFO success pairing and legacy “no outcome/event” immediate seating match the card when role does not diverge. `seat_log` refuses empty ids, bad outcomes, non-finite/`<0` duration, and missing launch for card/service/context. Synthetic launch fallback is gone.
