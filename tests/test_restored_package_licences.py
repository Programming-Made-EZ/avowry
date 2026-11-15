"""Card 369: every package a generated service and its specs restore is listed with its licence.

Offline unittest. pathlib and json (plus the existing page helpers). Never starts dotnet and never
reads the NuGet cache. Failure messages start `restored-package-licences: <id> <version>: <what>`.
"""

import json
import posixpath
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


def repo_reader(rel):
    path = REPO / rel
    return path.read_text(encoding="utf-8") if path.is_file() else None


DIR_FILES = ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props")


def _local(tag):
    return tag.rsplit("}", 1)[-1] if isinstance(tag, str) else ""


def _attr(el, name):
    for key, value in el.attrib.items():
        if _local(key).lower() == name.lower():
            return value
    return None


def _norm_rel(base_dir, inc):
    path = inc.strip().replace("\\", "/")
    return posixpath.normpath(posixpath.join(base_dir, path)) if base_dir else posixpath.normpath(path)


def _project_refs(text):
    try:
        root = ET.fromstring(text)
    except ET.ParseError:
        return PROJECT_RE.findall(text)
    out = []
    for el in root.iter():
        if _local(el.tag).lower() == "projectreference":
            include = _attr(el, "Include")
            if include:
                out.extend(part for part in include.split(";") if part.strip())
    return out


def _imports(text):
    try:
        root = ET.fromstring(text)
    except ET.ParseError:
        return []
    return [el for el in root.iter() if _local(el.tag) == "Import" and _attr(el, "Sdk") is None]


def _import_target(imp):
    project = (_attr(imp, "Project") or "").replace("$(MSBuildThisFileDirectory)", "")
    if not project.strip() or "$(" in project or "@(" in project or "*" in project:
        return None
    return project


def tool_closure(start_rel, read):
    texts = {}

    def add(rel):
        rel = posixpath.normpath(rel)
        if rel.startswith("..") or rel in texts:
            return
        text = read(rel)
        if text is None:
            return
        texts[rel] = text
        base = posixpath.dirname(rel)
        if rel.endswith("proj"):
            for include in _project_refs(text):
                add(_norm_rel(base, include))
            current = base
            while True:
                for name in DIR_FILES:
                    add(posixpath.join(current, name) if current else name)
                if not current:
                    break
                current = posixpath.dirname(current)
        for imp in _imports(text):
            project = _import_target(imp)
            if project is not None:
                add(_norm_rel(base, project))

    add(start_rel)
    return texts


def load_tool_texts():
    return tool_closure(CLI_PROJ.relative_to(REPO).as_posix(), repo_reader)


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
    if body is None:
        return []
    return rows("\n".join(re.sub(r"^ {1,3}\|", "|", line) for line in body.split("\n")))


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


SHALLOW_NOTE = "restored-package-licences: section -: commit-blob check skipped (shallow checkout)"


def is_shallow():
    r = git(["rev-parse", "--is-shallow-repository"])
    return bool(r) and r.stdout.strip() == "true"


def lockfile_commit_skip(page, shallow):
    body = restored_body(page) or ""
    if not HASH_RE.search(body):
        return None
    return SHALLOW_NOTE if shallow else None


def lockfile_commit_failures(page, shallow=None):
    body = restored_body(page) or ""
    hashes = HASH_RE.findall(body)
    if not hashes:
        return []
    shallow = is_shallow() if shallow is None else shallow
    if shallow:
        print(SHALLOW_NOTE)
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


LOCK_PROPS = ("RestorePackagesWithLockFile", "RestoreLockedMode")
_TRUE_WORDS, _FALSE_WORDS = {"true", "on", "yes"}, {"false", "off", "no"}
UNKNOWN = object()


def condition_value(cond):
    if cond is None:
        return True
    text = cond.strip()
    if not text:
        return True
    lowered = text.lower()
    if any(token in text for token in ("$(", "@(", "%(")) or " and " in lowered or " or " in lowered:
        return None
    word = text.strip("'").strip().lower() if re.fullmatch(r"'[^']*'|[A-Za-z]+", text) else None
    if word in _TRUE_WORDS:
        return True
    if word in _FALSE_WORDS:
        return False
    match = re.fullmatch(r"'([^']*)'\s*(==|!=)\s*'([^']*)'", text)
    if match:
        same = match.group(1).strip().lower() == match.group(3).strip().lower()
        return same if match.group(2) == "==" else not same
    return None


def props_failures(text):
    if text is None:
        return [msg("Directory.Build.props", "-", "missing")]
    try:
        root = ET.fromstring(text)
    except ET.ParseError:
        return [msg("Directory.Build.props", "-", "does not parse")]

    value = {name: None for name in LOCK_PROPS}
    why = {name: "never set" for name in LOCK_PROPS}

    for child in list(root):
        tag = _local(child.tag)
        if tag in ("Import", "ImportGroup", "Choose"):
            if tag != "Import" or _attr(child, "Sdk") is None:
                for name in LOCK_PROPS:
                    value[name], why[name] = UNKNOWN, f"a later {tag} may change it"
            continue
        if tag != "PropertyGroup":
            continue

        group_cond = condition_value(_attr(child, "Condition"))
        for element in list(child):
            name = _local(element.tag)
            if name not in LOCK_PROPS:
                continue
            element_cond = condition_value(_attr(element, "Condition"))
            if group_cond is False or element_cond is False:
                if value[name] is None:
                    why[name] = "set only under a false Condition"
                continue
            if group_cond is None or element_cond is None:
                value[name], why[name] = UNKNOWN, "set under a Condition the check cannot decide"
                continue
            text_value = (element.text or "").strip()
            value[name] = text_value.lower()
            why[name] = f"last set to {text_value or 'empty'}"

    out = []
    for name in LOCK_PROPS:
        if value[name] != "true":
            out.append(msg(name, "-", f"not true ({why[name]})"))
    return out


TOOL_ITEMS = {
    "packagereference": "PackageReference",
    "globalpackagereference": "GlobalPackageReference",
    "packagedownload": "PackageDownload",
}


def _item_version(el):
    for name in ("Version", "VersionOverride"):
        value = _attr(el, name)
        if value is not None and value.strip():
            return value.strip()
    for sub in list(el):
        if _local(sub.tag).lower() in ("version", "versionoverride") and (sub.text or "").strip():
            return sub.text.strip()
    return "-"


def tool_package_ref_failures(tool_texts):
    out = []
    for key, text in (tool_texts or {}).items():
        rel = str(key).replace("\\", "/")
        try:
            root = ET.fromstring(text or "")
        except ET.ParseError:
            out.append(msg(rel, "-", "does not parse as MSBuild XML"))
            continue

        for el in root.iter():
            kind = TOOL_ITEMS.get(_local(el.tag).lower())
            if kind is None:
                continue
            ids = _attr(el, "Include")
            if ids is None or not ids.strip():
                ids = _attr(el, "Update")
            if ids is None or not ids.strip():
                continue
            version = _item_version(el)
            for pid in ids.split(";"):
                if pid.strip():
                    out.append(msg(pid.strip(), version, f"{kind} in {rel}"))

        for imp in _imports(text or ""):
            if _import_target(imp) is None:
                out.append(msg(rel, "-", f"Import {_attr(imp, 'Project')} cannot be followed"))
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


