## 1. What I checked, and what I found

The card is broadly right, but it starts from a wrong assumption. The Avowry tool itself restores no NuGet packages. Every package Dana would restore comes from the C# projects the compiler generates for her. The repository has no lockfile, so a test cannot read that list offline today.

- **The page today (`docs/release/pre-publication-checks.md`):** The Package licences section lists the four direct packages: FastEndpoints 8.3.0 (MIT), Microsoft.NET.Test.Sdk 17.11.1 (MIT), xunit 2.9.2 (Apache-2.0) and xunit.runner.visualstudio 2.8.2 (Apache-2.0). It says "Shipped with the compiler and CLI: none". For transitive packages it says "not reviewed: needs a restore" and points to this card.
- **The shipped tool:** `src/Avowry.Cli/Avowry.Cli.csproj` is the dotnet tool (package id Avowry, version 0.1.0). It references only `Avowry.Compiler` and `Avowry.Adapters.CSharp`, and neither has a PackageReference. Its local restore output, `src/Avowry.Cli/obj/project.assets.json`, lists only those two project references and no packages. Besides them it records only the .NET 8 targeting packs that come with the SDK.
- **What Dana actually restores:** `CSharpProvider.cs:237` writes FastEndpoints 8.3.0 into the generated API project. Lines 251 to 253 write the three test packages into the generated specs project. The projects under `reference/csharp/` (`Orders.Api`, `Orders.Specs`) are hand-written copies of that generated output, with the same references. `ReferenceTarget.cs` builds and tests them in a temporary folder.
- **No lockfile and no central package management:** I found no `packages.lock.json` and no `Directory.Packages.props`. The only package-related setting anywhere is in the root `Directory.Build.props`, which sets net8.0 and does not enable lockfiles. `reference/Directory.Build.props` deliberately stops the root settings from applying to the reference projects.
- **Restore output never covers the generated API project:** No `project.assets.json` in the repository mentions FastEndpoints or FluentValidation. The restore output that does exist is git-ignored (`obj/` in `.gitignore`) and contains absolute local paths. That makes it unsuitable to commit, which matters for card 370.
- **The test-only closure is known:** `tests/Avowry.Acceptance.Tests/obj/project.assets.json` lists 14 packages:
  - Microsoft.CodeCoverage, Microsoft.NET.Test.Sdk, Microsoft.TestPlatform.ObjectModel and Microsoft.TestPlatform.TestHost (all 17.11.1)
  - Newtonsoft.Json 13.0.1 and System.Reflection.Metadata 1.6.0
  - xunit, xunit.assert, xunit.core, xunit.extensibility.core and xunit.extensibility.execution (all 2.9.2), plus xunit.abstractions 2.0.3, xunit.analyzers 1.16.0 and xunit.runner.visualstudio 2.8.2

  The generated specs project has the same three direct references, so it restores the same closure.
- **The runtime closure from the NuGet cache (`~/.nuget/packages`):**
  - FastEndpoints 8.3.0 depends on FastEndpoints.Attributes, .JobQueues and .Messaging (all 8.3.0) and on FluentValidation 12.1.1.
  - Those pull in FastEndpoints.Core and FastEndpoints.Messaging.Core (8.3.0) and several Microsoft.Extensions.*.Abstractions and Primitives packages at 10.0.11. The cache holds eight of those.
  - Licences: all six FastEndpoints packages are MIT, FluentValidation is Apache-2.0, and the Microsoft.Extensions packages I sampled are MIT.

  I could not run a restore in this read-only session, so the exact runtime closure is not confirmed.
- **Real cases with no licence expression:** System.Reflection.Metadata 1.6.0 and xunit.abstractions 2.0.3 have no `<license>` element, only a `licenseUrl`. The first also has `LICENSE.TXT` in its cache folder. The page's current method, reading the `<license>` element, gives nothing for these two.
- **Licence-acceptance flag:** Microsoft.CodeCoverage 17.11.1 is MIT but sets `requireLicenseAcceptance` to true.
- **The cache is not a list:** It also holds the FastEndpoints 7.2.0 family. The cache can supply licences but cannot say which packages are restored.
- **The existing tests:**
  - `tests/test_pre_publication_checks.py` requires the Package licences table to match the tracked PackageReferences exactly, and requires the words "not reviewed" in that section.
  - If transitive rows are added to that table, both checks fail. The second check also becomes wrong once the transitive packages are reviewed.
  - `sect()` stops only at the next `## ` heading, so a `###` subsection would still count as part of the table.
  - There must be one `Recommendation:` line per `##` section.
  - `tests/Avowry.Acceptance.Tests/PrePublicationChecks.cs` runs that Python module from `dotnet test`, so the Done-when command already covers it.
- **The Done-when filter:** A `Status` trait exists in `UsableV1.cs`, `UsableV1Closure.cs` and `PatternDocs.cs`, so `--filter "Status!=Pending"` is meaningful.

**Where a test should read the restore list from:** There is no lockfile today. A lockfile is still the narrowest reliable source, but it should be committed for the two reference projects, not the solution. Turn it on in `reference/Directory.Build.props` and commit:

- `reference/csharp/Orders.Api/packages.lock.json`
- `reference/csharp/Orders.Specs/packages.lock.json`

A lockfile lists every package the project restores, each marked Direct or Transitive, with its exact version and a content hash. It contains no local paths and is plain JSON, so the Python test can read it without network access.

The tool needs no lockfile. Its "no packages" case can be checked from the tracked project files of `Avowry.Cli` and the two projects it references.

## 2. Revised card

