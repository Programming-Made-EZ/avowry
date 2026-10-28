#!/usr/bin/env python3
"""Write matching Phrase and plain C# codebases of 1, 10 or 50 endpoints.

Usage: make-codebase.py SIZE OUT_DIR [--seed N]

The domain is an invented community library desk. The same size and seed write
the same bytes. Output inside a repository is refused unless the path is
git-ignored, using the same check as make-holdout-tasks.py.
"""

from __future__ import annotations

import argparse
import json
import random
import shutil
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any


DESKS = ("north", "south", "east", "west", "central")
ACTIONS = (
    ("PlaceHold", "holds"),
    ("RenewLoan", "renewals"),
    ("RecordReturn", "returns"),
    ("SettleFine", "fines"),
    ("ReserveCopy", "reservations"),
    ("NoteDamage", "damage-notes"),
    ("RequestTransfer", "transfers"),
    ("ExtendLoan", "extensions"),
    ("ShelveCopy", "shelving"),
    ("RecallCopy", "recalls"),
)
TITLES = ("River Town", "Glass Atlas", "Paper Lanterns", "North Wind", "Quiet Stacks")
DAY_CHOICES = (3, 5, 7, 10, 14)
STUDY_SIZES = (1, 10, 50)

HEADER = """\
module Shelf

# Community library desk. An invented lending room: copies, loans and one shelf.
"""

TYPES = """\
type Copy
  copyId: Id
  title: Text

type Loan
  book: Copy
  days: Int

type Shelf
  name: Text
  copies: List<Copy>
"""

LEAVES = """\
phrase "ensure {book: Copy} has a title"
  fails with MissingTitle
  spec "rejects a blank title"
    given book = { copyId: "c-1", title: "" }
    when called
    then fails with MissingTitle
  native csharp
    if (string.IsNullOrWhiteSpace(book.Title)) Fail("MissingTitle");

phrase "ensure {days: Int} is a loan of at most three weeks"
  fails with LoanTooLong
  spec "rejects a loan past three weeks"
    given days = 22
    when called
    then fails with LoanTooLong
  native csharp
    if (days > 21) Fail("LoanTooLong");

phrase "the day count of {days: Int}" -> Int
  spec "returns the days"
    given days = 7
    when called
    then returns 7
  native csharp
    return days;

phrase "count copies on {shelf: Shelf}" -> Int
  spec "counts the copies"
    given shelf = { name: "North", copies: [{ copyId: "c-1", title: "Elm" }, { copyId: "c-2", title: "Ash" }] }
    when called
    then returns 2
  native csharp
    return shelf.Copies.Count;
"""

API_PROJECT = """\
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Shelf</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FastEndpoints" Version="8.3.0" />
  </ItemGroup>
</Project>
"""

SPECS_PROJECT = """\
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <RootNamespace>Shelf.Specs</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../Shelf.Api/Shelf.Api.csproj" />
  </ItemGroup>
</Project>
"""

PROGRAM = """\
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();
app.Run();

public partial class Program;
"""

RUNTIME = """\
using System.Diagnostics.CodeAnalysis;

namespace Shelf;

/// <summary>Links a generated symbol back to the phrase or endpoint it came from.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PhraseAttribute(string template) : Attribute
{
    public string Template { get; } = template;
}

/// <summary>A declared failure code raised by a phrase (fails with Code).</summary>
public sealed class PhraseFailure(string code) : Exception(code)
{
    public string Code { get; } = code;

    [DoesNotReturn]
    public static void Raise(string code) => throw new PhraseFailure(code);
}
"""

TYPES_CS = """\
namespace Shelf;

public sealed record Copy(string CopyId, string Title);

public sealed record Loan(Copy Book, int Days);

public sealed record Shelf(string Name, IReadOnlyList<Copy> Copies);
"""