_UNITS = [
    "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
    "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
]
_TENS = {"twenty": 20, "thirty": 30, "forty": 40, "fifty": 50}
_WORDNUM = (r"(?:(?:twenty|thirty|forty|fifty)(?:[- ](?:one|two|three|four|five|six|seven|eight|nine))?"
            r"|" + "|".join(sorted(_UNITS, key=len, reverse=True)) + r")")
_WRAP = r"(?:\*\*|__|`|\*|_)?"
NUM = r"(?<![\w*`])" + _WRAP + r"(?<![\w])(\d+|" + _WORDNUM + r")" + _WRAP


def number_value(token):
    token = token.strip().lower()
    if token.isdigit():
        return int(token)
    if token in _UNITS:
        return _UNITS.index(token)
    parts = re.split(r"[- ]", token)
    if parts[0] in _TENS:
        return _TENS[parts[0]] + (_UNITS.index(parts[1]) if len(parts) > 1 else 0)
    return None


def _fig(pattern):
    return re.compile(pattern.replace(" ", r"\s+").replace("{N}", NUM), re.I)


REVIEWED_URLS = {
    ("xunit.abstractions", "2.0.3"): ("https://raw.githubusercontent.com/xunit/xunit/master/license.txt", "Apache-2.0")
}


def _bullet_after(body, match):
    end = re.search(r"\n[ \t]*(?:[-*+]\s|\n)|\n(?=Recommendation:)", body[match.end():])
    return body[match.start(): match.end() + (end.start() if end else len(body))]


def _named(text, pid, ver):
    return re.search(r"(?<![\w.])" + re.escape(pid) + r"\s+" + re.escape(ver) + r"(?![\w.])", text) is not None


def notice_count(listing, table, out):
    total = 0
    ids = {row[0] for row in table}
    for item in re.split(r",\s*(?:and\s+)?|\s+and\s+", " ".join(listing.split())):
        item = item.strip()
        if not item:
            continue
        group = re.fullmatch(r"the\s+" + NUM + r"\s+(\S+)\s+packages", item, re.I)
        if group:
            count = number_value(group.group(1))
            prefix = group.group(2)
            have = sum(1 for pid in ids if pid.startswith(prefix + "."))
            if count != have:
                out.append(msg("figure", "-", f"notice group {prefix}: page says {group.group(1)}, rows give {have}"))
            total += count
        elif item in ids:
            total += 1
        else:
            out.append(msg("figure", "-", f"notice files: {item} is no row"))
            total += 1
    return total


def figure_failures(page, locks):
    if lock_parse_failures(locks):
        return []
    body = restored_body(page)
    if body is None:
        return [msg("section", "-", "missing")]
    table = [row for row in page_rows(page) if len(row) >= 7]
    api = lock_packages((locks or {}).get("Orders.Api") or {})
    merged, _ = union_packages(locks)
    direct = sum(1 for item in merged.values() if item["direct"])
    licences = [row[3] for row in table]
    non_expression = sum(1 for row in table if row[4] != "nuspec licence expression")
    out = []

    def check(name, pattern, want, group=1):
        found = list(_fig(pattern).finditer(body))
        if not found:
            out.append(msg("figure", "-", f"{name}: not found on the page"))
            return []
        for match in found:
            got = number_value(match.group(group))
            expected = want(match) if callable(want) else want
            if got != expected:
                out.append(msg("figure", "-", f"{name}: page says {match.group(group)}, rows give {expected}"))
        return found

    check("packages", r"{N} packages: the generated service", len(merged))
    check("Orders.Api packages", r"\(Orders\.Api\) restores {N}\b", len(api))
    check("Orders.Specs packages", r"\(Orders\.Specs\) adds? {N} test-only", len(merged) - len(api))
    check("direct", r"{N} are direct", direct)
    check("transitive", r"direct and {N} transitive", len(merged) - direct)
    check("no licence expression", r"No licence expression in the nuspec \({N} packages?", non_expression)
    check("with a licence expression", r"the other {N} have one", len(table) - non_expression)
    check("MIT", r"{N} are MIT\b", licences.count("MIT"))
    check("Apache-2.0", r"{N} are Apache-2\.0", licences.count("Apache-2.0"))
    check("recommended packages", r"accept all {N} restored packages", len(merged))
    check("recommended MIT", r"restored packages \({N} MIT", licences.count("MIT"))
    check("recommended Apache-2.0", r"MIT, {N} Apache-2\.0", licences.count("Apache-2.0"))

    acceptance = list(_fig(r"Licence acceptance required \({N} packages?").finditer(body))
    if not acceptance:
        out.append(msg("figure", "-", "licence acceptance: not found on the page"))
    for match in acceptance:
        bullet = _bullet_after(body, match)
        named = sum(1 for row in table if _named(bullet, row[0], row[1]))
        check("licence acceptance", r"Licence acceptance required \({N} packages?", named)
        check("acceptance packages recommended", r"the {N} Microsoft test packages", named)
        break

    check(
        "notice files",
        r"{N} packages? also bundle a third-party notice file \(([^)]*)\)",
        lambda match: notice_count(match.group(2), table, out),
    )

    for (pid, ver), (url, _) in REVIEWED_URLS.items():
        line = next((ln for ln in body.split("\n") if _named(ln, pid, ver) and not ln.lstrip().startswith("|")), "")
        if url not in line:
            out.append(msg(pid, ver, f"page bullet does not cite the reviewed licence URL {url}"))

    return out


def classify_licence_text(text):
    head = " ".join((text or "")[:600].split())
    if re.search(r"\bMIT License\b", head, re.I):
        return "MIT"
    if re.search(r"\bApache License\b", head, re.I) and re.search(r"\bVersion 2\.0\b", head, re.I):
        return "Apache-2.0"
    match = re.search(r"\bBSD ([23])-Clause\b", head, re.I)
    return f"BSD-{match.group(1)}-Clause" if match else None


