"""Card 369: every package a generated service and its specs restore is listed with its licence.

Offline unittest. pathlib and json (plus the existing page helpers). Never starts dotnet and never
reads the NuGet cache. Failure messages start `restored-package-licences: <id> <version>: <what>`.
"""

import json
import re
import unittest
import xml.etree.ElementTree as ET

from tests.test_pre_publication_checks import HASH_RE, PAGE, PKG_RE, REPO, git, rows, sect

TITLE = "Restored package licences"
ALLOWED = {"MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause"}
SOURCES = ("nuspec licence expression", "packaged licence file", "licence URL")
WHERE_API = "generated service and generated specs"
WHERE_SPECS = "generated specs (test-only)"
API_LOCK_REL = "reference/csharp/Orders.Api/packages.lock.json"
SPECS_LOCK_REL = "reference/csharp/Orders.Specs/packages.lock.json"
API_LOCK = REPO / "reference" / "csharp" / "Orders.Api" / "packages.lock.json"
SPECS_LOCK = REPO / "reference" / "csharp" / "Orders.Specs" / "packages.lock.json"
PROPS = REPO / "reference" / "Directory.Build.props"
PROVIDER = REPO / "src" / "Avowry.Adapters.CSharp" / "CSharpProvider.cs"
CLI_PROJ = REPO / "src" / "Avowry.Cli" / "Avowry.Cli.csproj"
NONE_LINE = "No package is outside MIT, Apache-2.0, BSD-2-Clause or BSD-3-Clause"
TOOL_LINE = "The Avowry tool restores no packages"
H40 = "c" * 40
PROJECT_RE = re.compile(r'ProjectReference Include=\\?"([^"\\]+)\\?"')
ROW_LICENCE = {
    "FastEndpoints": ("MIT", "nuspec licence expression", "ok"),
    "FluentValidation": ("Apache-2.0", "nuspec licence expression", "ok"),
    "Microsoft.NET.Test.Sdk": ("MIT", "nuspec licence expression", "ok"),
    "xunit": ("Apache-2.0", "nuspec licence expression", "ok"),
    "xunit.runner.visualstudio": ("Apache-2.0", "nuspec licence expression", "ok"),
    "System.Reflection.Metadata": ("MIT", "packaged licence file", "ok"),
    "xunit.abstractions": ("MIT", "licence URL", "ok"),
    "Microsoft.CodeCoverage": ("MIT", "nuspec licence expression", "ok"),
}


def msg(pid, version, what):
    return f"restored-package-licences: {pid} {version}: {what}"


def read_page():
    if not PAGE.is_file():
        return None
    return PAGE.read_text(encoding="utf-8").replace("\r\n", "\n")


def read_lock(path):
    if not path.is_file():
        return None
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError:
        return {"_unparsed": True}


def read_locks():
    return {"Orders.Api": read_lock(API_LOCK), "Orders.Specs": read_lock(SPECS_LOCK)}


def read_provider():
    return PROVIDER.read_text(encoding="utf-8") if PROVIDER.is_file() else ""


def load_tool_texts():
    texts = {}

    def add(path):
        try:
            resolved = path.resolve()
            rel = resolved.relative_to(REPO.resolve()).as_posix()
        except ValueError:
            return
        if rel in texts or not resolved.is_file():
            return
        text = resolved.read_text(encoding="utf-8")
        texts[rel] = text
        for inc in PROJECT_RE.findall(text):
            add(resolved.parent / inc.replace("\\", "/"))

    add(CLI_PROJ)
    return texts


def lock_packages(lock, kinds=("Direct", "Transitive")):
    if not isinstance(lock, dict) or lock.get("_unparsed"):
        return {}
    net = (lock.get("dependencies") or {}).get("net8.0") or {}
    out = {}
    for pid, meta in net.items():
        if not isinstance(meta, dict) or meta.get("type") == "Project":
            continue
        if meta.get("type") in kinds:
            out[pid] = meta
    return out


