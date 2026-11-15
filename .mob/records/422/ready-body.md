**Status: refined from the card 369 second review into the card template, 2026-11-15.** The restored-package licence page (`docs/release/pre-publication-checks.md`, section "Restored package licences", card 369) lists the 30 packages the generated C# service and specs restore from two committed lockfiles, with each package's licence, kind and where-used, and prose figures. Its checks are `tests/test_restored_package_licences.py`, run through `tests/Avowry.Acceptance.Tests/RestoredPackageLicences.cs` and `RestoredPackageLicencesLockedRestore.cs`. The review found the checks pass on wrong pages: a changed licence value, prose figures that no longer match the rows, tool package references written in other legal MSBuild forms, and lockfile properties under a false Condition. Separately, the old-prefix guard (`NoPhCodesInTheLiveTree.cs`) allow-lists the two lockfiles (H6, H7) only because their base64 content hashes contain the old prefix by chance. CI runs on main with SDK 8.0.x, while the lockfiles were made with SDK 10.0.403, so the first CI run on main after this card is the [O] observation. The networked checks wait for the cross-mob heavy-run lock like any acceptance run.

## Persona

Dana

## Story

As Dana, I want the restored-package licence page to be checked as strictly as it is written, so that I can trust its licence column and its totals when I adopt the preview without reconstructing generated details (goal: review the Phrase source and its specs without reconstructing generated details).

## Acceptance criteria

- Scenario: [Z] Given the committed page, lockfiles, provider and tool projects, when the offline and networked checks run, then they pass with no failure, and the old-prefix guard names no file.
- Scenario: [E] Given the page row of xunit.abstractions 2.0.3 or System.Reflection.Metadata 1.6.0, when its licence value is changed to a different licence, then a networked test fails naming the package and both licences (today it checks only that a licence file exists or that the nuspec has a licenseUrl).
- Scenario: [M][B] Given the page's prose figures (30 packages; 16 and 14; 4 direct and 26 transitive; 21 MIT and 9 Apache-2.0; 12 bundle a notice file), when a package is added to a lockfile and to the rows but a prose figure is not updated, or a figure is off by one, then an offline test fails naming the figure and both values.
- Scenario: [I] Given a tool PackageReference written with the version as a child element, with `Update=`, with attributes in either order, or in the root Directory.Build.props or Directory.Build.targets, when the tool projects are read, then the check sees each one, and a package the tool gains in any of those forms fails the check.
- Scenario: [S] Given a PropertyGroup whose Condition is false and that holds the two lockfile properties, when the props check runs, then it fails, and the same properties in an unconditioned group pass.
- Scenario: [O] Given the locked-restore test, when the network is missing on a warm cache it is reported as a network miss only when no package was restored, a real lock mismatch is never reported as a network miss, and a shallow checkout reports that the commit-blob check was skipped; and given the first CI run on main after this card (SDK 8.0.x), then the locked restore passes, or the lockfiles are regenerated in the same card follow-up with the reason.
- Scenario: [B] Given the old-prefix guard, when it scans a lockfile, then it skips only `contentHash` lines, H6 and H7 are removed from its allow-list, and an old-prefix hit on any other line of a lockfile fails the guard.

## ZOMBIES coverage

- Z (Zero): covered
- O (One): covered
- M (Many): covered
- B (Boundaries): covered
- I (Interface): covered
- E (Exceptions): covered
- S (Simple): covered

## Dimensions of Difficulty

- Environment: no
- Scope: no
- Unknown: no
- Communication: no

## Links

- Parent epic: none
- Spikes: none
- Related: #369 (the restored-package licence page), card 369 second review, Decider D251 (the toolchain stamp)
