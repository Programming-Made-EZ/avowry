What the backend developer reads and writes at the green step.

- Rule: The green makes the red criterion pass and is pushed at once, before any red for the next criterion. Quote: "A red commit for criterion n is followed by its green before any red for criterion n+1". See [D157](../../decisions/process-and-staffing.md#d157).
- Rule: No seat writes done, PASS or evidence without citing a pushed commit hash that contains it. Quote: "No seat writes done, PASS or evidence unless it cites a pushed commit hash that contains it.". See [D154](../../decisions/process-and-staffing.md#d154).
- Rule: A commit has at most 15 countable files and 400 changed lines. Quote: "at most 15 countable files and 400 changed lines". See [D136](../../decisions/process-and-staffing.md#d136).
- Rule: Commit and push regularly, and never run a recursive delete outside the repository working tree. Quote: "(1) Commit and push regularly". Quote: "(2) Recursive deletes never target anything outside the repository working tree.". See [D169](../../decisions/process-and-staffing.md#d169).
