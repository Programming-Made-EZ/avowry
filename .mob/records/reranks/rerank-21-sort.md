# Re-rank 21: D183 sort of the open cards (independent review)

- Judge: independent reviewer seat, fresh context; not the proposer of any card sorted here (most process cards were filed by the facilitator). This list changes nothing; it goes to a Decider.
- Date: 2026-11-03 (local; the records carry 2026-11-04 UTC for D184).
- Read: D183 in docs/decisions/process-and-staffing.md (principle, effects, the five-point merit-review procedure, and the condition not adopted: at most one process change per story's diff); TEAM_WORKING_AGREEMENT.md line 36; D182 (its measure: product cards closed per day, hours to the first product card Done; guards: open FAILs and audit findings per card must not rise, empty attempts per window near 0 with every counted vote cited); D184 (deferred retro experiments); rr21-open-cards.json (158 cards); each process card's board file (Status, Type, Source, Done checks); card 228 slice 639c5e2 and sign-off b0e23e7; card 229 sign-off (.mob/records/229/signoff-ctx6.md) and pre-ship gate; mob-crosscli git log and `git ls-files --eol`; loop_runner.py.
- Definition used (D183 procedure point 1): a PROCESS BUG is a real defect in safety, decision votes or tests, shown by a failing check, a reproduced wrong vote or safety outcome, or a named incident (file, commit, window or audit). Flaky and cross-shell test failures are test defects; seat safety and delete scanning are safety; empty, uncited or misparsed votes are decision-vote defects. A process card without such evidence is a RETRO EXPERIMENT and leaves the board, with an owner (a role), its proof (the kind of story and the measure) and a story trigger or review date. PRODUCT is Avowry itself (language, compiler, CLI, docs, packaging, benchmarks, studies, the rename). DELIVERED and SUPERSEDED name the card or commit.
- Not judged here: product-card order, and whether this re-rank carries or replaces D182's measure and guards (procedure point 5, the product manager's call).

## Sort

