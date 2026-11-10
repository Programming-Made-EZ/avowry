# Card 376 second read-only review (Grok, reply-only, plan mode; the script diff inlined, scrubbed of the account name)

Tested tip: main d6a12bc. Launch note: the first launch carried one removed line holding the old literal name (disclosed in retro-input.md); that run was stopped and its reply discarded; this is the second run with the scrubbed prompt. Reply verbatim:

## FINDINGS

1. **F10 is not a programming whole-word** (`home_paths.py`, `_account_pattern`: `(?<![^\W_])…(?![^\W_])`). A letter or digit blocks; `_`, `-`, `.`, `(` do not. Failing inputs: account `jdoe` on `user_jdoe_id = 1` or `foo-jdoe-bar`; account `max` on `max(1, 2)`. Hook and `--tree` both fire F10.

2. **Spaced segment closer is too tight** (`_CLOSE` / `_SEG_STR` lookahead: `\`, `/`, `"`, `'`, backtick, `%5c`, `%2f`, EOL only). Known limits cover four words, double spaces, tabs, and prose-after-name. They do not cover `)`, `,`, `.`, `;`, `]`, `>`. Failing input: `(C:\Users\<user> Cd)` or `C:\Users\<user> Cd,` — two-word form fails, only `Ab` is rewritten, `Cd` stays.

3. **Bench can print an absolute path** (`check-bench-history-clean.py`, `print(f"{path}:{line_number}:{kind}")` with `path = Path(argv[0])`). Failing input: invoke with an absolute history path; every leak line prints that path (drive prefix included). Match text and account are still omitted.

4. **Bench F10 is this machine’s filtered names only** (`scan_history` → `local_accounts()`, `GENERIC_ACCOUNTS`). Failing input: history `"runner": "admin"` when the account is `admin` (or `user`/`root`) — names are dropped, F10 is skipped, that line is not a “user name” hit. `C:\Users\<user>\…` is still a drive-path hit.

## NOT A PROBLEM

- Space rule runs only inside PATH/SLUG, not on bare prose. Three-word `/` over-scrub (`C:\Users\<user>/x`) is the documented safe direction.
- `--tree` / hook: relative `path:line:form` or `account name (F10)`; no match text; F10 checked/skipped line; `return 1 if ordered else 0`; 50-line cap with `... K more` on the full list; one hit per line; F10 omitted when a home-path already hit; bench strips drive/tilde from `rest` before names.
- `MIN_ACCOUNT_LENGTH` 3, allow-list, casefold dedupe, `IGNORECASE`, `re.escape`. `scrub_tracked` does not apply F10.

## VERDICT

**ACCEPT WITH NOTES** — F10 boundaries and `_CLOSE` miss real neighbours; document or tighten them, and print the bench path as a relative name.

## Facilitator ruling

ACCEPT WITH NOTES. None of the four findings fails a scenario of the card: (1) F10 is a whole word with letter/digit boundaries by design (the draft's rule), a local-only check whose cost is a false positive on an account that is also a common word, which the length floor and the generic list reduce; (2) the closer set of the spaced rule is exactly the slice's list, and a spaced account followed by ) , . ; ] > keeps its second word (a gap in forms, not an overreach); (3) the bench checker prints the path it was given (an absolute path if the caller passes one; the account name and match text are never printed); (4) the bench name check uses the local filtered names, as designed. Findings 2 and 3 are filed as one bug after the card; 1 and 4 are stated limits.
