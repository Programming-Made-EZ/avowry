# Card 188 (publish the first version): what the owner is asked to approve

Status on 2026-11-10: NOT ready to publish. The trademark search is no longer a blocker (declined by the owner, inbox 10:12). Two conditions remain before anything is published: (1) the free first-pass name screen reports CLEAR (the owner's written approval of 10:13 is conditional on it); (2) the cards the publish card depends on are Done: CI installs the packed tool on Windows and Linux (177), version 0.1.0 and a changelog (184), an issues route, a contributing note and a security note (185), and a known-limitations page (187). Published versions cannot be removed.

The three items, one-word answerable once the dependencies are Done and the screen is CLEAR:

1. Package name on the feed: **Avowry** (installed with `dotnet tool install`, run as `avowry`). Version 0.1.0. Answer: yes or no.
2. Licence: **Apache-2.0**, holder Programming Made EZ, with the trademark policy in TRADEMARKS.md. Answer: yes or no.
3. Announcement text: evidence and limits only (what was measured and where it falls short, from the known-limitations page); the text is drafted after cards 184 and 187 are Done and is shown for a yes or no. Answer: yes or no.

On publish day the mob re-checks that the package name is still free on the feed. Checked 2026-11-10: the feed lookup for the id Avowry returns not found (the id is free today). A feed account and API key are needed to publish; no API key is set in this machine's environment, and the mob never enters one itself: so the owner's key is needed at publish time, and Dispatch can relay that.

The owner's conditional written approval (inbox 2026-11-10 10:13) is recorded; it does not replace the three answers above for the announcement text if the text makes claims beyond evidence and limits.
