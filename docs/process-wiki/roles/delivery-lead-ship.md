What the delivery lead reads and writes at the ship step.

- Rule: Before ship, run the closure record check in dry-run mode and confirm every cited hash and record path resolves on origin. See [D154](../../decisions/process-and-staffing.md#d154).
- Rule: The retro for card N is pushed before issue N is closed and before any later commit that names a different card. See [D136](../../decisions/process-and-staffing.md#d136).
- Rule: The record check compares evidence with the record committed immediately before this card's record. See [D159](../../decisions/process-and-staffing.md#d159).
- Rule: Every commit is pushed, and the detector for unpushed commits must actually be called. See [D169](../../decisions/process-and-staffing.md#d169).