def lock_parse_failures(locks):
    out = []
    for name in ("Orders.Api", "Orders.Specs"):
        lock = (locks or {}).get(name)
        if lock is None:
            out.append(msg(name, "-", "lockfile missing"))
        elif lock.get("_unparsed"):
            out.append(msg(name, "-", "lockfile does not parse"))
        elif "net8.0" not in (lock.get("dependencies") or {}):
            out.append(msg(name, "-", 'dependencies["net8.0"] missing'))
    return out


def union_packages(locks):
    conflicts, merged = [], {}
    api = lock_packages((locks or {}).get("Orders.Api") or {})
    specs = lock_packages((locks or {}).get("Orders.Specs") or {})
    for source, in_api in ((api, True), (specs, False)):
        for pid, meta in source.items():
            resolved = meta.get("resolved") or ""
            direct = meta.get("type") == "Direct"
            if pid in merged:
                if merged[pid]["resolved"] != resolved:
                    conflicts.append(msg(pid, resolved, "version conflict"))
                merged[pid]["direct"] = merged[pid]["direct"] or direct
                merged[pid]["in_api"] = merged[pid]["in_api"] or in_api
            else:
                merged[pid] = {"resolved": resolved, "direct": direct, "in_api": in_api}
    return merged, conflicts


def restored_body(page):
    return sect(page or "", TITLE)


def page_rows(page):
    body = restored_body(page)
    return [] if body is None else rows(body)


def row_match_failures(page, locks):
    if lock_parse_failures(locks):
        return []
    merged, conflicts = union_packages(locks)
    out = list(conflicts)
    by_id = {}
    for r in page_rows(page):
        pid = r[0]
        if pid in by_id:
            out.append(msg(pid, r[1] if len(r) > 1 else "-", "appears more than once"))
        else:
            by_id[pid] = r
    for pid, info in merged.items():
        r = by_id.get(pid)
        if r is None:
            out.append(msg(pid, info["resolved"], "missing from the page"))
        elif len(r) < 2 or r[1] != info["resolved"]:
            got = r[1] if r and len(r) > 1 else "-"
            out.append(msg(pid, got, f"listed at a different version than {info['resolved']}"))
    for pid, r in by_id.items():
        if pid not in merged:
            out.append(msg(pid, r[1] if len(r) > 1 else "-", "in neither lockfile"))
    return out


def kind_where_failures(page, locks):
    if lock_parse_failures(locks):
        return []
    if restored_body(page) is None:
        return [msg("section", "-", "missing")]
    merged, conflicts = union_packages(locks)
    out = list(conflicts)
    by_id = {r[0]: r for r in page_rows(page) if r}
    for pid, info in merged.items():
        r = by_id.get(pid)
        if not r or len(r) < 6:
            continue
        expect_kind = "Direct" if info["direct"] else "Transitive"
        expect_where = WHERE_API if info["in_api"] else WHERE_SPECS
        if r[2] != expect_kind:
            out.append(msg(pid, info["resolved"], f"kind {r[2]} expected {expect_kind}"))
        if r[5] != expect_where:
            out.append(msg(pid, info["resolved"], f"where used {r[5]} expected {expect_where}"))
    return out


def row_shape_failures(page):
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    table = page_rows(page)
    if not table:
        return [msg("section", "-", "no rows")]
    out = []
    for r in table:
        pid, ver = r[0], r[1] if len(r) > 1 else "-"
        if len(r) < 7:
            out.append(msg(pid, ver, "row missing licence, licence source or where-used"))
            continue
        if not r[3]:
            out.append(msg(pid, ver, "licence empty"))
        if r[4] not in SOURCES:
            out.append(msg(pid, ver, "licence source is not one of the three labels"))
        if not r[5]:
            out.append(msg(pid, ver, "where-used empty"))
    return out


def section_meta_failures(page):
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    out = []
    if API_LOCK_REL not in body:
        out.append(msg("section", "-", f"missing {API_LOCK_REL}"))
    if SPECS_LOCK_REL not in body:
        out.append(msg("section", "-", f"missing {SPECS_LOCK_REL}"))
    if "dotnet restore" not in body:
        out.append(msg("section", "-", "missing dotnet restore"))
    if "--locked-mode" not in body:
        out.append(msg("section", "-", "missing --locked-mode"))
    if not HASH_RE.search(body):
        out.append(msg("section", "-", "missing 40-character commit"))
    return out


