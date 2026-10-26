# Invalid or superseded benchmark rows

Rows are moved here, never deleted. `superseded-rows-2026-10-26.jsonl` holds the 55 rows that were in `../history.jsonl` before the
fixed harness re-ran the baseline on 2026-10-26. They were produced by earlier harness versions that:

- captured nothing from agy (22 of its 24 rows are empty replies recorded as `completed` / `no_phrase_found`),
- did not extract Phrase from fenced or narrated replies,
- passed Copilot a file path instead of the prompt, and left Copilot without a row for one task.

Their pass rates (2 of 29 completed rows) measure those harness faults, not the language. They are kept for the record and must
not be counted as baseline evidence. The baseline is the rows in `../history.jsonl` from the 2026-10-26 re-run.
