# Train and ship

When: before a train or deploy starts, and after it finishes.

- The inbox is clear: no OPEN entry left unacknowledged. Gate: `inbox_open.py`
- Every commit is pushed. Gate: `mob_process.py unpushed`
- The push gate ran on the pushed records. Gate: `templates/hooks/pre-push.default`
- Toolchain versions are stamped before and after; a changed toolchain voids the verdict. Evidence: the stamp lines in the train's log. Card: #6
- The deployed version is verified after deploy. Evidence: the live version read after the deploy. Card: #44