def lockfile_commit_failures(page):
    body = restored_body(page) or ""
    hashes = HASH_RE.findall(body)
    if not hashes:
        return []
    shallow = git(["rev-parse", "--is-shallow-repository"])
    if shallow and shallow.stdout.strip() == "true":
        print("shallow: recompute skipped")
        return []
    commit = hashes[0]
    out = []
    for rel, path in ((API_LOCK_REL, API_LOCK), (SPECS_LOCK_REL, SPECS_LOCK)):
        if not path.is_file():
            continue
        working = path.read_text(encoding="utf-8").replace("\r\n", "\n")
        shown = git(["show", f"{commit}:{rel}"])
        if shown is None or shown.returncode != 0:
            out.append(msg("section", commit, f"lockfile blob missing at that commit ({rel})"))
            continue
        if (shown.stdout or "").replace("\r\n", "\n") != working:
            out.append(msg("section", commit, f"lockfile blob at that commit differs from the working file ({rel})"))
    return out


def props_failures(text):
    if text is None:
        return [msg("Directory.Build.props", "-", "missing")]
    try:
        root = ET.fromstring(text)
    except ET.ParseError:
        return [msg("Directory.Build.props", "-", "does not parse")]
    out = []
    for name in ("RestorePackagesWithLockFile", "RestoreLockedMode"):
        found = False
        for elem in root.iter():
            tag = elem.tag.rsplit("}", 1)[-1]
            if tag == name and (elem.text or "").strip().lower() == "true":
                found = True
                break
        if not found:
            out.append(msg(name, "-", "not true"))
    return out


def tool_package_ref_failures(tool_texts):
    out = []
    for key, text in (tool_texts or {}).items():
        rel = str(key).replace("\\", "/")
        for pid, ver in PKG_RE.findall(text or ""):
            out.append(msg(pid, ver, f"PackageReference in {rel}"))
    return out


def tool_phrase_failures(page, tool_texts):
    out = tool_package_ref_failures(tool_texts)
    if TOOL_LINE not in (page or ""):
        out.append(msg("Avowry tool", "-", f'page missing "{TOOL_LINE}"'))
    return out


def no_expression_failures(page):
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    out = []
    if "No licence expression" not in body:
        out.append(msg("section", "-", "missing No licence expression bullet"))
    needed = [("System.Reflection.Metadata", "1.6.0"), ("xunit.abstractions", "2.0.3")]
    for r in page_rows(page):
        if len(r) >= 5 and r[4] != "nuspec licence expression":
            pair = (r[0], r[1])
            if pair not in needed:
                needed.append(pair)
    for pid, ver in needed:
        named = any(pid in ln and ver in ln for ln in body.splitlines())
        rec = any(pid in ln and "recommend" in ln.lower() for ln in body.splitlines())
        if not named:
            out.append(msg(pid, ver, "not named on the No licence expression bullet"))
        elif not rec:
            out.append(msg(pid, ver, "No licence expression bullet missing a recommendation"))
    return out


def acceptance_failures(page):
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    if not re.search(r"acceptance", body, re.I):
        return [msg("Microsoft.CodeCoverage", "17.11.1", "not named on the acceptance bullet")]
    named = any("Microsoft.CodeCoverage" in ln and "17.11.1" in ln for ln in body.splitlines())
    rec = any("Microsoft.CodeCoverage" in ln and "recommend" in ln.lower() for ln in body.splitlines())
    if not named:
        return [msg("Microsoft.CodeCoverage", "17.11.1", "not named on the acceptance bullet")]
    if not rec:
        return [msg("Microsoft.CodeCoverage", "17.11.1", "acceptance bullet missing a recommendation")]
    return []


