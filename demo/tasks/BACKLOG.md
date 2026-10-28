# Tasks API backlog

1. Create — a valid title creates an open task; empty and overlong titles return their declared validation failure.
2. Get — a known id returns its task; an unknown id returns `NotFound`.
3. List — available storage returns its tasks; unavailable storage returns `StoreUnavailable`.
4. Complete — a known task becomes done; an unknown id returns `NotFound`.
5. Delete — a known task is removed; an unknown id returns `NotFound`.
6. Validation — titles must be non-blank and at most 200 characters, reported as `EmptyTitle` or `TitleTooLong`.
