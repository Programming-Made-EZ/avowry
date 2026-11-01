Frozen task text, card 237 pilot trial. Role: signoff. Source card: 227 (closed; board/227-recursive-deletes-on-variable-paths-are-blocked-and-every-commit-is-pushed.md).

Run key: {RUN_KEY}
Context for this run: {ARM_CONTEXT}
Output file: {OUTPUT_PATH}

Your step: signoff (product manager seat). Decide sign-off for card 227.
Produce the sign-off: check each Done check against the card text, give the verdict (sign off or not), the metrics and counter-metric evidence you would require, and what the decision rules require you to record and who must decide any rule change.
Rules for this run:
- Treat card 227 as open and at the step named above. Work only from the board card file, the repository as it is, and the context named above. Do not read anything under .mob/records/227*, .mob/records/decisions/227*, or the git history of card 227: those hold the real answers.
- Apply every rule of the working agreement that governs your step. Name, in the output file, each rule or page you relied on, by path.
- Write one markdown file at the output file path and nothing else. Change no product code, no board file, no existing record.
- End the file with one line: "Size: N bytes".
- Then reply with the output file path and one sentence.