def allowed_list_failures(page):
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    out, outside = [], []
    for r in page_rows(page):
        if len(r) < 7:
            continue
        if r[3] not in ALLOWED:
            outside.append(r)
            if r[6] != "needs review":
                out.append(msg(r[0], r[1], "needs review"))
    if not outside and NONE_LINE not in body:
        out.append(msg("allowed list", "-", f'missing "{NONE_LINE}"'))
    return out


def b4_failures(page):
    out = []
    restored = restored_body(page)
    if restored is None:
        out.append(msg("section", "-", "missing"))
        restored = ""
    n = len(re.findall(r"(?m)^Recommendation:", restored))
    if n != 1:
        out.append(msg("section", "-", f"expected exactly one Recommendation line, found {n}"))
    licences = sect(page or "", "Package licences")
    if licences is None:
        out.append(msg("Package licences", "-", "missing"))
    else:
        if "not reviewed" in licences:
            out.append(msg("Package licences", "-", "still says not reviewed"))
        if TITLE not in licences:
            out.append(msg("Package licences", "-", "does not point to Restored package licences"))
    return out


def split_provider(text):
    api_at = (text or "").find("private static string ApiProject")
    specs_at = (text or "").find("private static string SpecsProject")
    runtime_at = (text or "").find("private static string Runtime")
    if api_at < 0 or specs_at < 0:
        return text or "", ""
    specs_end = runtime_at if runtime_at > specs_at else len(text)
    return text[api_at:specs_at], text[specs_at:specs_end]


def provider_direct_failures(locks, provider_text):
    out = lock_parse_failures(locks)
    if out:
        return out
    api_src, specs_src = split_provider(provider_text)
    api_pkgs = set(PKG_RE.findall(api_src))
    specs_pkgs = set(PKG_RE.findall(specs_src))
    api_direct = {(pid, m.get("resolved") or "") for pid, m in lock_packages((locks or {}).get("Orders.Api") or {}, ("Direct",)).items()}
    specs_direct = {(pid, m.get("resolved") or "") for pid, m in lock_packages((locks or {}).get("Orders.Specs") or {}, ("Direct",)).items()}
    for pid, ver in sorted(api_pkgs - api_direct):
        out.append(msg(pid, ver, "provider ApiProject package not Direct in the Orders.Api lockfile"))
    for pid, ver in sorted(api_direct - api_pkgs):
        out.append(msg(pid, ver, "Orders.Api lockfile Direct not in the provider ApiProject"))
    for pid, ver in sorted(specs_pkgs - specs_direct):
        out.append(msg(pid, ver, "provider SpecsProject package not Direct in the Orders.Specs lockfile"))
    for pid, ver in sorted(specs_direct - specs_pkgs):
        out.append(msg(pid, ver, "Orders.Specs lockfile Direct not in the provider SpecsProject"))
    return out


def collect_failures(page, locks, provider_text, tool_texts):
    page = "" if page is None else page.replace("\r\n", "\n")
    locks = locks or {}
    out, seen = [], set()

    def add(items):
        for item in items:
            if item not in seen:
                seen.add(item)
                out.append(item)

    add(lock_parse_failures(locks))
    add(row_match_failures(page, locks))
    add(kind_where_failures(page, locks))
    add(row_shape_failures(page))
    add(section_meta_failures(page))
    add(tool_phrase_failures(page, tool_texts))
    add(no_expression_failures(page))
    add(acceptance_failures(page))
    add(allowed_list_failures(page))
    add(b4_failures(page))
    add(provider_direct_failures(locks, provider_text))
    return out


def make_lock(entries):
    deps = {}
    for pid, ver, kind in entries:
        rec = {"type": kind, "resolved": ver, "contentHash": "A" * 88}
        if kind == "Direct":
            rec["requested"] = f"[{ver}, )"
        deps[pid] = rec
    return {"version": 1, "dependencies": {"net8.0": deps}}


