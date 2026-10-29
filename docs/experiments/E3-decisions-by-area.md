# Experiment E3: decisions by area

A documentation and tooling experiment. The retro decides through the Decider whether to keep, revise, or drop it.

## Problem

The single decision log was 148,475 bytes, about 37,119 estimated tokens at four bytes per token, and held 106 records.
Every fresh-context seat paid that whole reading cost even when its question concerned only one area. Mixing language,
compiler, process, benchmark, and delivery history also coupled unrelated documentation changes.

## Practice

Keep one concern per area document. Store each record byte-for-byte in exactly one owning area, summarize current rules
near the top, and use links for cross-area relationships. Start at the generated index. Keep the old path as a small
landing page, and mechanically check ownership, completeness, supersession targets, index freshness, and migration hash.

## Hypothesis

A fresh-context seat answering “what is in force in my area?” will load the index plus one area file, about a fifth of
the old log's tokens, while still reaching every historical record through stable links.

## Baseline and first measurement

The baseline was 148,475 bytes, 106 records, and about 37,119 estimated tokens. The first generated-file sizes are:

| File | Bytes | Estimated tokens |
| --- | ---: | ---: |
| `INDEX.md` | 20,644 | 5,161 |
| `language-and-grammar.md` | 21,270 | 5,318 |
| `compiler-and-adapters.md` | 36,736 | 9,184 |
| `process-and-staffing.md` | 15,005 | 3,752 |
| `benchmarks-and-measurement.md` | 52,445 | 13,112 |
| `board-and-delivery.md` | 31,790 | 7,948 |
| `areas.json` | 24,505 | 6,127 |
| `MIGRATION.txt` | 82 | 21 |
| legacy-path `DECIDERS.md` stub | 311 | 78 |

Loading the index plus one area currently ranges from 35,649 bytes (about 8,913 tokens) for process and staffing to
73,089 bytes (about 18,273 tokens) for benchmarks and measurement, versus the 148,475-byte whole log. The first result
is 24% to 49% of the baseline, so only the smallest area is near the one-fifth hypothesis; the trial must show whether
the index or the largest area needs another cohesive split without duplicating history.

## Metric

At each use, record the bytes and estimated tokens loaded for `INDEX.md` plus the chosen area, divided by the old log's
148,475 bytes and 37,119 estimated tokens. Also record whether the seat found the governing rule without opening another
area. File size is evidence for judgment, not a target to game by removing necessary context.

## Trial scope

Run through the next retro. The retro uses a Decider to keep, revise, or drop the practice, considering both context cost
and wrong-area or missing-history incidents.

## Reversal

Revert the single migration commit to restore the original `DECIDERS.md` and remove the generated area/index tooling.
The migration hash proves the split history remains complete until that reversal.

Naming note: the decision record D22 already calls the atomic-runs trial an experiment named E3; that trial never had a record file here. This file is the record of the decision-log split and keeps the name E3 in this folder; the earlier use is historical.
