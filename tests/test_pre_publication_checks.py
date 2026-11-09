"""Card 186 (D157 Criteria 1 to 3): the pre-publication findings page matches the repo."""

import importlib.util, os, re, subprocess, sys, tempfile, unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
PAGE = REPO / "docs" / "release" / "pre-publication-checks.md"
SCAN = REPO / "scripts" / "scan-history-for-secrets.py"
CHECKER = REPO / "scripts" / "check-no-personal-names.py"
HEADER = "docs/release/pre-publication-checks.md must match the repo.\n"
PKG_RE = re.compile(r'PackageReference Include=\\?"([^"\\]+)\\?" Version=\\?"([^"\\]+)\\?"')
MODEL_RE = re.compile(r'"model"\s*:\s*"([^"]+)"')
HASH_RE = re.compile(r"\b[a-f0-9]{40}\b")
EMAIL_RE = re.compile(r"\S+@\S+\.\w+")
ASSIGN_RE = re.compile(r"(?i)\b(?:api[_-]?key|secret|token|passwd|password|authorization|access[_-]?key)\b\s*[:=]\s*\S+")
WHERE, LICS = ("shipped", "generated", "test-only"), ("MIT", "Apache-2.0")
BLOCKED = (
    "package-licences", "vendor-terms", "secrets-scan", "author-metadata",
    "name-collisions", "owner-items", "recommendations",
)
HEADINGS = {
    "package-licences": "Package licences", "vendor-terms": "Vendor terms",
    "secrets-scan": "Secrets scan", "author-metadata": "Author metadata",
    "personal-names": "Personal-name check", "owner-items": "Owner items",
    "name-collisions": "Name collisions (not a professional search)",
}
TARGETS = ("NuGet", "npm", "PyPI", "crates.io", "GitHub", "avowry.dev", "avowry.io")
SPEC = importlib.util.spec_from_file_location("check_no_personal_names", CHECKER)
check = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(check)
H40 = "a" * 40
COMPLIANT = "\n".join([
    "# Pre-publication checks", f"HEAD `{H40}`",
    "## Package licences", f"Command: git grep. HEAD {H40}",
    "| id | version | licence | source | where |", "| --- | --- | --- | --- | --- |",
    "| FastEndpoints | 8.3.0 | MIT | nuspec | generated |",
    "| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | nuspec | test-only |",
    "| xunit | 2.9.2 | Apache-2.0 | nuspec | test-only |",
    "| xunit.runner.visualstudio | 2.8.2 | Apache-2.0 | nuspec | test-only |",
    "transitive: not reviewed", "Recommendation: proceed.",
    "## Vendor terms", f"Command: list. HEAD {H40}",
    "| model | vendor | published | terms |", "| --- | --- | --- | --- |",
    "| grok-4.7 | xAI (inferred) | reply_head | not reviewed |",
    "| copilot-default | GitHub (inferred) | reply_head | not reviewed |",
    "| codex-gpt-5.6-sol | OpenAI (inferred) | reply_head | not reviewed |",
    "| agy-gemini-4-argon-medium | Google (inferred) | reply_head | not reviewed |",
    "Recommendation: hold.",
    "## Secrets scan", f"Command: python scripts/scan-history-for-secrets.py {H40}",
    "Count 0. Dispositions: 0.", "Recommendation: none.",
    "## Author metadata", f"Command: git log. HEAD {H40}",
    "distinct author identities 1", "committer identities 1", "combined 1",
    "the repository-local identity; every commit carries it", "Recommendation: ask.",
    "## Personal-name check",
    f"Command: python scripts/check-no-personal-names.py. HEAD {H40}", "Recommendation: keep.",
    "## Name collisions (not a professional search)",
    *[f"{t}: free on 2026-10-29, source D125" for t in TARGETS], "Recommendation: hold.",
    "## Owner items",
    *[f"- {x}: OPEN, asked 2026-11-08" for x in (
        "public name", "holder line", "professional trademark search",
        "four lawyer questions", "author identity going forward", "unread vendor terms")],
    "Recommendation: ask.", "",
])