def passing_locks():
    api = make_lock([
        ("FastEndpoints", "8.3.0", "Direct"),
        ("FluentValidation", "12.1.1", "Transitive"),
    ])
    specs = make_lock([
        ("Microsoft.NET.Test.Sdk", "17.11.1", "Direct"),
        ("xunit", "2.9.2", "Direct"),
        ("xunit.runner.visualstudio", "2.8.2", "Direct"),
        ("FastEndpoints", "8.3.0", "Transitive"),
        ("System.Reflection.Metadata", "1.6.0", "Transitive"),
        ("xunit.abstractions", "2.0.3", "Transitive"),
        ("Microsoft.CodeCoverage", "17.11.1", "Transitive"),
    ])
    specs["dependencies"]["net8.0"]["Orders.Api"] = {"type": "Project"}
    return {"Orders.Api": api, "Orders.Specs": specs}


def passing_provider():
    return (
        "private static string ApiProject(string module) =>\n"
        "    \"<PackageReference Include=\\\"FastEndpoints\\\" Version=\\\"8.3.0\\\" />\\n\";\n"
        "private static string SpecsProject(string module) =>\n"
        "    \"<PackageReference Include=\\\"Microsoft.NET.Test.Sdk\\\" Version=\\\"17.11.1\\\" />\\n\" +\n"
        "    \"<PackageReference Include=\\\"xunit\\\" Version=\\\"2.9.2\\\" />\\n\" +\n"
        "    \"<PackageReference Include=\\\"xunit.runner.visualstudio\\\" Version=\\\"2.8.2\\\" />\\n\";\n"
        "private static string Runtime(string module) => \"\";\n"
    )


def passing_tools():
    return {
        "src/Avowry.Cli/Avowry.Cli.csproj": (
            "<Project>\n  <ItemGroup>\n"
            "    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n"
            "    <ProjectReference Include=\"../Avowry.Adapters.CSharp/Avowry.Adapters.CSharp.csproj\" />\n"
            "  </ItemGroup>\n</Project>\n"
        ),
        "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project></Project>\n",
        "src/Avowry.Adapters.CSharp/Avowry.Adapters.CSharp.csproj": (
            "<Project>\n  <ItemGroup>\n"
            "    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n"
            "  </ItemGroup>\n</Project>\n"
        ),
    }


def passing_row_lines(locks):
    merged, _ = union_packages(locks)
    lines = []
    for pid in sorted(merged):
        info = merged[pid]
        lic, src, rev = ROW_LICENCE[pid]
        kind = "Direct" if info["direct"] else "Transitive"
        where = WHERE_API if info["in_api"] else WHERE_SPECS
        lines.append(f"| {pid} | {info['resolved']} | {kind} | {lic} | {src} | {where} | {rev} |")
    return lines


def passing_page(locks=None, extra=""):
    locks = passing_locks() if locks is None else locks
    return "\n".join([
        "# Pre-publication checks",
        "## Package licences",
        "Listed packages; transitive: see Restored package licences.",
        "Recommendation: proceed.",
        f"## {TITLE}",
        f"Command: `dotnet restore` {API_LOCK_REL} {SPECS_LOCK_REL} `--locked-mode`. Commit `{H40}`.",
        TOOL_LINE + " (Avowry.Cli, Avowry.Compiler, Avowry.Adapters.CSharp).",
        "| id | version | kind | licence | licence source | where used | review |",
        "| --- | --- | --- | --- | --- | --- | --- |",
        *passing_row_lines(locks),
        "- No licence expression: System.Reflection.Metadata 1.6.0 packaged file MIT, recommend accept; "
        "xunit.abstractions 2.0.3 URL only, recommend review the URL.",
        "- Licence acceptance required: Microsoft.CodeCoverage 17.11.1, recommend read the EULA.",
        NONE_LINE + " (bundled third-party notice files are not reviewed).",
        extra,
        "Recommendation: proceed.",
        "",
    ])


def passing_fixture():
    locks = passing_locks()
    return passing_page(locks), locks, passing_provider(), passing_tools()


def live_bundle():
    return read_page(), read_locks(), read_provider(), load_tool_texts()


