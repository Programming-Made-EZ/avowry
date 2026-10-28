# Experiment E5: a Linux check before push

A process experiment proposed at the Usable v1 retro. The retro decides through the Decider whether to keep it.

## Problem

The local gate runs on Windows only, while main is verified on Linux. Linux-only failures are therefore found after the push, when main is already red and the stop-the-line rule halts the loop. The cost of each such red is the push-to-red delay plus a fix cycle.

## Baseline

Runs on main since the CI fix (2026-10-28, about 36 runs): 27 passed, 4 failed and 4 were cancelled. The failures came from a small number of distinct Linux-only causes: a process-tree test that waited for a killed child (card 137), a determinism test that built through a backslash path that does not exist on Linux (card 161), and earlier the test job hitting its time limit. Before the fix, nearly every run was red for two Windows-only tests that no one saw. The local gate takes about 9 to 15 minutes; a CI run takes about 10 to 17.

## Practice (to trial)

Before pushing a change that touches code or tests, run the acceptance and Python tests in a Linux container with the repository mounted, in parallel with or right after the Windows gate. Candidate means, cheapest first: a container of the .NET 8 SDK image run through the existing container tool on this machine (the machine currently has the tool installed but its VM is stopped, so the first step is a feasibility check, not a commitment); a Linux distribution under the Windows subsystem for Linux with the SDK installed in a distribution used only for this. Not chosen: installing tooling into the existing container machine's own distribution.

## Hypothesis

We believe a Linux run before push will remove most Linux-only reds on main, at a run-time cost smaller than the delay and the cost of one red per few pushes.

## Metric

Over the next ten pushes: Linux-only reds on main (expect near zero), the extra minutes per gate (target at most the length of the Windows gate when run in parallel, and under 12 minutes), and false failures that are only container artefacts (file locks, path mapping). Evidence for a judgment, not a target.

## Trial scope

Step 1: feasibility on one change (start the container machine, pull the image once, run the suite against a copy of the repository, record the run time and any container-only failures). Step 2: ten pushes with the check on. Step 3: the Decider keeps, revises or drops it on judgment at the next retro.

## Reversal

The check is optional tooling in a script plus a line in the working agreement; removing the line and the script restores the current gate. No generated or published artefact depends on it.
