# Card 430: rulings on fix slice 1 (facilitator, 2026-11-15)

Fix slice 1 is `slice-fix.md`, with neighbour rows in `corpus-fix.txt`: 196 rows, `fix-time` 46 and `fix-waitasync` 150. It answers rulings R1-1 to R1-3 of review round 1, which are in `slice-rulings.md` on main. The seat was the architect (claude opus = claude-opus-5-5, ctx-1b). Under the 16:51Z watchdog ruling it built and ran only its probe while play-tracker #1065 held the heavy lock.

RF-1: accepted. `HttpClient?x` and `CancellationTokenSource?x` (a `?` touching the declared name) are now named. This is valid C# and a real limit, and it is the outcome that kills the old-pattern switch.

RF-2: accepted as a named limit, with no rows. A limit argument that is an identifier ending in `ct` (such as `strict` or `exact`) is read as a token. No argument in the repository ends that way. Filed on #443.

RF-3: accepted. The red embeds all 196 rows and extends the completeness fact to both `corpus.txt` and `corpus-fix.txt`.

The [E] fact's list is nine classes: the seven plus GeneratedApiBoundAddressEdges and StarvationProbe. The guard does not detect the two new classes, so their attributes do not change its outcome. The [E] fact checks them by reading the source text.