```markdown
# The licences of every package an Avowry user's generated service and specs restore are reviewed and listed

- **Status:** Draft
- **Shape:** Story
- **Type:** story
- **Repository:** avowry (docs/release; reference/csharp; tests)
- **Source:** product-owner answers to the nine owner items on docs/release/pre-publication-checks.md, inbox entry of 2026-11-08 (card 186): "restore-and-review". Ranked before the v0.1 publish card.
- **Depends on:** none
- **Story:** As Dana, I want every package that installing Avowry and building its generated C# service and specs restores, direct or transitive, listed with its exact version and licence, so that before I adopt the preview I can see whether anything it pulls into my build needs a licence review.
- **Consumer value:** On one page Dana sees that the Avowry tool restores no packages, and sees every package her generated service and specs restore, with version, licence, where it is used, and which packages need a closer look.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Scenario: [O][M][I][S] every package the generated projects restore is listed
     Given committed lockfiles for the reference API project and the reference specs project, whose direct packages equal the PackageReferences the C# provider writes
     When Dana reads the restored-package licences on docs/release/pre-publication-checks.md
     Then each package in either lockfile appears once with its exact resolved version, direct or transitive, its licence, where the licence was read (nuspec licence expression, packaged licence file, or licence URL), and where it is used (generated service, or generated specs, test-only); and the section names the two lockfiles, the restore command and the commit they came from
  2. Scenario: [Z][B] the empty and borderline cases are stated, not skipped
     Given the Avowry tool project and its two referenced projects, and the packages whose nuspec has no licence expression (today System.Reflection.Metadata 1.6.0 and xunit.abstractions 2.0.3) or asks for licence acceptance (today Microsoft.CodeCoverage 17.11.1)
     When Dana reads the page
     Then it says the Avowry tool restores no packages; it names each package without a licence expression, with the licence read from its packaged file or URL and a recommendation; it names any package whose licence is outside MIT, Apache-2.0, BSD-2-Clause or BSD-3-Clause with a recommendation, or says in one line that none is; and it no longer says the transitive packages are not reviewed
  3. Scenario: [E][I] a stale list fails without network access
     Given the page and the two committed lockfiles, read with no network access
     When `dotnet test Avowry.sln --filter "Status!=Pending"` runs
     Then it fails, naming the package, when a lockfile package is missing from the page, is listed at a different version, a page row is in neither lockfile, a lockfile's direct packages differ from those the C# provider writes, or the tool's projects gain a PackageReference; and the reference target build fails in locked mode when a reference project and its lockfile disagree
- **ZOMBIES:** Z the tool restores no packages (2); O one package's row (1); M the full closure of both generated projects (1); B packages with no licence expression or with licence acceptance required, and an exact version rather than another version from the cache (2, 3); I the lockfile format compared with the page's table, and the provider's PackageReferences compared with the lockfile's direct entries (1, 3); E a missing, extra or wrong-version row, or drift between the provider, a lockfile and the reference projects (3); S every row carries a licence and a where-used value (1).
- **Not in this card:** Changing any dependency or version; making the C# provider write lockfiles into Dana's generated projects; lockfiles for the repository's own test, bench and fixture projects (not shipped; their packages are the same test closure); the .NET targeting packs that come with the SDK; package metadata and the packed NOTICE (publish card); home paths (card 370); legal advice.
- **Size:** M

## Context

- Filed by the facilitator on 2026-11-08 UTC from the product owner's inbox entry on the nine owner items.
- Today: the page lists the four direct packages and says "Transitive packages: not reviewed: needs a restore". There is no packages.lock.json and no central package management. Restore output (obj/project.assets.json) is git-ignored and holds local paths. No restore output in the repo covers FastEndpoints.
- The Avowry tool (src/Avowry.Cli, PackAsTool, id Avowry 0.1.0) and its two referenced projects have no PackageReference; its local restore output lists only the two project references.
- Known closure: the test packages restore 14 packages (from the acceptance tests' restore output). FastEndpoints 8.3.0 brings FastEndpoints.Attributes, Core, JobQueues, Messaging and Messaging.Core (all 8.3.0, MIT), FluentValidation 12.1.1 (Apache-2.0) and Microsoft.Extensions.* 10.0.11 packages; the exact list comes from the first locked restore.
- The current check in tests/test_pre_publication_checks.py requires the licences table to match the tracked PackageReferences exactly and requires "not reviewed" in the section. Both change in this card: either a new `##` section with its own Recommendation line, or a test that splits direct rows from restored rows.

## Done when

    dotnet test Avowry.sln --filter "Status!=Pending"
```

## 3. Open points for the facilitator

- Approve committing `packages.lock.json` for the two `reference/csharp` projects, turned on in `reference/Directory.Build.props`, as the test's source.
- Approve switching the reference target test to locked restore so that a lockfile out of date with its project fails the build.
- Choose the page layout: a new `## Restored package licences` section, or one table with a Direct/Transitive column and the existing test reworked.
- Confirm the allowed-licence list (MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause); anything else is flagged "needs review".
- Confirm that packages with only a licence URL may take their licence from the packaged file or the URL, labelled as such.
- Decide whether Microsoft.CodeCoverage's licence-acceptance flag gets a recommendation line.
- Confirm that the .NET targeting packs and the repository's own test and bench projects are out of scope.
- Decide whether a later card makes the C# provider write lockfiles into Dana's generated projects.
- Confirm Dana as the persona and the reworded "so that", which avoids implying her service is Apache-2.0.
- Size stays M if a single restore settles the exact Microsoft.Extensions.* closure; I could not run one in this session.
