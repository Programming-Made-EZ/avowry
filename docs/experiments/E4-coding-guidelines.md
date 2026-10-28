# Experiment E4: coding guidelines for this repository's own code

A process experiment taken from the product owner's direction at the Usable v1 retro. The retro decides through the Decider whether to keep it.

## Problem

The review findings of the last cards fall into a few classes that recur: tests that cannot fail, exceptions on malformed input, output that depends on the platform, bundled concerns in one change and generated code that learned about its tooling. Each is a rule that was never written down in one place, so each author and reviewer rediscovers it.

## Practice

`docs/CODING_GUIDELINES.md` is one short document (about 7 KB, roughly 1,700 tokens) covering design, naming, errors and determinism, tests, code hygiene and documents, ending in a ten-item review checklist. Authors read it before building; reviewers check the change against the checklist. It covers this repository's code only and cross-references, without merging, the project-profile standards Phrase applies to generated code.

## Hypothesis

We believe a single cohesive guideline plus a review checklist will cut the number of recurring-class findings per card, because authors avoid them up front and reviewers look for them by name.

## Baseline

Review findings on recent cards, counted by recurring class (tests that cannot fail, exceptions on malformed input, platform dependence, bundled concerns, layering inversion): cards 137, 158, 159, 160 and 161 each had between four and seven findings, and at least two of those per card belonged to these classes (for example the zero-spec pass and the missing malformed-input tests on card 137, the vacuous demo assertion on card 160, the Windows-only tests that kept CI red for the project, and the header and comments in generated code on card 161).

## Metric

The number of recurring-class findings per card for the next ten cards, from the review reports, against the baseline of about two per card. Counter-metric: time and tokens to read the guideline (the document stays under 8 KB), and whether authors report the rules as getting in the way.

## Trial scope

The next ten cards or until the next retro, whichever comes first. Every author goal names the document, and every reviewer brief carries the checklist.

## Decision

At the retro the Decider keeps, revises or drops the document on judgment, with the metric as evidence and not as the target.

## Reversal

One commit adds the document and the two links; reverting it removes the experiment. The rules themselves stay recorded in the review reports.