def licence_value_failures(table, evidence):
    out = []
    for row in table:
        if len(row) < 7:
            continue
        pid, ver, licence, source = row[0], row[1], row[3], row[4]
        ev = evidence.get((pid, ver))
        if ev is None:
            out.append(msg(pid, ver, "no restored package to read the licence from"))
            continue
        if source == "nuspec licence expression":
            if ev.get("expression") != licence:
                out.append(msg(pid, ver, f"licence {licence} but the nuspec expression is {ev.get('expression')}"))
        elif source == "packaged licence file":
            found = sorted({c for c in map(classify_licence_text, (ev.get("licence_files") or {}).values()) if c})
            if not found:
                out.append(msg(pid, ver, f"licence {licence} but no packaged licence file names a known licence"))
            elif licence not in found:
                out.append(msg(pid, ver, f"licence {licence} but the packaged licence file says {' and '.join(found)}"))
        elif source == "licence URL":
            url = (ev.get("licence_url") or "").strip()
            reviewed = REVIEWED_URLS.get((pid, ver))
            if reviewed is None or reviewed[0] != url:
                out.append(msg(pid, ver, f"licence {licence} from licence URL {url or '-'}, which is not a reviewed URL"))
            elif reviewed[1] != licence:
                out.append(msg(pid, ver, f"licence {licence} but the reviewed licence URL says {reviewed[1]}"))
    return out


NETWORK_MARKERS = (
    "NU1301",
    "Unable to load the service index",
    "No such host is known",
    "Name or service not known",
    "nodename nor servname provided",
    "The remote name could not be resolved",
)


def restore_outcome(exit_code, timed_out, text, restored_count):
    if timed_out:
        return "fail"
    if exit_code == 0:
        return "pass"
    if re.search(r"\bNU1004\b", text or "", re.I):
        return "mismatch"
    lower = (text or "").lower()
    if any(marker.lower() in lower for marker in NETWORK_MARKERS) and restored_count == 0:
        return "network-missing"
    if exit_code == 0:
        return "pass"
    return "fail"


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


# ---- Card 422 red (task-force role: red). Every Python row of .mob/records/422/corpus.txt, one test per row.
# Test422Characterisation passes on today's code and must keep passing after the green. Test422Pending fails on
# today's code and the green turns it passing. A Pending row that calls figure_failures, tool_closure or
# lockfile_commit_skip fails today with a NameError: those three check functions are new in the slice (design rules
# B1, I1 and O1), so that error is the expected red. The other Pending rows fail by assertion. The check code above
# is untouched. Rows that need no new function and already pass are in the characterisation class.
# The old two-letter prefix is never written whole in this file (the guard scans it).

CORPUS_422 = REPO / ".mob" / "records" / "422" / "corpus.txt"
EMBEDDED_422 = (
    REPO / "tests" / "test_restored_package_licences.py",
    REPO / "tests" / "Avowry.Acceptance.Tests" / "RestoredPackageLicences.cs",
    REPO / "tests" / "Avowry.Acceptance.Tests" / "RestoredPackageLicencesLockedRestore.cs",
    REPO / "tests" / "Avowry.Acceptance.Tests" / "NoPhCodesInTheLiveTree.cs",
)
TOOL_START_422 = "src/Avowry.Cli/Avowry.Cli.csproj"
SHALLOW_NOTE_422 = "commit-blob check skipped (shallow checkout)"


def page_inputs_422(edits):
    """The real page and lockfiles with a row's edits applied (a replacement is made at its first match inside the section)."""
    page, locks = read_page(), read_locks()
    section_at = page.index("## " + TITLE)
    for edit in edits:
        if edit == "@crlf":
            page = page.replace("\n", "\r\n").replace("\r\n", "\n")
        elif isinstance(edit, str) and edit.startswith("@lock "):
            _, name, _, pid, ver, kind = edit.split(" ")
            locks[name]["dependencies"]["net8.0"][pid] = {"type": kind, "resolved": ver, "contentHash": "A" * 88}
        else:
            old, new = edit
            at = page.find(old, section_at)
            if at < 0:
                raise ValueError("edit not found: " + old[:60])
            page = page[:at] + new + page[at + len(old):]
    return page, locks


def outcome_matches_422(expected, failures):
    """expected is pass, or fail: followed by substrings that must all appear (joined by ' && ')."""
    if expected == "pass":
        return not failures
    joined = "\n".join(failures)
    return bool(failures) and all(part in joined for part in expected[len("fail:"):].split(" && "))


class _Rows422:
    def settle(self, rid, expected, failures):
        self.assertTrue(outcome_matches_422(expected, failures), f"{rid}: expected {expected}, got {failures}")

    def offline(self, rid, edits, expected, figures=False):
        page, locks = page_inputs_422(edits)
        failures = collect_failures(page, locks, read_provider(), load_tool_texts())
        if figures:
            failures = failures + figure_failures(page, locks)
        self.settle(rid, expected, failures)

    def tools(self, rid, files, expected):
        def read(rel):
            if rel in files:
                return files[rel]
            path = REPO / rel
            return path.read_text(encoding="utf-8") if path.is_file() else None

        self.settle(rid, expected, tool_package_ref_failures(tool_closure(TOOL_START_422, read)))

    def tools_real(self, rid, expected):
        self.settle(rid, expected, tool_package_ref_failures(load_tool_texts()))

    def props(self, rid, text, expected):
        self.settle(rid, expected, props_failures(PROPS.read_text(encoding="utf-8") if text is None else text))

    def shallow(self, rid, shallow, hash_present, note):
        page = read_page()
        if not hash_present:
            page = HASH_RE.sub("x", page)
        got = lockfile_commit_skip(page, shallow)
        if note:
            self.assertEqual(msg("section", "-", SHALLOW_NOTE_422), got, rid)
        else:
            self.assertIsNone(got, rid)