def sh(args, cwd=REPO, env=None):
    try:
        return subprocess.run(args, cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace", env=env)
    except OSError:
        return None


def git(args, cwd=REPO):
    return sh(["git", *args], cwd)


def rows(body):
    out = []
    for line in body.splitlines():
        if not line.startswith("|"):
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if cells and cells[0] and set(cells[0]) - set("-:") and cells[0].lower() not in {"id", "package", "name", "model"}:
            out.append(cells)
    return out


def sect(text, title):
    needle = "## " + title + "\n"
    at = text.find(needle)
    if at < 0:
        return None
    nxt = text.find("\n## ", at + len(needle))
    return text[at + len(needle):] if nxt < 0 else text[at + len(needle):nxt]


def counts_of(cwd, rev):
    def n(fmt):
        r = git(["log", rev, "--format=" + fmt], cwd)
        return None if r is None or r.returncode else len({x for x in r.stdout.splitlines() if x})
    vals = (n("%an|%ae"), n("%cn|%ce"), n("%an|%ae|%cn|%ce"))
    return None if any(v is None for v in vals) else vals


def live_ctx():
    ctx = {"forbidden_reasons": check.forbidden_reasons}
    ran = sh([sys.executable, str(CHECKER)])
    ctx["personal_names_exit"] = None if ran is None else ran.returncode
    if not SCAN.is_file():
        ctx["scan_script"] = "scripts/scan-history-for-secrets.py is missing"
    else:
        t = sh([sys.executable, str(SCAN), "--self-test"])
        ctx["scan_script"] = "ok" if t is not None and t.returncode == 0 else "self-test exit " + str(None if t is None else t.returncode)
    shallow = git(["rev-parse", "--is-shallow-repository"])
    ctx["is_shallow"] = bool(shallow and shallow.stdout.strip() == "true")
    if not PAGE.is_file():
        return ctx
    ident = git(["log", "-1", "--format=%an%n%ae"])
    lines = ident.stdout.splitlines() if ident else []
    ctx["identity_name"] = lines[0] if lines else ""
    ctx["identity_email"] = lines[1] if len(lines) > 1 else ""
    found, ls = set(), git(["ls-files", "*.csproj", "*.props", "*.targets"])
    if ls:
        for rel in ls.stdout.splitlines():
            found.update(PKG_RE.findall((REPO / rel).read_text(encoding="utf-8")))
    csharp = REPO / "src" / "Avowry.Adapters.CSharp" / "CSharpProvider.cs"
    if csharp.is_file():
        found.update(PKG_RE.findall(csharp.read_text(encoding="utf-8")))
    ctx["packages"] = found
    models, allf = [], git(["ls-files", "-z"])
    if allf:
        for raw in allf.stdout.split("\0"):
            if raw.startswith("bench/results/") and raw.endswith(".json") and "/" not in raw[14:]:
                m = MODEL_RE.search((REPO / raw).read_text(encoding="utf-8"))
                if m:
                    models.append(m.group(1))
    ctx["models"] = models
    ctx["skip_recompute"] = ctx["is_shallow"]
    return ctx


def collect_failures(page, ctx):
    failures = []

    def fail(cid, msg):
        failures.append(f"{cid}: {msg}")

    if page is None:
        fail("page", "docs/release/pre-publication-checks.md is missing")
        for cid in BLOCKED:
            fail(cid, "blocked: page missing")
    else:
        if ctx.get("is_shallow"):
            print("shallow: recompute skipped")
        found = set(re.findall(r"(?m)^## (.+)$", page))
        for cid, title in HEADINGS.items():
            if title not in found:
                fail(cid, "heading missing")
                continue
            body = sect(page, title) or ""
            if cid == "package-licences":
                pkgs = ctx.get("packages")
                if pkgs is not None:
                    got = {(r[0], r[1]) for r in rows(body) if len(r) >= 2}
                    if got != set(pkgs):
                        fail(cid, "table does not match tracked PackageReference ids")
                    elif any(
                        not any(w in " ".join(r) for w in LICS) or not any(w in " ".join(r) for w in WHERE)
                        for r in rows(body)
                    ):
                        fail(cid, "row missing licence or where-used word")
                    elif "not reviewed" not in body:
                        fail(cid, "transitive line does not say not reviewed")
            elif cid == "vendor-terms":
                models = ctx.get("models")
                if models is not None:
                    names = {r[0] for r in rows(body) if r}
                    if names != set(models):
                        fail(cid, "rows do not match tracked bench/results json models")
                    else:
                        for model in models:
                            line = next((ln for ln in body.splitlines() if model in ln), "")
                            if "reply_head" not in line or "not reviewed" not in line:
                                fail(cid, "row missing reply_head or terms status")
                                break
                            if "inferred" not in line and "vendor" not in line.lower():
                                fail(cid, "row missing vendor")
                                break
            elif cid == "secrets-scan":
                hashes = HASH_RE.findall(body)
                if "scan-history-for-secrets.py" not in body:
                    fail(cid, "script name missing")
                elif not hashes:
                    fail(cid, "no 40-character hash")
                elif "@" in body:
                    fail(cid, "section contains @")
                elif ASSIGN_RE.search(body):
                    fail(cid, "section contains an assignment")
                elif re.search(r"\b[A-Fa-f0-9]{41,}\b", body):
                    fail(cid, "section contains a long hex string")
                elif not re.search(r"(?i)count\s+(\d+)", body) and not re.search(r"\b0\b", body):
                    fail(cid, "no count")
                elif not ctx.get("is_shallow") and not ctx.get("skip_recompute") and SCAN.is_file():
                    cm = re.search(r"(?i)count\s+(\d+)", body)
                    n = int(cm.group(1)) if cm else 0
                    ran = sh([sys.executable, str(SCAN), hashes[0]], cwd=ctx.get("git_cwd", REPO))
                    text = ((ran.stdout or "") + (ran.stderr or "")) if ran else ""
                    m = re.search(r"(?i)\bcount\s*[:=]?\s*(\d+)", text)
                    lines = [ln.strip() for ln in text.splitlines() if ln.strip()]
                    got = int(m.group(1)) if m else (int(lines[-1]) if lines and lines[-1].isdigit() else len(lines))
                    if n != got:
                        fail(cid, "count does not match the recompute")
            elif cid == "author-metadata":
                name, email = ctx.get("identity_name") or "", ctx.get("identity_email") or ""
                if EMAIL_RE.search(page) or (name and name in page) or (email and email in page):
                    fail(cid, "the owner identity is in the page")
                else:
                    nums = ctx.get("author_counts")
                    if nums is None and not ctx.get("is_shallow") and not ctx.get("skip_recompute"):
                        hs = HASH_RE.findall(body)
                        nums = counts_of(ctx.get("git_cwd", REPO), hs[0] if hs else "HEAD")
                    if not (ctx.get("is_shallow") or (ctx.get("skip_recompute") and nums is None)):
                        if not nums:
                            fail(cid, "count does not match git log")
                        else:
                            for label, n in zip(("author", "committer", "combined"), nums):
                                if not re.search(rf"{label}[^\n]*\b{n}\b", body, re.I):
                                    fail(cid, "count does not match git log")
                                    break
            elif cid == "name-collisions":
                for t in TARGETS:
                    line = next((ln for ln in body.splitlines() if t in ln), "")
                    if "2026-10-29" not in line or "D125" not in line:
                        fail(cid, "target missing date or source")
                        break
            elif cid == "owner-items":
                items = [ln for ln in body.splitlines() if "OPEN" in ln and "2026-11-08" in ln]
                if len(items) < 6:
                    fail(cid, "need at least 6 OPEN items dated 2026-11-08")
        if len(re.findall(r"(?m)^Recommendation:", page)) != len(found):
            fail("recommendations", "one Recommendation line per topic section")
        reasons = ctx.get("forbidden_reasons")
        if reasons:
            for line in page.splitlines():
                if reasons("docs/release/pre-publication-checks.md", line):
                    fail("personal-names", "a page line has forbidden attribution")
                    break
    code = ctx.get("personal_names_exit")
    if code not in (None, 0):
        fail("personal-names", "check-no-personal-names.py exit " + str(code))
    scan = ctx.get("scan_script")
    if scan and scan != "ok":
        fail("scan-script", scan)
    return failures


def fixture_ctx():
    return {
        "packages": {
            ("FastEndpoints", "8.3.0"), ("Microsoft.NET.Test.Sdk", "17.11.1"),
            ("xunit", "2.9.2"), ("xunit.runner.visualstudio", "2.8.2"),
        },
        "models": [
            "grok-4.7", "copilot-default", "codex-gpt-5.6-sol", "agy-gemini-4-argon-medium",
        ],
        "author_counts": (1, 1, 1), "skip_recompute": True,
        "forbidden_reasons": check.forbidden_reasons,
    }


def two_authors(root):
    git(["init"], root)
    pair = (("Alpha" + " Tester", "alpha" + "@" + "example.test"), ("Beta" + " Tester", "beta" + "@" + "example.test"))
    git(["config", "user.name", pair[0][0]], root)
    git(["config", "user.email", pair[0][1]], root)
    git(["config", "commit.gpgsign", "false"], root)
    for i, (n, e) in enumerate(pair):
        (root / "f.txt").write_text(str(i) + "\n", encoding="utf-8")
        git(["add", "f.txt"], root)
        env = os.environ.copy()
        env.update(GIT_AUTHOR_NAME=n, GIT_AUTHOR_EMAIL=e, GIT_COMMITTER_NAME=n, GIT_COMMITTER_EMAIL=e)
        sh(["git", "commit", "-m", str(i)], cwd=root, env=env)


class PrePublicationSelfTests(unittest.TestCase):
    def test_compliant_fixture_page_gives_empty(self):
        self.assertEqual([], collect_failures(COMPLIANT, fixture_ctx()))

    def test_missing_page_gives_the_blocked_ids(self):
        self.assertEqual(
            ["page: docs/release/pre-publication-checks.md is missing"]
            + [cid + ": blocked: page missing" for cid in BLOCKED],
            collect_failures(None, {}),
        )

    def test_a_name_or_address_on_a_line_fails(self):
        named = COMPLIANT.replace(
            "the repository-local identity; every commit carries it",
            "approved by " + "Ste" + "ve",
        )
        self.assertIn("personal-names", [x.split(":", 1)[0] for x in collect_failures(named, fixture_ctx())])
        mailed = COMPLIANT + "\ncontact " + "alpha" + "@" + "example.test" + "\n"
        self.assertIn("author-metadata", [x.split(":", 1)[0] for x in collect_failures(mailed, fixture_ctx())])

    def test_stale_dependency_list_fails_package_licences(self):
        page = COMPLIANT.replace("| FastEndpoints | 8.3.0 | MIT | nuspec | generated |\n", "")
        self.assertIn("package-licences", [x.split(":", 1)[0] for x in collect_failures(page, fixture_ctx())])

    def test_author_count_of_three_fails_against_two_identities(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            two_authors(root)
            nums = counts_of(root, "HEAD")
            page = (COMPLIANT.replace("distinct author identities 1", "distinct author identities 3")
                    .replace("committer identities 1", "committer identities 3")
                    .replace("combined 1", "combined 3"))
            ctx = fixture_ctx()
            ctx["author_counts"] = nums
            self.assertEqual((2, 2, 2), nums)
            self.assertIn("author-metadata", [x.split(":", 1)[0] for x in collect_failures(page, ctx)])


class PrePublicationChecksTests(unittest.TestCase):
    def test_pre_publication_findings_match_the_repo(self):
        failures = collect_failures(None if not PAGE.is_file() else PAGE.read_text(encoding="utf-8").replace("\r\n", "\n"), live_ctx())
        self.assertEqual([], failures, HEADER + "\n".join(failures))


if __name__ == "__main__":
    unittest.main()
