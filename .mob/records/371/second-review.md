# Card 371 second read-only review (Cursor, ask mode, Probation rung 1; reads files, runs nothing)

Reviewed tip: the green 479f2dbd974a7d5f0060b4d8407615931a425928 (main). Verdict: **REVISE** (nothing blocking). The reply, verbatim:

**Verdict: REVISE.** I found nothing blocking. The holder lines, the cards, the scan counts, the registry list and the brief's anchor all check out. Six cited lines are wrong or unsupported, the brief contains one overclaim, and the wording needs some clean-up.

**What checks out**

- `NOTICE:2`, `TRADEMARKS.md:5` and `docs/LICENSING.md:27` say what the page says they do.
- Cards 366 to 370 exist in `board/` and match the page's descriptions.
- The scan figures match `.mob/records/186/signoff.md:9-11`: 3,124 commits, 65 hits, 45+5+4+8+3, and the home-path counts 146, 277, 381 and 137. The page's account of `scan-history-for-secrets.py` is accurate.
- The seven registry lines match `TARGETS` (`tests/test_pre_publication_checks.py:27`) in both the Name collisions section and the brief.
- The anchor `#name-collisions-not-a-professional-search` matches the heading slug.
- No personal name, email address, real home path or secret appears on the page.

**Findings**

1. **should-fix**, `pre-publication-checks.md:82`: "`docs/decisions/process-and-staffing.md:128`". Line 128 is blank. The D125 screen sentence is on `:129`.

2. **should-fix**, `pre-publication-checks.md:14-17`: "`CSharpProvider.cs:193`" and "`:207`", "`:208`", "`:209`". Those lines are validation code. The PackageReference strings are now at `:237` and `:251-253`.

3. **should-fix**, `pre-publication-checks.md:100`: "Scan tip: `c4692e0…`". This section reads `NOTICE` as filled by card 366 and the owner's answers of 2026-11-08. Both are later than that commit, and at that commit `NOTICE` still held placeholders. The intro at `:3`, "run on the tree at commit … except where a section says otherwise", should list this section as an exception.

4. **should-fix**, `pre-publication-checks.md:100`: "the owner's answers of 2026-11-08". No file holds them. `.mob/records/371/taskforce-proposal-pm.md` has no "PRODUCT OWNER'S ANSWERS" section. The only traces are the cards' "Source: … inbox entry of 2026-11-08" lines and the architect slice. A reader cannot verify answers like "skipped" or "keep".

5. **should-fix**, `:129`: "the earlier brand called Phrase: not checked (the search covers them)". Also `:93`. The brief asks for a search on Avowry, not Phrase, so "covers them" is unsupported. The Phrase brand was a risk for the old name (D124) and stopped mattering after the rename (D125). A provider would be confused; drop it from the brief or explain it.

6. **should-fix**, `:103-114`: ". commissioned by the owner", ". skipped by", ". keep.", ". read-first", ". scrub-tracked", ". restore-and-review". Each sentence starts lower-case, and the one-word codes are mob shorthand. Something like "keep the current author identity" or "remove home paths from tracked files" would read clearly to a stranger.

7. **note**, `:47`: "the red commit". Also "seat-session logs" and "seat replies" (`:42`, `:52`), and the unlinked "D121" and "D125". This is internal jargon a stranger cannot follow.

8. **note**, `:117-131`, the brief itself. A provider could start from it, though they would ask for:
   - whether the mark is a word mark (`TRADEMARKS.md` also mentions a logo);
   - a goods and services description beyond "downloadable software", such as "programming-language compiler and command-line tool";
   - the applicant's country and legal form, which the owner can supply privately.

9. **note**, `:80`: "it passes". This was verified only at `c4692e0`, not at main.

10. **note**, `:1`: "(card 186)" and `:3` "recorded 2026-11-08". Card 371 rewrote the page and its records are dated 2026-11-09. The heading and date undersell the revision.

11. **note**, `board/366…md:5`: "Status: Draft". The page says card 366 filled the files, and `checklist-d233.md:2` says card 366 closed. The generated board file is behind.

12. **note**, outside the page: `board/186…md:11` names the product owner by first name in a public file.

The Owner items, the brief and the Recommendation lines at `:97` and `:133` do not contradict each other.

## Facilitator reading
Findings 1 to 6 are should-fix and go into page revision 1 together with the cold reader's finding: the citation lines of `docs/decisions/process-and-staffing.md` and `CSharpProvider.cs`; the intro's list of sections read at another commit; the page saying where the owner's answers live; the unsupported clause about the earlier brand (finding 5); the capitalised, plain-sentence answers (finding 6). Finding 8 (brief completeness) is taken. Notes 7, 9 and 10 (internal jargon in untouched sections, a verified-at-another-commit claim, the heading and date) are carried to the follow-up note below; note 11 (the generated board file for card 366 is behind) is regenerated at the post-close step; note 12 (a generated board file for card 186 names the product owner by first name in a public file) is a bug for the board generator and is filed separately. The repository is already public (card 186 finding): the review is read-only and found no secret, address or home path on the page.