class RestoredLicencesOffline(unittest.TestCase):
    def test_L1_both_lockfiles_exist_parse_and_have_net8_0(self):
        # Given committed lockfiles for the reference API project and the reference specs project
        # When they are read as JSON
        # Then each exists, parses, and has dependencies["net8.0"]
        self.assertEqual([], lock_parse_failures(read_locks()))

    def test_L2_union_of_lockfile_packages_matches_page_rows(self):
        # Given the two lockfiles and the restored-package licences page
        # When the union of Direct and Transitive entries (excluding Project) is compared with the page rows
        # Then each (id, resolved) appears as exactly one row, and two versions of one id fail as a version conflict
        page, locks, _, _ = live_bundle()
        self.assertEqual([], lock_parse_failures(locks) + row_match_failures(page, locks))

    def test_L3_direct_kind_and_where_used(self):
        # Given an entry Direct in either lockfile
        # When its page row is read
        # Then kind is Direct, and where used is generated service and generated specs for an API lockfile package
        page, locks, _, _ = live_bundle()
        self.assertEqual([], lock_parse_failures(locks) + kind_where_failures(page, locks))

    def test_L4_every_row_has_licence_source_and_where_used(self):
        # Given every restored-package licences row
        # When its cells are read
        # Then licence is non-empty, licence source is one of the three labels, and where-used is present
        self.assertEqual([], row_shape_failures(read_page()))

    def test_L5_section_names_lockfiles_restore_locked_mode_and_commit(self):
        # Given the Restored package licences section
        # When Dana reads the command and the cited commit
        # Then it names both lockfile paths, dotnet restore, --locked-mode and a 40-hex commit whose lockfile blobs match
        page = read_page()
        self.assertEqual([], section_meta_failures(page) + lockfile_commit_failures(page))

    def test_L6_reference_props_enable_lockfile_restore(self):
        # Given reference/Directory.Build.props
        # When it is read as XML
        # Then RestorePackagesWithLockFile and RestoreLockedMode are true
        text = PROPS.read_text(encoding="utf-8") if PROPS.is_file() else None
        self.assertEqual([], props_failures(text))

    def test_Z1_the_avowry_tool_restores_no_packages(self):
        # Given the Avowry tool project and its two referenced projects
        # When their PackageReferences are followed recursively and Dana reads the page
        # Then none has a PackageReference and the page says The Avowry tool restores no packages
        page, _, _, tools = live_bundle()
        self.assertEqual([], tool_phrase_failures(page, tools))

    def test_B1_no_licence_expression_packages_are_named(self):
        # Given rows whose licence source is not nuspec licence expression
        # When Dana reads the No licence expression bullet
        # Then System.Reflection.Metadata 1.6.0 and xunit.abstractions 2.0.3 are named with a recommendation
        self.assertEqual([], no_expression_failures(read_page()))

    def test_B2_licence_acceptance_names_microsoft_codecoverage(self):
        # Given packages that ask for licence acceptance
        # When Dana reads the acceptance bullet
        # Then it names at least Microsoft.CodeCoverage 17.11.1 with a recommendation
        self.assertEqual([], acceptance_failures(read_page()))

    def test_B3_outside_allowed_list_or_the_none_line(self):
        # Given every restored-package licences row
        # When its licence is compared with MIT, Apache-2.0, BSD-2-Clause and BSD-3-Clause
        # Then a row outside that list carries needs review, otherwise the none line exists
        self.assertEqual([], allowed_list_failures(read_page()))

    def test_B4_one_recommendation_and_package_licences_points_here(self):
        # Given the Restored package licences section and the Package licences body
        # When Dana reads both
        # Then the new section has exactly one Recommendation line, and Package licences no longer says not reviewed
        self.assertEqual([], b4_failures(read_page()))

    def test_E4_lockfile_direct_set_equals_the_provider(self):
        # Given the C# provider's ApiProject and SpecsProject texts and the two lockfiles
        # When Direct entries are compared with the PackageReferences each method writes
        # Then the sets are equal
        _, locks, provider, _ = live_bundle()
        self.assertEqual([], provider_direct_failures(locks, provider))

    def test_E1_E2_E3_planted_drift_is_reported(self):
        # Given a passing page and lockfiles
        # When a lockfile package is missing from the page, listed at another version, or a page row is in neither lockfile
        # Then each drift is reported by id and version
        page, locks, provider, tools = passing_fixture()
        self.assertEqual([], collect_failures(page, locks, provider, tools))
        missing = page.replace("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n", "")
        with self.subTest(id="E1"):
            self.assertIn(msg("FastEndpoints", "8.3.0", "missing from the page"), row_match_failures(missing, locks))
        wrong = page.replace("| FastEndpoints | 8.3.0 |", "| FastEndpoints | 9.0.0 |")
        with self.subTest(id="E2"):
            self.assertIn(msg("FastEndpoints", "9.0.0", "listed at a different version than 8.3.0"), row_match_failures(wrong, locks))
        extra = page.replace(
            "| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |",
            "| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n"
            "| ExtraPackage | 1.0.0 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |",
        )
        with self.subTest(id="E3"):
            self.assertIn(msg("ExtraPackage", "1.0.0", "in neither lockfile"), row_match_failures(extra, locks))

    def test_E4_planted_provider_version_is_reported(self):
        # Given a passing lockfile Direct set
        # When the provider ApiProject writes another version
        # Then the drift is reported by id and version
        page, locks, provider, tools = passing_fixture()
        planted = provider.replace("FastEndpoints\\\" Version=\\\"8.3.0", "FastEndpoints\\\" Version=\\\"9.0.0")
        fails = provider_direct_failures(locks, planted)
        self.assertTrue(any(f.startswith("restored-package-licences: FastEndpoints ") for f in fails), fails)
        self.assertEqual([], collect_failures(page, locks, provider, tools))

    def test_E5_planted_tool_packagereference_is_reported(self):
        # Given three tool project texts with no PackageReference, and one planted PackageReference
        # When collect_failures reads them
        # Then the clean texts pass and the planted PackageReference is reported by id and version
        page, locks, provider, tools = passing_fixture()
        with self.subTest(id="E5-clean"):
            self.assertEqual([], tool_package_ref_failures(tools))
        dirty = dict(tools)
        dirty["src/Avowry.Compiler/Avowry.Compiler.csproj"] = (
            "<Project>\n  <ItemGroup>\n"
            "    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n"
            "  </ItemGroup>\n</Project>\n"
        )
        with self.subTest(id="E5"):
            self.assertIn(
                msg("Newtonsoft.Json", "13.0.1", "PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj"),
                tool_package_ref_failures(dirty),
            )

    def test_B3_planted_outside_and_inside_allowed_list(self):
        # Given a page whose licences are all allowed, and a page with a licence outside that list
        # When the allowed-list check runs
        # Then the none line is required for the first, and needs review is required for the second
        page, locks, provider, tools = passing_fixture()
        with self.subTest(id="B3-inside"):
            self.assertEqual([], allowed_list_failures(page))
        old = "| FluentValidation | 12.1.1 | Transitive | Apache-2.0 | nuspec licence expression | generated service and generated specs | ok |"
        unflagged = page.replace(
            old,
            "| FluentValidation | 12.1.1 | Transitive | GPL-2.0 | nuspec licence expression | generated service and generated specs | ok |",
        )
        with self.subTest(id="B3-outside"):
            self.assertIn(msg("FluentValidation", "12.1.1", "needs review"), allowed_list_failures(unflagged))
        flagged = page.replace(
            old,
            "| FluentValidation | 12.1.1 | Transitive | GPL-2.0 | nuspec licence expression | generated service and generated specs | needs review |",
        ).replace(NONE_LINE, "")
        with self.subTest(id="B3-flagged"):
            self.assertEqual([], allowed_list_failures(flagged))
        self.assertEqual([], collect_failures(page, locks, provider, tools))

    def test_passing_fixture_gives_no_failures(self):
        # Given the planted passing page, lockfiles, provider and tool texts
        # When collect_failures reads them
        # Then there are no failures
        page, locks, provider, tools = passing_fixture()
        self.assertEqual([], collect_failures(page, locks, provider, tools))


if __name__ == "__main__":
    unittest.main()
