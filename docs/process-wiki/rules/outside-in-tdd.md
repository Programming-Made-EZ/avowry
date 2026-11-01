Outside-in TDD: one failing acceptance test per criterion, committed pending, made green in one commit.

- Rule: The red commit adds the acceptance test for one criterion, marked pending and carrying a criterion trait. See [D137](../../decisions/process-and-staffing.md#d137).
- Rule: The green commit removes the pending mark in the commit that makes the test pass, and the test body stays byte-identical. See [D137](../../decisions/process-and-staffing.md#d137).
- Rule: Item 7 of the outside-in rule was amended on the final wording. See [D143](../../decisions/process-and-staffing.md#d143).