class Test422Characterisation(_Rows422, unittest.TestCase):
    def test_422_L_gpl(self):
        # L-gpl [422-3] page-licence/table-cell: 369 B3-outside near miss on the real page
        self.offline("L-gpl", [
            ("| FluentValidation | 12.1.1 | Transitive | Apache-2.0 | nuspec licence expression | generated service and generated specs | ok |", "| FluentValidation | 12.1.1 | Transitive | GPL-2.0 | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FluentValidation 12.1.1: needs review", figures=False)

    def test_422_L_bold(self):
        # L-bold [422-3] page-licence/bold: cell markup is not a licence id (fail-closed, unchanged)
        self.offline("L-bold", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | **MIT** | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: needs review", figures=False)

    def test_422_L_code(self):
        # L-code [422-3] page-licence/code-span: cell markup (fail-closed, unchanged)
        self.offline("L-code", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | `MIT` | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: needs review", figures=False)

    def test_422_L_inner_ws(self):
        # L-inner-ws [422-3] page-licence/inner-ws: cell padding is trimmed (unchanged)
        self.offline("L-inner-ws", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct |    MIT\t| nuspec licence expression | generated service and generated specs | ok |"),
        ], "pass", figures=False)

    def test_422_L_source_label(self):
        # L-source-label [422-3] page-licence/table-cell: 369 L4 near miss
        self.offline("L-source-label", [
            ("| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence URL | generated specs (test-only) | ok |", "| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence-url | generated specs (test-only) | ok |"),
        ], "fail:xunit.abstractions 2.0.3: licence source is not one of the three labels", figures=False)

    def test_422_L_indent4(self):
        # L-indent4 [422-3] page-licence/leading-ws: 4 spaces is a code block, not a row (unchanged)
        self.offline("L-indent4", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n    | FastEndpoints | 8.3.0 | Direct | GPL-3.0 | nuspec licence expression | generated service and generated specs | ok |"),
        ], "pass", figures=False)

    def test_422_L_row_wrapped(self):
        # L-row-wrapped [422-3] page-licence/line-break: a row broken over two lines (unchanged)
        self.offline("L-row-wrapped", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct\n| MIT | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: row missing licence", figures=False)

    def test_422_L_E1(self):
        # L-E1 [422-3] page-licence/table-cell: 369 E1 on the real page
        self.offline("L-E1", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n", ""),
        ], "fail:FastEndpoints 8.3.0: missing from the page", figures=False)

    def test_422_L_E2(self):
        # L-E2 [422-3] page-licence/table-cell: 369 E2
        self.offline("L-E2", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 9.0.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 9.0.0: listed at a different version than 8.3.0", figures=False)

    def test_422_L_E3(self):
        # L-E3 [422-3] page-licence/table-cell: 369 E3
        self.offline("L-E3", [
            ("| Newtonsoft.Json | 13.0.1 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |", "| Newtonsoft.Json | 13.0.1 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |\n| ExtraPackage | 1.0.0 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |"),
        ], "fail:ExtraPackage 1.0.0: in neither lockfile", figures=False)

    def test_422_L_kind(self):
        # L-kind [422-3] page-licence/table-cell: 369 L3
        self.offline("L-kind", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Transitive | MIT | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: kind Transitive expected Direct", figures=False)

    def test_422_L_crlf(self):
        # L-crlf [422-3] page-licence/line-break: CRLF page (read_page normalises)
        self.offline("L-crlf", [
            "@crlf",
        ], "pass", figures=False)

    def test_422_Z_tools(self):
        # Z-tools [422-1] tools/plain: src/Avowry.Cli, Avowry.Compiler, Avowry.Adapters.CSharp csproj and root Directory.Build.props (today)
        self.tools_real("Z-tools", "pass")

    def test_422_Z_props(self):
        # Z-props [422-1] props/plain: reference/Directory.Build.props (today)
        self.props("Z-props", None, "pass")

    def test_422_S_true(self):
        # S-true [422-5] props/condition-true: Condition="true"
        self.props("S-true", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"true\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_true_compare(self):
        # S-true-compare [422-5] props/condition-true: a true comparison
        self.props("S-true-compare", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\" 'true' == 'TRUE' \">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_true_wrapped(self):
        # S-true-wrapped [422-5] props/line-break: a comparison over two lines
        self.props("S-true-wrapped", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"'a'\n      == 'a'\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_empty_cond(self):
        # S-empty-cond [422-5] props/condition-true: an empty Condition is true
        self.props("S-empty-cond", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_plain(self):
        # S-plain [422-5] props/plain: card [S] unconditioned group
        self.props("S-plain", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_both(self):
        # S-both [422-5] props/condition-false: a false group then an unconditioned one
        self.props("S-both", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"false\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_override_false(self):
        # S-override-false [422-5] props/condition-false: a later reset under a false Condition does not apply
        self.props("S-override-false", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <PropertyGroup Condition=\"false\">\n    <RestoreLockedMode>false</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_import_before(self):
        # S-import-before [422-5] props/plain: an Import before the properties
        self.props("S-import-before", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <Import Project=\"../shared.props\" />\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_import_sdk(self):
        # S-import-sdk [422-5] props/plain: an Sdk Import after
        self.props("S-import-sdk", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <Import Project=\"Sdk.targets\" Sdk=\"Microsoft.NET.Sdk\" />\n</Project>\n", "pass")

    def test_422_S_case(self):
        # S-case [422-5] props/case: True in another case
        self.props("S-case", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>True</RestorePackagesWithLockFile>\n    <RestoreLockedMode>TRUE</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_value_ws(self):
        # S-value-ws [422-5] props/line-break: the value on its own indented line
        self.props("S-value-ws", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>\n      true\n    </RestorePackagesWithLockFile>\n    <RestoreLockedMode>\ttrue </RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_value_prop(self):
        # S-value-prop [422-5] props/plain: a property reference as the value
        self.props("S-value-prop", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>$(Locked)</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestoreLockedMode -: not true")

    def test_422_S_cdata(self):
        # S-cdata [422-5] props/cdata: value in CDATA
        self.props("S-cdata", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile><![CDATA[true]]></RestorePackagesWithLockFile>\n    <RestoreLockedMode><![CDATA[true]]></RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_entity(self):
        # S-entity [422-5] props/entity: value through an entity
        self.props("S-entity", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>&#116;rue</RestorePackagesWithLockFile>\n    <RestoreLockedMode>tru&#x65;</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "pass")

    def test_422_S_broken(self):
        # S-broken [422-5] props/plain: not XML (unchanged)
        self.props("S-broken", "<Project><PropertyGroup>", "fail:Directory.Build.props -: does not parse")

    def test_422_every_corpus_row_is_embedded(self):
        # Every row id of the card 422 corpus appears, quoted, in this file or in one of the three C# files.
        self.assertTrue(CORPUS_422.is_file(), "missing " + CORPUS_422.as_posix())
        ids = []
        for line in CORPUS_422.read_text(encoding="utf-8").split("\n"):
            if line and not line.startswith("#"):
                ids.append(line.split("\t")[0])
        self.assertEqual(len(ids), len(set(ids)), "duplicate row ids in the corpus")
        embedded = "".join(p.read_text(encoding="utf-8") for p in EMBEDDED_422 if p.is_file())
        missing = [i for i in ids if '"' + i + '"' not in embedded]
        self.assertEqual([], missing, f"{len(missing)} of {len(ids)} corpus rows are not embedded")


class Test422Pending(_Rows422, unittest.TestCase):
    def test_422_Z_page(self):
        # Z-page [422-1] page-licence/table-cell: docs/release/pre-publication-checks.md (today)
        self.offline("Z-page", [], "pass", figures=True)

    def test_422_E_xa_mit_off(self):
        # E-xa-mit-off [422-2] page-licence/table-cell: card [E] offline half (the figures see a one-way change)
        self.offline("E-xa-mit-off", [
            ("| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence URL | generated specs (test-only) | ok |", "| xunit.abstractions | 2.0.3 | Transitive | MIT | licence URL | generated specs (test-only) | ok |"),
        ], "fail:MIT: page says 21, rows give 22 && Apache-2.0: page says 9, rows give 8", figures=True)

    def test_422_E_swap_off(self):
        # E-swap-off [422-2] page-licence/table-cell: card [E]: the swap keeps every offline figure, so only the network sees it
        self.offline("E-swap-off", [
            ("| xunit.abstractions | 2.0.3 | Transitive | Apache-2.0 | licence URL | generated specs (test-only) | ok |", "| xunit.abstractions | 2.0.3 | Transitive | MIT | licence URL | generated specs (test-only) | ok |"),
            ("| System.Reflection.Metadata | 1.6.0 | Transitive | MIT | packaged licence file | generated specs (test-only) | ok |", "| System.Reflection.Metadata | 1.6.0 | Transitive | Apache-2.0 | packaged licence file | generated specs (test-only) | ok |"),
        ], "pass", figures=True)

    def test_422_L_gpl_flagged(self):
        # L-gpl-flagged [422-3] page-licence/table-cell: 369 B3-flagged near miss: flagged, the none line stays, so the figures and none line catch it
        self.offline("L-gpl-flagged", [
            ("| FluentValidation | 12.1.1 | Transitive | Apache-2.0 | nuspec licence expression | generated service and generated specs | ok |", "| FluentValidation | 12.1.1 | Transitive | GPL-2.0 | nuspec licence expression | generated service and generated specs | needs review |"),
        ], "fail:Apache-2.0: page says 9, rows give 8", figures=True)

    def test_422_L_indent_dup(self):
        # L-indent-dup [422-3] page-licence/leading-ws: a 2-space indented row still renders as a row (changed: V0 does not see it)
        self.offline("L-indent-dup", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n  | FastEndpoints | 8.3.0 | Direct | GPL-3.0 | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: appears more than once", figures=False)

    def test_422_L_indent3_dup(self):
        # L-indent3-dup [422-3] page-licence/leading-ws: 3 spaces still a row
        self.offline("L-indent3-dup", [
            ("| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |", "| FastEndpoints | 8.3.0 | Direct | MIT | nuspec licence expression | generated service and generated specs | ok |\n   | FastEndpoints | 8.3.0 | Direct | GPL-3.0 | nuspec licence expression | generated service and generated specs | ok |"),
        ], "fail:FastEndpoints 8.3.0: appears more than once", figures=False)

    def test_422_M_add(self):
        # M-add [422-3] page-figure/digits: card [M]: a package added to a lockfile and the rows, no figure updated
        self.offline("M-add", [
            "@lock Orders.Specs add Serilog 4.0.0 Transitive",
            ("| Newtonsoft.Json | 13.0.1 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |", "| Newtonsoft.Json | 13.0.1 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |\n| Serilog | 4.0.0 | Transitive | MIT | nuspec licence expression | generated specs (test-only) | ok |"),
        ], "fail:packages: page says 30, rows give 31 && Orders.Specs packages: page says 14, rows give 15 && transitive: page says 26, rows give 27 && MIT: page says 21, rows give 22", figures=True)

    def test_422_B_total(self):
        # B-total [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-total", [
            ("30 packages:", "31 packages:"),
        ], "fail:packages: page says 31, rows give 30", figures=True)

    def test_422_B_api(self):
        # B-api [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-api", [
            ("restores 16", "restores 17"),
        ], "fail:Orders.Api packages: page says 17, rows give 16", figures=True)

    def test_422_B_specs(self):
        # B-specs [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-specs", [
            ("add 14 test-only", "add 13 test-only"),
        ], "fail:Orders.Specs packages: page says 13, rows give 14", figures=True)

    def test_422_B_direct(self):
        # B-direct [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-direct", [
            ("4 are direct", "5 are direct"),
        ], "fail:direct: page says 5, rows give 4", figures=True)

    def test_422_B_trans(self):
        # B-trans [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-trans", [
            ("26 transitive", "25 transitive"),
        ], "fail:transitive: page says 25, rows give 26", figures=True)

    def test_422_B_mit(self):
        # B-mit [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-mit", [
            ("21 are MIT and 9 are Apache-2.0", "22 are MIT and 9 are Apache-2.0"),
        ], "fail:MIT: page says 22, rows give 21", figures=True)

    def test_422_B_apache(self):
        # B-apache [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-apache", [
            ("21 are MIT and 9 are Apache-2.0", "21 are MIT and 8 are Apache-2.0"),
        ], "fail:Apache-2.0: page says 8, rows give 9", figures=True)

    def test_422_B_notice(self):
        # B-notice [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-notice", [
            ("12 packages also bundle a third-party notice file", "13 packages also bundle a third-party notice file"),
        ], "fail:notice files: page says 13, rows give 12", figures=True)

    def test_422_B_noexpr(self):
        # B-noexpr [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-noexpr", [
            ("No licence expression in the nuspec (2 packages; the other 28 have one)", "No licence expression in the nuspec (3 packages; the other 28 have one)"),
        ], "fail:no licence expression: page says 3, rows give 2", figures=True)

    def test_422_B_withexpr(self):
        # B-withexpr [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-withexpr", [
            ("No licence expression in the nuspec (2 packages; the other 28 have one)", "No licence expression in the nuspec (2 packages; the other 27 have one)"),
        ], "fail:with a licence expression: page says 27, rows give 28", figures=True)

    def test_422_B_acc(self):
        # B-acc [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-acc", [
            ("Licence acceptance required (4 packages,", "Licence acceptance required (5 packages,"),
        ], "fail:licence acceptance: page says 5, rows give 4", figures=True)

    def test_422_B_acc_word(self):
        # B-acc-word [422-3] page-figure/words: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-acc-word", [
            ("the four Microsoft test packages", "the five Microsoft test packages"),
        ], "fail:acceptance packages recommended: page says five, rows give 4", figures=True)

    def test_422_B_group(self):
        # B-group [422-3] page-figure/words: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-group", [
            ("the eight Microsoft.Extensions packages", "the seven Microsoft.Extensions packages"),
        ], "fail:notice group Microsoft.Extensions: page says seven, rows give 8 && notice files: page says 12, rows give 11", figures=True)

    def test_422_B_rec_total(self):
        # B-rec-total [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-rec-total", [
            ("accept all 30 restored packages (21 MIT, 9 Apache-2.0, none marked needs review)", "accept all 29 restored packages (21 MIT, 9 Apache-2.0, none marked needs review)"),
        ], "fail:recommended packages: page says 29, rows give 30", figures=True)

    def test_422_B_rec_mit(self):
        # B-rec-mit [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-rec-mit", [
            ("accept all 30 restored packages (21 MIT, 9 Apache-2.0, none marked needs review)", "accept all 30 restored packages (20 MIT, 9 Apache-2.0, none marked needs review)"),
        ], "fail:recommended MIT: page says 20, rows give 21", figures=True)

    def test_422_B_rec_apache(self):
        # B-rec-apache [422-3] page-figure/digits: card [B]: figure off by one; docs/release/pre-publication-checks.md
        self.offline("B-rec-apache", [
            ("accept all 30 restored packages (21 MIT, 9 Apache-2.0, none marked needs review)", "accept all 30 restored packages (21 MIT, 10 Apache-2.0, none marked needs review)"),
        ], "fail:recommended Apache-2.0: page says 10, rows give 9", figures=True)

    def test_422_B_acc_named(self):
        # B-acc-named [422-3] page-figure/digits: the acceptance count is the packages the bullet names
        self.offline("B-acc-named", [
            ("Microsoft.TestPlatform.ObjectModel 17.11.1 and ", ""),
        ], "fail:licence acceptance: page says 4, rows give 3", figures=True)

    def test_422_W_words(self):
        # W-words [422-3] page-figure/words: prose number as a word
        self.offline("W-words", [
            ("30 packages:", "Thirty packages:"),
        ], "pass", figures=True)

    def test_422_W_words_hyphen(self):
        # W-words-hyphen [422-3] page-figure/words: compound number word
        self.offline("W-words-hyphen", [
            ("21 are MIT and 9 are Apache-2.0", "twenty-one are MIT and nine are Apache-2.0"),
        ], "pass", figures=True)

    def test_422_W_words_wrong(self):
        # W-words-wrong [422-3] page-figure/words: word, off by one
        self.offline("W-words-wrong", [
            ("30 packages:", "thirty-one packages:"),
        ], "fail:packages: page says thirty-one, rows give 30", figures=True)

    def test_422_W_bold(self):
        # W-bold [422-3] page-figure/bold: bold figure
        self.offline("W-bold", [
            ("30 packages:", "**30** packages:"),
        ], "pass", figures=True)

    def test_422_W_bold_wrong(self):
        # W-bold-wrong [422-3] page-figure/bold: bold figure off by one
        self.offline("W-bold-wrong", [
            ("30 packages:", "**31** packages:"),
        ], "fail:packages: page says 31, rows give 30", figures=True)

    def test_422_W_code(self):
        # W-code [422-3] page-figure/code-span: figure in a code span
        self.offline("W-code", [
            ("restores 16", "restores `16`"),
        ], "pass", figures=True)

    def test_422_W_code_wrong(self):
        # W-code-wrong [422-3] page-figure/code-span: code span off by one
        self.offline("W-code-wrong", [
            ("restores 16", "restores `17`"),
        ], "fail:Orders.Api packages: page says 17, rows give 16", figures=True)

    def test_422_W_wrapped(self):
        # W-wrapped [422-3] page-figure/line-break: the figures paragraph wrapped at other words
        self.offline("W-wrapped", [
            ("30 packages: the generated service (Orders.Api) restores 16, and the generated specs (Orders.Specs) add 14 test-only packages; 4 are direct and 26 transitive.", "30\npackages: the generated service (Orders.Api) restores\n16, and the generated specs (Orders.Specs) add 14 test-only packages; 4 are\ndirect and 26 transitive."),
        ], "pass", figures=True)

    def test_422_W_wrapped_wrong(self):
        # W-wrapped-wrong [422-3] page-figure/line-break: wrapped and off by one
        self.offline("W-wrapped-wrong", [
            ("30 packages: the generated service (Orders.Api) restores 16, and the generated specs (Orders.Specs) add 14 test-only packages; 4 are direct and 26 transitive.", "30 packages: the generated service (Orders.Api) restores 16, and the generated specs (Orders.Specs) add 14 test-only packages; 5\nare direct and 26 transitive."),
        ], "fail:direct: page says 5, rows give 4", figures=True)

    def test_422_W_inner(self):
        # W-inner [422-3] page-figure/inner-ws: double spaces and a tab between words
        self.offline("W-inner", [
            ("21 are MIT and 9 are Apache-2.0", "21  are\tMIT and 9  are Apache-2.0"),
        ], "pass", figures=True)

    def test_422_W_leading(self):
        # W-leading [422-3] page-figure/leading-ws: the none-line bullet indented
        self.offline("W-leading", [
            ("- No package is outside", "  - No package is outside"),
        ], "pass", figures=True)

    def test_422_W_gone(self):
        # W-gone [422-3] page-figure/digits: a figure reworded away (fail-closed)
        self.offline("W-gone", [
            ("21 are MIT and 9 are Apache-2.0", "most are MIT, the rest Apache-2.0"),
        ], "fail:MIT: not found on the page", figures=True)

    def test_422_W_dup_wrong(self):
        # W-dup-wrong [422-3] page-figure/digits: the figure repeated, the second copy wrong
        self.offline("W-dup-wrong", [
            ("Recommendation: accept", "In short, 31 packages: the generated service and specs.\n\nRecommendation: accept"),
        ], "fail:packages: page says 31, rows give 30", figures=True)

    def test_422_W_notice_unknown(self):
        # W-notice-unknown [422-3] page-figure/digits: a notice list item that names no row
        self.offline("W-notice-unknown", [
            ("(Microsoft.CodeCoverage, the eight", "(Microsoft.CodeCoverag, the eight"),
        ], "fail:notice files: Microsoft.CodeCoverag is no row", figures=True)

    def test_422_W_url(self):
        # W-url [422-3] page-figure/code-span: the URL string (second review finding 2)
        self.offline("W-url", [
            ("xunit/master/license.txt", "xunit/main/license.txt"),
        ], "fail:xunit.abstractions 2.0.3: page bullet does not cite the reviewed licence URL", figures=True)

    def test_422_I_E5(self):
        # I-E5 [422-4] tools/plain: 369 E5 near miss in the real Compiler project
        self.tools("I-E5", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_order(self):
        # I-order [422-4] tools/attr-order: card [I] attributes in the other order
        self.tools("I-order", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Version=\"13.0.1\" Include=\"Newtonsoft.Json\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_child(self):
        # I-child [422-4] tools/child-element: card [I] version as a child element
        self.tools("I-child", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\">\n      <Version>13.0.1</Version>\n    </PackageReference>\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_child_cdata(self):
        # I-child-cdata [422-4] tools/cdata: child Version in CDATA
        self.tools("I-child-cdata", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\"><Version><![CDATA[13.0.1]]></Version></PackageReference>\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_entity(self):
        # I-entity [422-4] tools/entity: an entity in the id
        self.tools("I-entity", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft&#46;Json\" Version=\"13&#x2E;0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_single(self):
        # I-single [422-4] tools/single-quote: single-quoted attributes
        self.tools("I-single", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include='Newtonsoft.Json' Version='13.0.1' />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_update(self):
        # I-update [422-4] tools/update: card [I] Update=
        self.tools("I-update", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Update=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in")

    def test_422_I_update_child(self):
        # I-update-child [422-4] tools/update: Update= with a child Version
        self.tools("I-update-child", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Update=\"Newtonsoft.Json\"><Version>13.0.1</Version></PackageReference>\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_list(self):
        # I-list [422-4] tools/plain: an item list a;b
        self.tools("I-list", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json;Serilog\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in && Serilog 13.0.1: PackageReference in")

    def test_422_I_lower(self):
        # I-lower [422-4] tools/case: item type in another case (MSBuild item types ignore case)
        self.tools("I-lower", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <packagereference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_multiline(self):
        # I-multiline [422-4] tools/line-break: attributes on their own lines
        self.tools("I-multiline", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference\n        Include=\"Newtonsoft.Json\"\n        Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_inner_ws(self):
        # I-inner-ws [422-4] tools/inner-ws: spaces round = and two spaces between attributes
        self.tools("I-inner-ws", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference  Include = \"Newtonsoft.Json\"   Version = \"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_value_ws(self):
        # I-value-ws [422-4] tools/leading-ws: leading and trailing spaces in the values
        self.tools("I-value-ws", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\" Newtonsoft.Json \" Version=\" 13.0.1 \" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_tab(self):
        # I-tab [422-4] tools/inner-ws: tab between attributes
        self.tools("I-tab", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n\t<PackageReference\tInclude=\"Newtonsoft.Json\"\tVersion=\"13.0.1\"/>\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_comment(self):
        # I-comment [422-4] tools/comment: a commented-out reference restores nothing (changed: V0 fails it)
        self.tools("I-comment", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <!-- <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" /> -->\n  </ItemGroup>\n</Project>\n",
        }, "pass")

    def test_422_I_cdata_text(self):
        # I-cdata-text [422-4] tools/cdata: reference text inside a CDATA property value is not an item (changed)
        self.tools("I-cdata-text", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n    <Notes><![CDATA[<PackageReference Include=\"X\" Version=\"1.0.0\" />]]></Notes>\n  </PropertyGroup>\n</Project>\n",
        }, "pass")

    def test_422_I_remove(self):
        # I-remove [422-4] tools/plain: Remove= takes a package away
        self.tools("I-remove", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Remove=\"Newtonsoft.Json\" />\n  </ItemGroup>\n</Project>\n",
        }, "pass")

    def test_422_I_noversion(self):
        # I-noversion [422-4] tools/plain: central version management: no Version
        self.tools("I-noversion", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json -: PackageReference in")

    def test_422_I_override(self):
        # I-override [422-4] tools/attr-order: VersionOverride
        self.tools("I-override", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" VersionOverride=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_cond_false(self):
        # I-cond-false [422-4] tools/condition-false: a reference under a false Condition is still reported (strict; ruling R-2)
        self.tools("I-cond-false", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup Condition=\"false\">\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Compiler/Avowry.Compiler.csproj")

    def test_422_I_download(self):
        # I-download [422-4] tools/plain: PackageDownload restores a package too
        self.tools("I-download", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n    <ProjectReference Include=\"../Avowry.Adapters.CSharp/Avowry.Adapters.CSharp.csproj\" />\n    <PackageDownload Include=\"Newtonsoft.Json\" Version=\"[13.0.1]\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json [13.0.1]: PackageDownload in src/Avowry.Cli/Avowry.Cli.csproj")

    def test_422_I_root_props(self):
        # I-root-props [422-4] tools/directory-build-props: card [I] root Directory.Build.props (real file plus a reference)
        self.tools("I-root-props", {
            "Directory.Build.props": "<Project>\n  <PropertyGroup>\n    <TargetFramework>net8.0</TargetFramework>\n    <Nullable>enable</Nullable>\n    <ImplicitUsings>enable</ImplicitUsings>\n    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in Directory.Build.props")

    def test_422_I_root_targets(self):
        # I-root-targets [422-4] tools/directory-build-targets: card [I] root Directory.Build.targets
        self.tools("I-root-targets", {
            "Directory.Build.targets": "<Project>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in Directory.Build.targets")

    def test_422_I_src_props(self):
        # I-src-props [422-4] tools/directory-build-props: a nearer Directory.Build.props
        self.tools("I-src-props", {
            "src/Directory.Build.props": "<Project>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Directory.Build.props")

    def test_422_I_global(self):
        # I-global [422-4] tools/directory-packages-props: GlobalPackageReference in central package management
        self.tools("I-global", {
            "Directory.Packages.props": "<Project>\n  <ItemGroup>\n    <GlobalPackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: GlobalPackageReference in Directory.Packages.props")

    def test_422_I_pkgversion(self):
        # I-pkgversion [422-4] tools/directory-packages-props: PackageVersion alone restores nothing
        self.tools("I-pkgversion", {
            "Directory.Packages.props": "<Project>\n  <ItemGroup>\n    <PackageVersion Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "pass")

    def test_422_I_reference_props(self):
        # I-reference-props [422-4] tools/directory-build-props: reference/ props are not on the tool's path
        self.tools("I-reference-props", {
            "reference/Directory.Build.targets": "<Project>\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "pass")

    def test_422_I_projref_single(self):
        # I-projref-single [422-4] tools/single-quote: a ProjectReference in single quotes
        self.tools("I-projref-single", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n    <ProjectReference Include='../Avowry.Extra/Avowry.Extra.csproj' />\n  </ItemGroup>\n</Project>\n",
            "src/Avowry.Extra/Avowry.Extra.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Extra/Avowry.Extra.csproj")

    def test_422_I_projref_backslash(self):
        # I-projref-backslash [422-4] tools/plain: a ProjectReference with backslashes (Windows form)
        self.tools("I-projref-backslash", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n    <ProjectReference Include=\"..\\Avowry.Extra\\Avowry.Extra.csproj\" />\n  </ItemGroup>\n</Project>\n",
            "src/Avowry.Extra/Avowry.Extra.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Extra/Avowry.Extra.csproj")

    def test_422_I_projref_order(self):
        # I-projref-order [422-4] tools/attr-order: a ProjectReference with another attribute first
        self.tools("I-projref-order", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n    <ProjectReference Condition=\"true\" Include=\"../Avowry.Extra/Avowry.Extra.csproj\" />\n  </ItemGroup>\n</Project>\n",
            "src/Avowry.Extra/Avowry.Extra.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Extra/Avowry.Extra.csproj")

    def test_422_I_projref_plain(self):
        # I-projref-plain [422-4] tools/plain: 369 closure near miss
        self.tools("I-projref-plain", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n    <ProjectReference Include=\"../Avowry.Extra/Avowry.Extra.csproj\" />\n  </ItemGroup>\n</Project>\n",
            "src/Avowry.Extra/Avowry.Extra.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" />\n  </ItemGroup>\n</Project>\n",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in src/Avowry.Extra/Avowry.Extra.csproj")

    def test_422_I_import(self):
        # I-import [422-4] tools/plain: a literal Import
        self.tools("I-import", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <Import Project=\"../../build/packages.props\" />\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n  </ItemGroup>\n</Project>\n",
            "build/packages.props": "<Project><ItemGroup><PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" /></ItemGroup></Project>",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in build/packages.props")

    def test_422_I_import_thisdir(self):
        # I-import-thisdir [422-4] tools/plain: an Import through $(MSBuildThisFileDirectory)
        self.tools("I-import-thisdir", {
            "Directory.Build.props": "<Project>\n  <PropertyGroup>\n    <TargetFramework>net8.0</TargetFramework>\n    <Nullable>enable</Nullable>\n    <ImplicitUsings>enable</ImplicitUsings>\n    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>\n  </PropertyGroup>\n  <Import Project=\"$(MSBuildThisFileDirectory)build/packages.props\" />\n</Project>\n",
            "build/packages.props": "<Project><ItemGroup><PackageReference Include=\"Newtonsoft.Json\" Version=\"13.0.1\" /></ItemGroup></Project>",
        }, "fail:Newtonsoft.Json 13.0.1: PackageReference in build/packages.props")

    def test_422_I_import_unknown(self):
        # I-import-unknown [422-4] tools/plain: an Import the check cannot resolve (fail-closed)
        self.tools("I-import-unknown", {
            "Directory.Build.props": "<Project>\n  <PropertyGroup>\n    <TargetFramework>net8.0</TargetFramework>\n    <Nullable>enable</Nullable>\n    <ImplicitUsings>enable</ImplicitUsings>\n    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>\n  </PropertyGroup>\n  <Import Project=\"$(SharedProps)/packages.props\" />\n</Project>\n",
        }, "fail:Directory.Build.props -: Import $(SharedProps)/packages.props cannot be followed")

    def test_422_I_import_sdk(self):
        # I-import-sdk [422-4] tools/plain: an Sdk Import is not followed
        self.tools("I-import-sdk", {
            "src/Avowry.Cli/Avowry.Cli.csproj": "<Project>\n  <Import Project=\"Sdk.props\" Sdk=\"Microsoft.NET.Sdk\" />\n  <ItemGroup>\n    <ProjectReference Include=\"../Avowry.Compiler/Avowry.Compiler.csproj\" />\n  </ItemGroup>\n  <Import Project=\"Sdk.targets\" Sdk=\"Microsoft.NET.Sdk\" />\n</Project>\n",
        }, "pass")

    def test_422_I_broken(self):
        # I-broken [422-4] tools/plain: a tool project that is not XML (fail-closed)
        self.tools("I-broken", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project><ItemGroup>\n",
        }, "fail:src/Avowry.Compiler/Avowry.Compiler.csproj -: does not parse as MSBuild XML")

    def test_422_I_framework(self):
        # I-framework [422-4] tools/plain: FrameworkReference is not a package
        self.tools("I-framework", {
            "src/Avowry.Compiler/Avowry.Compiler.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <AssemblyName>Avowry.Compiler</AssemblyName>\n    <RootNamespace>Avowry.Compiler</RootNamespace>\n  </PropertyGroup>\n  <ItemGroup>\n    <FrameworkReference Include=\"Microsoft.AspNetCore.App\" />\n  </ItemGroup>\n</Project>\n",
        }, "pass")

    def test_422_S_false(self):
        # S-false [422-5] props/condition-false: card [S]
        self.props("S-false", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"false\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (set only under a false Condition)")

    def test_422_S_false_compare(self):
        # S-false-compare [422-5] props/condition-false: card [S] a false comparison
        self.props("S-false-compare", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"'a' == 'b'\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_false_ne(self):
        # S-false-ne [422-5] props/condition-false: a false !=
        self.props("S-false-ne", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"'x' != 'x'\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_false_single(self):
        # S-false-single [422-5] props/single-quote: Condition attribute in single quotes
        self.props("S-false-single", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition='false'>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_unknown(self):
        # S-unknown [422-5] props/condition-unknown: a real MSBuild default idiom
        self.props("S-unknown", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"'$(RestoreLockedMode)' == ''\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (set under a Condition the check cannot decide)")

    def test_422_S_unknown_and(self):
        # S-unknown-and [422-5] props/condition-unknown: a compound Condition
        self.props("S-unknown-and", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"'a' == 'a' and 'b' == 'b'\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_elem_false(self):
        # S-elem-false [422-5] props/condition-false: a false Condition on one property
        self.props("S-elem-false", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode Condition=\"false\">true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestoreLockedMode -: not true")

    def test_422_S_override(self):
        # S-override [422-5] props/plain: a later group sets it false (last assignment wins; changed)
        self.props("S-override", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <PropertyGroup>\n    <RestoreLockedMode>false</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestoreLockedMode -: not true (last set to false)")

    def test_422_S_override_unknown(self):
        # S-override-unknown [422-5] props/condition-unknown: a later group under an unknown Condition
        self.props("S-override-unknown", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <PropertyGroup Condition=\"'$(CI)' == 'true'\">\n    <RestoreLockedMode>false</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestoreLockedMode -: not true")

    def test_422_S_target(self):
        # S-target [422-5] props/plain: properties inside a Target are not evaluation properties (changed)
        self.props("S-target", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <Target Name=\"X\">\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  </Target>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_choose(self):
        # S-choose [422-5] props/condition-unknown: inside Choose/When (changed)
        self.props("S-choose", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <Choose>\n    <When Condition=\"true\">\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n    </When>\n  </Choose>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_import_after(self):
        # S-import-after [422-5] props/plain: an Import after the properties
        self.props("S-import-after", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  <Import Project=\"../shared.props\" />\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (a later Import may change it)")

    def test_422_S_comment(self):
        # S-comment [422-5] props/comment: commented-out properties (unchanged)
        self.props("S-comment", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <!--\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n  -->\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (never set)")

    def test_422_S_cond_entity(self):
        # S-cond-entity [422-5] props/entity: Condition written with entities (&apos;a&apos;==&apos;b&apos;)
        self.props("S-cond-entity", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup Condition=\"&apos;a&apos;==&apos;b&apos;\">\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n    <RestoreLockedMode>true</RestoreLockedMode>\n  </PropertyGroup>\n</Project>\n", "fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true")

    def test_422_S_missing(self):
        # S-missing [422-5] props/plain: one property absent
        self.props("S-missing", "<Project>\n  <!-- Stops inheritance of the repo root props: this folder mimics standalone generated output. -->\n  <PropertyGroup>\n    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>\n  </PropertyGroup>\n</Project>\n", "fail:RestoreLockedMode -: not true (never set)")

    def test_422_O_shallow(self):
        # O-shallow [422-6] restore/plain: card [O] a shallow checkout reports the skip
        self.shallow("O-shallow", shallow=True, hash_present=True, note=True)

    def test_422_O_deep(self):
        # O-deep [422-6] restore/plain: a full checkout runs the check
        self.shallow("O-deep", shallow=False, hash_present=True, note=False)

    def test_422_O_shallow_nohash(self):
        # O-shallow-nohash [422-6] restore/plain: no commit on the page: nothing to skip (L5 meta fails instead)
        self.shallow("O-shallow-nohash", shallow=True, hash_present=False, note=False)


if __name__ == "__main__":
    unittest.main()