| Card | Title (short) | Disposition | Reason | PROCESS BUG: check, reproduction or incident | RETRO: owner; proof; trigger or review |
|---|---|---|---|---|---|
| 282 | A seat's prompt reaches it without placeholder rewriting | DELIVERED | Card 229's Done check 3 delivered it. | | Red cef48a7, green 2d4d24d (mob-crosscli), tests/test_prompt_placeholders.py; card 229 sign-off section 1 check 3 Met |
| 277 | An empty decision vote opens a diagnosis | RETRO EXPERIMENT | The empty-vote defects are fixed (229, check 2) or carded (291); what remains is a diagnose-and-rotate rule, not a wrong vote. | | Decider seat; proof: on a story whose window gets an empty or voteless vote, a diagnosis is filed and the model returns; measure empty attempts per window near 0 (D182 guard); trigger: first empty or voteless decision vote after 291 is Done |
| 264 | The read-and-modify acceptance test is not flaky | PROCESS BUG | Flaky product acceptance test (test defect). | Audit 123 FAIL (GitHub issue #243); D182 left it unverified whether still open: verify first, and if open it goes first | |
| 265 | Decision windows launch voters in parallel safely | RETRO EXPERIMENT | Speed improvement; no lost or corrupted vote named. | | Backend developer; proof: a story with a multi-voter window; measure window minutes with seat-use.jsonl still valid; trigger: a window over its cap from serial launches |
| 258 | The loop-state lock test is not flaky | PROCESS BUG | Flaky test. | tests/test_loop_state.py test_lock_released_when_holder_exits failed in the full suite on e4f7c29, passes alone (card 228 sign-off b0e23e7, condition 1) | |
| 268 | The mob summary publish tests pass in every shell | PROCESS BUG | Cross-shell test failure. | .mob/records/210/preship-gate.md: 2 tests in tests/test_mob_summary.py fail under cmd /c in the Copilot CLI on c633d3a, pass in a plain shell | |
| 269 | mob_process.py keeps LF line endings | RETRO EXPERIMENT | Real churn (mob_process.py is still i/crlf) but not a safety, vote or test defect. | | Backend developer; proof: the next story touching mob_process.py adds .gitattributes and its diff matches the change; trigger: that story |
| 238 | The reshaped process wiki carries the slice limits and upkeep | RETRO EXPERIMENT | D173 wiki follow-up; no defect. | | Agile coach; proof: a story whose seats read the wiki; measure token delta and upkeep minutes per card; review: re-rank 23 |
| 230 | The facilitator and seats never write the same checkout at once | PROCESS BUG | Two writers on one checkout lost staging and voided a vote (safety, decision votes). | Auditor: 3 index-lock collisions 2026-10-30/31, 2 push races 2026-10-31; commit 3c82909 swept a seat's staged file; a facilitator commit moved HEAD under a running voter and voided its vote | |
| 236 | The process wiki closes the writer's seven content gaps | RETRO EXPERIMENT | Seven minor content gaps; no failing check. | | Agile coach; proof: the next wiki-routed story closes the gaps it touches; measure wiki misses per card; trigger: next wiki-routed seat |
| 253 | Codex rollouts are read by the delete scan | PROCESS BUG | Codex seats unscanned: a safety Done check left open. | Card 228 sign-off b0e23e7 (signoff-ctx6.md): Done check 2 PARTLY MET, Codex rollouts exist on this machine and are not read | |
| 252 | Seat transcripts are scanned automatically (subagent, headless) | PROCESS BUG | Subagent and headless seats go unscanned: safety. | Card 237 left 15 of 17 seats unscanned (card 228 slice 639c5e2, F1) | |
| 254 | The delete scan cuts its known false positives | PROCESS BUG | False findings on read-only commands: reproduced wrong safety outcomes. | False-positive classes found on card 228's own seat scans (slice 639c5e2, F3; the "5-of-6 false positive class") | |
| 255 | The delete scan covers the residual forms and path forms | RETRO EXPERIMENT | Uncovered forms enumerated at slice, none reproduced from a seat command. | | QA; proof: a story whose seat uses a listed form gets a red for it first; measure missed forms in scans = 0; trigger: first seat command in a listed form, or next story touching delete_guard.py |
| 231 | Uncommitted work older than about 30 minutes fails the checks | PROCESS BUG | D169's after-the-fact safety assumes regular commits; a named exposure broke it. | Auditor: card 227's last green uncommitted for about 70 minutes across full-suite waits | |
| 232 | A seat driver never opens a browser or other UI | PROCESS BUG | A seat opened UI on the product owner's machine: seat safety. | Incident 2026-10-29 19:58 PDT, aider-20261029-195650.log, traced by the auditor to the local-seat probe | |
| 233 | Evaluate side-by-side pairing tooling | RETRO EXPERIMENT | A spike; no defect. | | Agile coach; proof: a driver/navigator slice; measure rework cycles and tokens per slice against a solo slice; trigger: the first story planned for pairing |
| 235 | Green commit bodies carry a checked Proof block | RETRO EXPERIMENT | Missing Proof blocks (c941885, 9daab9f) are record hygiene, not a failing test. | | Audit lens; proof: every green of the next story carries a checked Proof block; measure greens without one = 0; trigger: next story's first green |
| 256 | Command-scan reports are readable and trusted | RETRO EXPERIMENT | Report plumbing; no wrong scan outcome named. | | QA; proof: a story's command-scan entry renders and a later edit is flagged; review: re-rank 23 |
| 257 | Git-state detectors catch unpushed and hidden work | RETRO EXPERIMENT | No named loss or incident for these states. | | Audit lens; proof: a story in a side branch is flagged when unpushed; measure hidden work found by audit, not detector = 0; trigger: first audit finding hidden or unpushed work |
| 266 | Docs and code consistently use the name Avowry | PRODUCT | The rename; stays. | | |
| 267 | mob-defaults docs contain no dates | RETRO EXPERIMENT | Template wording principle; no defect. | | Agile coach; proof: the next story editing a template removes dates in what it touches; measure dated lines; trigger: that story |
| 216 | The default templates carry the pending process rulings | RETRO EXPERIMENT | Template content; D183 itself changes what it must say. | | Agile coach; proof: the next new mob starts from the template with no hand-added ruling; trigger: next mob spin-up or template story |
| 220 | The loop runner starts a corrective card while a finding blocks | RETRO EXPERIMENT | F-008 (a FLAG: missing detector) runner gap; no wrong run recorded. | | Architect; proof: an unattended run of a corrective card proceeds; measure loop stops per card; trigger: next unattended loop run |
| 221 | The loop runs a card in the repository its line names | RETRO EXPERIMENT | Runner gap found by reading code; cards are run by hand, never observed. | | Architect; measure commits in the wrong repository = 0; trigger: next unattended loop run |
| 222 | The loop wires retro and re-rank steps and stops when one fails | RETRO EXPERIMENT | Exit codes ignored, found at slice; no incident. | | Architect; measure a failed ceremony stops the loop; trigger: next unattended loop run |
| 223 | The loop takes the per-card retro from every role | RETRO EXPERIMENT | Hand-run today; an improvement. | | Agile coach; measure facilitator minutes per retro; trigger: next unattended loop run |
| 224 | The loop re-ranks Ready through the PM and a Decider | RETRO EXPERIMENT | Hand-run today; an improvement. | | Product manager; measure hours from Done to next card; trigger: next unattended loop run |
| 225 | Card mode reads only role lines as the task force | RETRO EXPERIMENT | Misparse verified at slice, but it fails closed and is not a vote, safety or test defect. | | Facilitator; proof: the next story's task force parses to its declared roles; trigger: a card refused for a non-role line |
| 213 | The checker rejects an incomplete objection window | RETRO EXPERIMENT | D153 L3b checker rule; no incomplete window that passed is named. | | Audit lens; measure incomplete windows found by audit = 0; trigger: first incomplete window an audit finds |
| 217 | Acceptance tests do not starve the thread pool | PROCESS BUG | Flaky acceptance tests; open FAILs. | Open FAILs F-063 and F-065 (audits 34, 37); card 214 commit 111960d full runs failed 3 and 6 tests that pass alone | |
| 218 | A mid-card retro removal does not drop the seat early | RETRO EXPERIMENT | Staffing loophole found at slice; no seat was dropped. | | Facilitator; proof: a mid-card "removed: retro" keeps the seat to the retro; trigger: the first such line written mid-card |
| 212 | Every mob re-ranks at every loop boundary | RETRO EXPERIMENT | Done by hand already; template part belongs to 216. | | Product manager; measure missing re-rank notes over the next three stories; review: re-rank 23 |
| 215 | Every mob reviews each previous adjustment | RETRO EXPERIMENT | Ceremony improvement; template part belongs to 216. | | Agile coach; measure reviewed / prior adjustments at the next story retro; trigger: next story retro |
| 206 | The findings file mirrors the audit register | RETRO EXPERIMENT | F-008 corrective (FLAG); no wrong audit outcome named. | | Audit lens; measure file/register mismatches per audit; trigger: next audit that finds a mismatch |
| 207 | The findings file history rules are checked | RETRO EXPERIMENT | F-008 corrective (FLAG); no named history rewrite. | | Audit lens; measure history-rule violations = 0; trigger: as 206 |
| 196 | Rename the two test projects to Avowry.*.Tests | PRODUCT | The rename; stays. | | |
| 197 | Rename the solution, CI jobs, scripts and paths to Avowry | PRODUCT | The rename; stays. | | |
| 162 | Fold the pattern steps into the grammar and cheat sheet | PRODUCT | Product docs; stays. | | |
| 201 | Migrate the sealed holdouts to avow, new baseline | PRODUCT | Rename and benchmark; stays. | | |
| 189 | An unsupported construct is a diagnostic | PRODUCT | Compiler bug; stays. | | |
| 176 | Package the CLI as a dotnet tool and version it | PRODUCT | Packaging; stays. | | |
| 178 | A prerequisites page that matches what CI tests | PRODUCT | Docs; stays. | | |
| 179 | Rewrite the README and the getting-started guide | PRODUCT | Docs; stays. | | |
| 186 | Pre-publication checks: name, licences, history, terms | PRODUCT | Release; stays. | | |
| 169 | The map bytes do not depend on the culture | PRODUCT | Compiler; stays. | | |
| 171 | The CLI reads names from the adapter; Program.cs is tidy | PRODUCT | CLI; stays. | | |
| 172 | Trace hardening follow-ups and a shared test helper | PRODUCT | Product code and tests; stays. | | |
| 165 | Failure observer: a neutral failure event | PRODUCT | Language feature; stays. | | |
| 166 | Move the generated [Phrase] link into the source map | PRODUCT | Compiler; stays. | | |
| 177 | CI installs the packed tool on Windows and Linux | PRODUCT | Packaging; stays. | | |
| 200 | An avowry fmt command | PRODUCT | CLI; stays. | | |
| 180 | A test that runs the getting-started guide's commands | PRODUCT | Docs test; stays. | | |
| 181 | A cold model completes the getting-started guide | PRODUCT | Docs check; stays. | | |
| 187 | A known-limitations page | PRODUCT | Docs; stays. | | |
| 188 | Publish the first version | PRODUCT | Release (one-way door); stays. | | |
| 163 | Generated endpoint files are named *.cs.cs | PRODUCT | Compiler convention; stays. | | |
| 182 | Actionable messages on the getting-started path | PRODUCT | Diagnostics; stays. | | |
| 164 | line directives point at .phrase lines | PRODUCT | Compiler; stays. | | |
| 184 | Version 0.1.0 and a changelog | PRODUCT | Release; stays. | | |
| 153 | Verdict page states the success criteria | PRODUCT | Product evidence page; stays. | | |
| 185 | An issues route, contributing and security notes | PRODUCT | Docs; stays. | | |
| 155 | Verdict page as a judgment backed by data | PRODUCT | Product evidence page; stays. | | |
| 156 | Personas capture the same ten fields | PRODUCT | Product personas; stays. | | |
| 145 | Store operations as Phrase steps | PRODUCT | Language; stays. | | |
| 146 | Validation rules as Phrase | PRODUCT | Language; stays. | | |
| 147 | A list endpoint without a fake flag | PRODUCT | Language; stays. | | |
| 148 | Composite steps | PRODUCT | Language; stays. | | |
| 134 | ZOMBIES spec-coverage warnings | PRODUCT | Compiler; stays. | | |
| 149 | Undetected-defect rate study | PRODUCT | Product study; stays. | | |
| 150 | Intent recovery study | PRODUCT | Product study; stays. | | |
| 151 | Review speed and accuracy study | PRODUCT | Product study; stays. | | |
| 289 | Drift resistance study | PRODUCT | Product study (D182); stays. | | |
| 139 | Loop step: re-probe unavailable drivers | RETRO EXPERIMENT | Mob loop tooling; an improvement ("coach, don't bench"). | | Facilitator; measure hours a recovered driver stays out; trigger: next driver quota-out |
| 140 | Cheat sheet plus library index, measured | PRODUCT | Product measurement; stays. | | |
| 143 | Split the documentation: core and web sheet | PRODUCT | Docs; stays. | | |
| 144 | Rerun Copilot's unavailable holdout rows | PRODUCT | Benchmark run; stays. | | |
| 107 | Spec to code with a model | PRODUCT | Product experiment; stays. | | |
| 108 | A second target's Specs project | PRODUCT | Compiler; stays. | | |
| 109 | C# implementation emission | PRODUCT | Compiler; stays. | | |
| 115 | Which cheaper models run headless and cold | PRODUCT | Benchmark probe; stays. | | |
| 116 | Pre-register the cheaper-model experiment | PRODUCT | Benchmark; stays. | | |
| 117 | Run the cheaper models in both arms | PRODUCT | Benchmark; stays. | | |
| 118 | Cost per passing task report | PRODUCT | Benchmark; stays. | | |
| 120 | Pre-register the codebase-size experiment | PRODUCT | Benchmark; stays. | | |
| 123 | Run the codebase-size experiment | PRODUCT | Benchmark; stays. | | |
| 124 | Codebase-size report | PRODUCT | Benchmark; stays. | | |
| 125 | Choose and pin public C# repositories | PRODUCT | Benchmark; stays. | | |
| 126 | Port a slice of the pinned repo to Phrase | PRODUCT | Benchmark; stays. | | |
| 127 | Growth-study backlog and pre-registration | PRODUCT | Benchmark; stays. | | |
| 128 | Run the growth study | PRODUCT | Benchmark; stays. | | |
| 129 | Growth-study curve | PRODUCT | Benchmark; stays. | | |
| 089 | Option and nested-record fields compile | PRODUCT | Compiler; stays. | | |
| 065 | Loop: run one card with a pinned toolchain | RETRO EXPERIMENT | 2026-10-26 loop design (D23); card 208's runner now runs cards; improvement. | | Architect; measure toolchain-drift failures; trigger: first story run unattended by the loop |
| 066 | Loop: bounded retry, park and split | RETRO EXPERIMENT | Retry exists (run_loop max_attempts=3); park and split are improvements. | | Architect; proof: a failing story parks and splits, not stops; trigger: first loop stop "blocked" |
| 078 | Ready check: reject a role-only story | RETRO EXPERIMENT | Ready-check lint; improvement. | | Product manager; measure stories sent back for role-only wording; trigger: next such story |
| 067 | Loop: on Done run retro and re-rank, then next | SUPERSEDED | Delivered by card 208's runner; remaining gaps carried by 222, 223, 224. | | By card 208 (Done, sign-off ef393a3) and 222 to 224 |
| 053 | Docs: one diagnostics table, not two | PRODUCT | Docs; stays. | | |
| 062 | Atomic runs: fresh worktree, landed or discarded | RETRO EXPERIMENT | Loop design (D22); improvement. | | Architect; measure partial landings = 0; trigger: first story run unattended by the loop |
| 068 | Loop: liveness, publish, STOP and Needs-PO file | RETRO EXPERIMENT | STOP file and heartbeat exist (064, 208); the rest is improvement. | | Facilitator; proof: an unattended run writes a Needs-product-owner file when needed; trigger: as 062 |
| 072 | Loop: pace the work against driver allotments | RETRO EXPERIMENT | Improvement. | | Product manager; measure quota stops per card; trigger: next quota exhaustion |
| 069 | Loop: resume after a crash or reboot | RETRO EXPERIMENT | Improvement; no incident named. | | Backend developer; measure minutes lost; trigger: first reboot during an unattended run |
| 070 | Loop: run every card atomically | RETRO EXPERIMENT | Depends on 062; improvement. | | Architect; proof and trigger as 062 |
| 054 | Benchmark task 005 gets a failure-path spec | PRODUCT | Benchmark; stays. | | |
| 056 | An endpoint with an output cannot say succeeds | PRODUCT | Compiler; stays. | | |
| 022 | Keyword model ADR | PRODUCT | Language design; stays. | | |
| 058 | A validation phrase returns its validated value | PRODUCT | Language; stays. | | |
| 013 | Make GRAMMAR.md say what the resolver does | PRODUCT | Docs; stays. | | |
| 019 | Publish exact diagnostic message templates | PRODUCT | Docs; stays. | | |
| 014 | Document the native C# environment | PRODUCT | Docs; stays. | | |
| 046 | Docs: adapter contract and HTTP status mapping | PRODUCT | Docs; stays. | | |
| 017 | Benchmark v1: score by running the specs | PRODUCT | Benchmark; stays. | | |
| 024 | Pattern discovery | PRODUCT | Language; stays. | | |
| 027 | Benchmark: catalogue tasks | PRODUCT | Benchmark; stays. | | |
| 060 | Catalogue tasks for concurrent work and events | PRODUCT | Benchmark; stays. | | |
| 081 | Measure stored and published outcomes | PRODUCT | Benchmark; stays. | | |
| 020 | Did-you-mean suggestions | PRODUCT | Compiler; stays. | | |
| 052 | phrase build reports file:line:col | PRODUCT | CLI; stays. | | |
| 023 | Ordinary control flow | PRODUCT | Language; stays. | | |
| 033 | Generic type parameters on phrases | PRODUCT | Language; stays. | | |
| 034 | Behavioral holes and spec test doubles | PRODUCT | Language; stays. | | |
| 059 | ADR: deterministic async and event semantics | PRODUCT | Language design; stays. | | |
| 082 | ADR: a spec actor is who observes an outcome | PRODUCT | Language design; stays. | | |
| 025 | Base library v0 | PRODUCT | Language; stays. | | |
| 015 | Proposal: make the spec when line optional | PRODUCT | Language; stays. | | |
| 021 | Proposal: consistent given/responds syntax | PRODUCT | Language; stays. | | |
| 035 | Proposal: parameterizable failure codes | PRODUCT | Language; stays. | | |
| 038 | Dogfood: the mob status API, written in Phrase | PRODUCT | A second real Phrase app; stays. | | |
| 026 | Catalogue dedup linter | PRODUCT | Tooling for the language; stays. | | |
| 061 | Spike: scheduler, fake clock, event capture | PRODUCT | Language spike; stays. | | |
| 083 | Spike: in-memory store and event capture | PRODUCT | Language spike; stays. | | |
| 028 | Second adapter spike | PRODUCT | Language; stays. | | |
| 029 | LLM-as-compiler with hash pinning | PRODUCT | Language; stays. | | |
| 030 | Synthesize Phrase from existing code | PRODUCT | Language spike; stays. | | |
| 031 | Converter: LLM-optimized and human rendering | PRODUCT | Language spike; stays. | | |
| 003 | Adapter contract and C# adapter: types and runtime | PRODUCT | Compiler; stays (unranked: check whether already Done). | | |
| 004 | C# adapter: phrases, effects and failures | PRODUCT | Compiler; stays (unranked: check whether Done). | | |
| 005 | C# adapter: FastEndpoints endpoints and Program | PRODUCT | Compiler; stays (unranked: check whether Done). | | |
| 006 | C# adapter: specs as xUnit, first acceptance test | PRODUCT | Compiler; stays (unranked: check whether Done). | | |
| 007 | Phrase CLI: one command compiles a program | PRODUCT | CLI; stays (unranked: check whether Done). | | |
| 063 | Loop: pick the next eligible card | SUPERSEDED | Card file says SUPERSEDED by 076. | | By card 076 (Done, mob-defaults 6c04bff) |
| 071 | Loop: discover new cards when Ready runs low | SUPERSEDED | Card file says WITHDRAWN, folded into 067. | | By card 067, itself superseded by 208 |
| 202 | A native block with an undeclared code is a diagnostic | PRODUCT | Compiler; stays. | | |
| 203 | Names reserved by generated code are rejected | PRODUCT | Compiler; stays. | | |
| 204 | Reconcile the personas with the v0.1 bar | PRODUCT | Product docs; stays. | | |
| 271 | The loop and audit thresholds are parameters | RETRO EXPERIMENT | Hard-coded values are a cleanup, not a wrong outcome. | | Architect; proof: a story that needs another threshold sets it without a code edit; trigger: that story |
| 272 | facilitator.py and test_card_mode.py keep line endings | RETRO EXPERIMENT | Sibling of 269; disputed for the final tree; no failing check. | | Backend developer; fold with 269 (facilitator.py still i/crlf); trigger: next story editing facilitator.py |
| 273 | Each gate has a red run that proves its test | RETRO EXPERIMENT | No gate shown to pass its test while broken; reviewer disputed. | | QA; measure gates whose test passes with the gate removed = 0; trigger: next story adding a gate |
| 274 | The final-review record can say unclear | PROCESS BUG | Record tests rewrite reviewer words and votes: misrecorded decision votes. | records/211/final-review.json (f6fb952), card 211 greens 1 and 3: six Support-it votes stored as Yes, "unclear" stored as "adds" | |
| 275 | Decide card 211's unproven defects; seat attribution | RETRO EXPERIMENT | A pending decision, not a defect card. | | Facilitator; proof: the retro decides close or card; trigger already met: card 229's sign-off says every subject commit is authored "Copilot" (D177 recurrence), so take it to the next story's retro |
| 280 | Muse usage text reads as signed in | RETRO EXPERIMENT | Availability defect of a trial driver, not safety, votes or tests. | | Backend developer; measure Muse seats wrongly unavailable = 0; trigger: next story with a Muse seat |
| 281 | Muse seats run with a known workspace trust | PROCESS BUG | Untrusted Muse seats never load the project's PreToolUse delete guard: seat safety. | Muse session 93b23003-d64b-439f-801a-ca8c0ef89e03 log, "Workspace trust: untrusted ... hooks are not loaded" (window 276-scope item 6) | |
| 283 | The delete guard does not flag a read-only python command | PROCESS BUG | Reproduced wrong safety outcome (false finding). | Card 276 red amendment 4c, Copilot session 50624fa6 (2026-11-03 17:04 UTC): a python -c file read reported as a recursive delete | |
| 285 | Muse's sandboxed shell completes a command | RETRO EXPERIMENT | Reproduced (session a7505b9d) but fails closed: availability, not a safety outcome. | | Backend developer; measure escalation requests = 0; trigger: next story with a Muse driver seat |
| 286 | A NO OBJECTION reply is stored as no objection | PROCESS BUG | Misparsed vote. | .mob/records/276/muse-trial-seat/decider-result-2.json line 35; card 276 sign-off section 5 | |
| 287 | locate_muse takes one kind of root | RETRO EXPERIMENT | API cleanup. | | Backend developer; done inside the next story touching command_source.py |
| 290 | The decider's result counts only cited votes | PROCESS BUG | An uncited Yes counts toward "passed": wrong vote outcome. | Window 229-scope-2 (.mob/records/decisions/229-scope-2.json); .mob/records/229/green2-cycle2-attempt.diff | |
| 291 | A Grok seat ends with a vote; a vote after narration is read | PROCESS BUG | Empty and dropped votes. | Windows 229-scope and 229-scope-2: 8 of 10 Grok attempts voteless, 2 dropped (w229scr2cp1-coach, w229sc2cp1-coach); card 229 sign-off section 4 | |

## Counts (158 cards)

- PRODUCT: 98
- PROCESS BUG: 16 (264, 258, 268, 230, 253, 252, 254, 231, 232, 217, 274, 281, 283, 286, 290, 291)
- RETRO EXPERIMENT: 40
- DELIVERED: 1 (282)
- SUPERSEDED: 3 (067, 063, 071)

## Borderline calls

1. 277 (empty vote opens a diagnosis): RETRO, though it cites a named incident (window 276-taskforce). The wrong outcomes in that window were the votes lost to the cmd.exe limit and Grok narration; card 229 fixed the first and 291 carries the second. Empty votes are already never counted. What 277 adds is a rule for responding, so it waits for the next empty vote.
2. 255 vs 252, 253, 254 (delete scan): the three kept as bugs each have an incident or failed Done check (15 of 17 seats unscanned; Done check 2 partly met; false positives on real seat scans). 255's forms were listed at slice and never seen in a seat's command, so it is RETRO even though it is safety. A Decider that wants safety gaps kept on the board without evidence would be reading back the condition D183 rejected.
3. 231 (uncommitted work): kept as a BUG. The incident (70 minutes uncommitted) lost nothing, but D169 accepts after-the-fact delete detection only while work is committed regularly, so the exposure breaks a stated safety premise. 257 (unpushed and hidden work) has no incident and is RETRO.
4. 274 (final-review record): kept as a BUG on its vote half (Support-it stored as Yes). If the Decider reads that as wording only, since both count as passing, it becomes RETRO.
5. 281 (Muse trust) is a BUG and 285 (Muse sandbox) is RETRO. 281 means a safety guard is missing. 285 fails closed: Muse cannot run anything, which is unsafe for no one.
6. 225, 220 to 224, 206, 207 (F-008 correctives): RETRO. F-008 is a FLAG (a missing detector), and the loop runner is not run unattended. Guard (D182: open findings must not rise): F-008 loses its owning cards, so the audit lens should record the move in the retro log or the findings file, or the auditor will read F-008 as unowned. D182 also noted that audit 200's open FAILs (F-072, F-074, F-078, F-086) are named by no card. This sort adds none.
7. 269 and 272 (line endings): RETRO. The CRLF churn is real and still present, but it is not a safety, vote or test defect. It costs reviewability, and the fix belongs in the next story that touches those files.
8. 264: kept as a BUG on audit 123. D182 left open whether that FAIL is still open. If the auditor confirms it closed with no flake seen since, it becomes DELIVERED or RETRO.
9. Cards 003 to 007 are unranked product cards that may already be Done. They are product, so this sort leaves them alone, but the re-rank should check them.