PHRASES_CS = """\
namespace Shelf;

public static class Phrases
{
    public static void EnsureBookHasATitle(Copy book)
    {
        if (string.IsNullOrWhiteSpace(book.Title)) PhraseFailure.Raise("MissingTitle");
    }

    public static void EnsureDaysIsALoanOfAtMostThreeWeeks(int days)
    {
        if (days > 21) PhraseFailure.Raise("LoanTooLong");
    }

    public static int TheDayCountOfDays(int days)
    {
        return days;
    }

    public static int CountCopiesOnShelf(Shelf shelf)
    {
        return shelf.Copies.Count;
    }
}
"""

PHRASE_SPECS = """\
using Shelf;

namespace Shelf.Specs;

public class PhraseSpecs
{
    [Fact]
    public void Ensure_book_rejects_a_blank_title()
    {
        var failure = Assert.Throws<PhraseFailure>(() => Phrases.EnsureBookHasATitle(new Copy(CopyId: "c-1", Title: "")));
        Assert.Equal("MissingTitle", failure.Code);
    }

    [Fact]
    public void Ensure_days_rejects_a_loan_past_three_weeks()
    {
        var failure = Assert.Throws<PhraseFailure>(() => Phrases.EnsureDaysIsALoanOfAtMostThreeWeeks(22));
        Assert.Equal("LoanTooLong", failure.Code);
    }

    [Fact]
    public void The_day_count_returns_the_days()
    {
        Assert.Equal(7, Phrases.TheDayCountOfDays(7));
    }

    [Fact]
    public void Count_copies_counts_the_copies()
    {
        var shelf = new Shelf(
            Name: "North",
            Copies: new Copy[]
            {
                new Copy(CopyId: "c-1", Title: "Elm"),
                new Copy(CopyId: "c-2", Title: "Ash"),
            });
        Assert.Equal(2, Phrases.CountCopiesOnShelf(shelf));
    }
}
"""

STANDALONE_PROPS = """\
<Project>
  <!-- Stops inheritance of a parent Directory.Build.props. This folder is a standalone codebase. -->
</Project>
"""


@dataclass(frozen=True)
class Sample:
    title: str
    days: int
    copy_id: str


@dataclass(frozen=True)
class Endpoint:
    name: str
    route: str
    posts_limit: bool


def git_root() -> Path | None:
    git = shutil.which("git")
    if git is None:
        return None
    result = subprocess.run(
        [git, "rev-parse", "--show-toplevel"],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        return None
    root = result.stdout.strip()
    return Path(root).resolve() if root else None


def is_path_in_repo(path: Path, repo_root: Path | None) -> bool:
    if repo_root is None:
        return False
    try:
        path.resolve().relative_to(repo_root)
        return True
    except ValueError:
        return False


def refuse_if_in_repo_and_unignored(out_dir: Path) -> None:
    repo_root = git_root()
    if not is_path_in_repo(out_dir, repo_root):
        return
    git = shutil.which("git")
    if git is None:
        raise RuntimeError("refusing to write: repository path is not git-ignored and git unavailable")
    probe = out_dir / "__holdout_probe__"
    result = subprocess.run(
        [git, "-C", str(repo_root), "check-ignore", "--quiet", "--no-index", str(probe)],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode == 0:
        return
    if result.returncode in (1, 2, 128):
        raise RuntimeError("refusing to write: output path is inside the repository and not git-ignored")
    raise RuntimeError("refusing to write: failed to verify output path safety")


def sample_for(seed: int) -> Sample:
    rng = random.Random(seed)
    return Sample(
        title=rng.choice(TITLES),
        days=rng.choice(DAY_CHOICES),
        copy_id=f"c-{rng.randrange(1000, 10000)}",
    )


def study_endpoints(size: int) -> list[Endpoint]:
    found: list[Endpoint] = []
    for desk in DESKS:
        desk_name = desk[:1].upper() + desk[1:]
        for action, slug in ACTIONS:
            name = f"{action}{desk_name}"
            found.append(Endpoint(name=name, route=f"/desks/{desk}/{slug}", posts_limit=name == "PlaceHoldNorth"))
    return found[:size]


def phrase_input(sample: Sample, days: int) -> str:
    return (
        "{ loan: { book: { copyId: "
        + json.dumps(sample.copy_id)
        + ", title: "
        + json.dumps(sample.title)
        + " }, days: "
        + str(days)
        + " } }"
    )


def phrase_endpoint(ep: Endpoint, sample: Sample, call_loan: bool) -> str:
    lines = []
    if ep.posts_limit:
        lines.append("# The north desk posts its loan limit as 14 days.")
    lines.append(f"endpoint {ep.name} : POST {ep.route}")
    lines.append("  input")
    lines.append("    loan: Loan")
    lines.append("  output")
    lines.append("    days: Int")
    lines.append("  steps")
    lines.append("    ensure input.loan.book has a title")
    if call_loan:
        lines.append("    ensure input.loan.days is a loan of at most three weeks")
    lines.append("    let counted = the day count of input.loan.days")
    lines.append("    respond with { days: counted }")
    lines.append("")
    lines.append('  spec "accepts a short loan"')
    lines.append(f"    given input {phrase_input(sample, sample.days)}")
    lines.append("    when handled")
    lines.append(f"    then responds {{ days: {sample.days} }}")
    lines.append("")
    lines.append('  spec "rejects a loan past three weeks"')
    lines.append(f"    given input {phrase_input(sample, 22)}")
    lines.append("    when handled")
    lines.append("    then fails with LoanTooLong")
    return "\n".join(lines)


def csharp_loan(sample: Sample, days: int) -> str:
    title = json.dumps(sample.title)
    copy_id = json.dumps(sample.copy_id)
    return f"new Loan(Book: new Copy(CopyId: {copy_id}, Title: {title}), Days: {days})"


def csharp_endpoint(ep: Endpoint, sample: Sample, call_loan: bool) -> str:
    lines = [
        "using FastEndpoints;",
        "",
        "namespace Shelf;",
        "",
    ]
    if ep.posts_limit:
        lines.append("// The north desk posts its loan limit as 14 days.")
        lines.append("")
    lines.extend(
        [
            f"public sealed record {ep.name}Request(Loan Loan);",
            "",
            f"public sealed record {ep.name}Response(int Days);",
            "",
            f"public static class {ep.name}Handler",
            "{",
            f"    public static {ep.name}Response Handle({ep.name}Request input)",
            "    {",
            "        Phrases.EnsureBookHasATitle(input.Loan.Book);",
        ]
    )
    if call_loan:
        lines.append("        Phrases.EnsureDaysIsALoanOfAtMostThreeWeeks(input.Loan.Days);")
    lines.extend(
        [
            "        var counted = Phrases.TheDayCountOfDays(input.Loan.Days);",
            f"        return new {ep.name}Response(Days: counted);",
            "    }",
            "}",
            "",
            f"public sealed class {ep.name}Endpoint : Endpoint<{ep.name}Request, {ep.name}Response>",
            "{",
            "    public override void Configure()",
            "    {",
            f'        Post("{ep.route}");',
            "        AllowAnonymous();",
            "    }",
            "",
            f"    public override async Task HandleAsync({ep.name}Request req, CancellationToken ct)",
            "    {",
            "        try",
            "        {",
            f"            await Send.OkAsync({ep.name}Handler.Handle(req), ct);",
            "        }",
            "        catch (PhraseFailure failure)",
            "        {",
            "            AddError(failure.Code);",
            "            await Send.ErrorsAsync(422, ct);",
            "        }",
            "    }",
            "}",
        ]
    )
    return "\n".join(lines)


def csharp_specs(ep: Endpoint, sample: Sample) -> str:
    ok = csharp_loan(sample, sample.days)
    bad = csharp_loan(sample, 22)
    return "\n".join(
        [
            "using Shelf;",
            "",
            "namespace Shelf.Specs;",
            "",
            f"public class {ep.name}Specs",
            "{",
            "    [Fact]",
            "    public void Accepts_a_short_loan()",
            "    {",
            f"        var input = new {ep.name}Request(Loan: {ok});",
            f"        var response = {ep.name}Handler.Handle(input);",
            f"        Assert.Equal(new {ep.name}Response(Days: {sample.days}), response);",
            "    }",
            "",
            "    [Fact]",
            "    public void Rejects_a_loan_past_three_weeks()",
            "    {",
            f"        var input = new {ep.name}Request(Loan: {bad});",
            f"        var failure = Assert.Throws<PhraseFailure>(() => {ep.name}Handler.Handle(input));",
            '        Assert.Equal("LoanTooLong", failure.Code);',
            "    }",
            "}",
        ]
    )


def dump_outcomes(cases: list[dict[str, Any]]) -> str:
    return json.dumps(cases, ensure_ascii=False, indent=2) + "\n"


def modify_outcomes() -> list[dict[str, Any]]:
    return [
        {
            "name": "accepts the posted desk limit",
            "given": {"days": 14},
            "when": "called",
            "then": {"succeeds": True},
        },
        {
            "name": "rejects a day past the posted desk limit",
            "given": {"days": 15},
            "when": "called",
            "then": {"fails_with": "LoanTooLong"},
        },
    ]


def fix_outcomes(sample: Sample) -> list[dict[str, Any]]:
    return [
        {
            "name": "accepts a short loan",
            "given": {
                "loan": {
                    "book": {"copyId": sample.copy_id, "title": sample.title},
                    "days": sample.days,
                }
            },
            "when": "handled",
            "then": {"responds": {"days": sample.days}, "status": 200},
        },
        {
            "name": "rejects a loan past three weeks",
            "given": {
                "loan": {
                    "book": {"copyId": sample.copy_id, "title": sample.title},
                    "days": 22,
                }
            },
            "when": "handled",
            "then": {"fails_with": "LoanTooLong"},
        },
    ]


def task_prompt(kind: str, changed_phrase: str, needs_phrase: str, changed_csharp: str, needs_csharp: str, prose: str, outcomes: str, phrase_start: str, csharp_start: str) -> str:
    return "\n".join(
        [
            prose.strip(),
            "",
            f"Kind: {kind}",
            "",
            f"Changed phrase file: {changed_phrase}",
            f"Needs phrase file: {needs_phrase}",
            f"Changed csharp file: {changed_csharp}",
            f"Needs csharp file: {needs_csharp}",
            "",
            "Change only the changed file. Read the needs file before editing; do not change it.",
            "The starting code of the changed file is the same size class in both arms.",
            "",
            "Outcomes:",
            outcomes.strip("\n"),
            "",
            "## Starting phrase file",
            "```phrase",
            phrase_start.strip("\n"),
            "```",
            "",
            "## Starting C# file",
            "```csharp",
            csharp_start.strip("\n"),
            "```",
            "",
        ]
    )


def finished(text: str) -> str:
    return text.replace("\r\n", "\n").strip("\n") + "\n"


def build_files(size: int, seed: int) -> dict[str, str]:
    sample = sample_for(seed)
    endpoints = study_endpoints(size)
    place = endpoints[0]
    if place.name != "PlaceHoldNorth":
        raise RuntimeError("PlaceHoldNorth must be the first endpoint")

    endpoint_phrase = {ep.name: phrase_endpoint(ep, sample, call_loan=True) for ep in endpoints}
    endpoint_csharp = {ep.name: csharp_endpoint(ep, sample, call_loan=True) for ep in endpoints}
    shelf = "\n\n".join(
        [finished(HEADER).strip("\n"), finished(TYPES).strip("\n"), finished(LEAVES).strip("\n")]
        + [endpoint_phrase[ep.name] for ep in endpoints]
    )

    files: dict[str, str] = {
        "phrase/shelf.phrase": finished(shelf),
        "phrase/leaves.phrase": finished(LEAVES),
        "csharp/Directory.Build.props": finished(STANDALONE_PROPS),
        "csharp/Shelf.Api/Shelf.Api.csproj": finished(API_PROJECT),
        "csharp/Shelf.Api/Program.cs": finished(PROGRAM),
        "csharp/Shelf.Api/PhraseRuntime.cs": finished(RUNTIME),
        "csharp/Shelf.Api/Types.cs": finished(TYPES_CS),
        "csharp/Shelf.Api/Phrases.cs": finished(PHRASES_CS),
        "csharp/Shelf.Specs/Shelf.Specs.csproj": finished(SPECS_PROJECT),
        "csharp/Shelf.Specs/PhraseSpecs.cs": finished(PHRASE_SPECS),
    }
    for ep in endpoints:
        files[f"phrase/endpoints/{ep.name}.phrase"] = finished(endpoint_phrase[ep.name])
        files[f"csharp/Shelf.Api/{ep.name}.cs"] = finished(endpoint_csharp[ep.name])
        files[f"csharp/Shelf.Specs/{ep.name}Specs.cs"] = finished(csharp_specs(ep, sample))

    modify_phrase = files["phrase/leaves.phrase"]
    modify_csharp = files["csharp/Shelf.Api/Phrases.cs"]
    modify_body = dump_outcomes(modify_outcomes())
    modify_prompt = task_prompt(
        "modify",
        "phrase/leaves.phrase",
        "phrase/endpoints/PlaceHoldNorth.phrase",
        "csharp/Shelf.Api/Phrases.cs",
        "csharp/Shelf.Api/PlaceHoldNorth.cs",
        "Modify the shared loan-length rule so it enforces the limit the north desk posts. "
        "The number is not in this prompt. It is written in the endpoint file this task names.",
        modify_body,
        modify_phrase,
        modify_csharp,
    )
    files["tasks/modify-loan-limit/prompt.md"] = finished(modify_prompt)
    files["tasks/modify-loan-limit/outcomes.json"] = modify_body
    files["tasks/modify-loan-limit/phrase/start.phrase"] = modify_phrase
    files["tasks/modify-loan-limit/csharp/start.cs"] = modify_csharp

    fix_phrase = finished(phrase_endpoint(place, sample, call_loan=False))
    fix_csharp = finished(csharp_endpoint(place, sample, call_loan=False))
    fix_body = dump_outcomes(fix_outcomes(sample))
    fix_prompt = task_prompt(
        "fix",
        "phrase/endpoints/PlaceHoldNorth.phrase",
        "phrase/leaves.phrase",
        "csharp/Shelf.Api/PlaceHoldNorth.cs",
        "csharp/Shelf.Api/Phrases.cs",
        "Fix PlaceHoldNorth so a long loan fails with the code the shared leaf declares. "
        "The endpoint file no longer calls that leaf. The wording to restore is not in this prompt. "
        "It is declared in the leaf file this task names.",
        fix_body,
        fix_phrase,
        fix_csharp,
    )
    files["tasks/fix-place-hold/prompt.md"] = finished(fix_prompt)
    files["tasks/fix-place-hold/outcomes.json"] = fix_body
    files["tasks/fix-place-hold/phrase/start.phrase"] = fix_phrase
    files["tasks/fix-place-hold/csharp/start.cs"] = fix_csharp
    return files


def write_tree(out_dir: Path, files: dict[str, str]) -> None:
    for relative in sorted(files):
        text = files[relative]
        if "\r" in text:
            raise RuntimeError(f"refusing to write CR bytes in {relative}")
        path = out_dir.joinpath(*relative.split("/"))
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(text.encode("utf-8"))


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Write matching Phrase and plain C# codebases.")
    parser.add_argument("size", type=int)
    parser.add_argument("out_dir")
    parser.add_argument("--seed", type=int, default=1)
    args = parser.parse_args(argv)
    try:
        if args.size not in STUDY_SIZES:
            raise ValueError("size must be 1, 10 or 50")
        out_dir = Path(args.out_dir).resolve()
        refuse_if_in_repo_and_unignored(out_dir)
        write_tree(out_dir, build_files(args.size, args.seed))
        print(f"wrote {args.size} endpoints, seed {args.seed}, to {out_dir}")
        return 0
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
