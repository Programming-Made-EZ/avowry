#!/usr/bin/env python3
"""Check card 234 criterion 3 restart record shape and references."""
from __future__ import annotations
import argparse, json, re, subprocess, sys
from pathlib import Path
RECORD_REL = Path(".mob/records/234/record-in-progress.json")
INDEX_REL = Path("docs/process-wiki/INDEX.md")
BASELINE_TOKENS = 965000
HASH_RE = re.compile(r"^[0-9a-f]{7,40}$")
BOUNDARY_RE = re.compile(r"^B(\d+)$")
MARKER_LINE_RE = re.compile(r"^Boundary\s+([^:\s]+)\s*:\s*([0-9a-f]{7,40})\s*$")
def is_token(value: object) -> bool:
    return isinstance(value, int) and not isinstance(value, bool) and value >= 1
def rel(path: Path) -> str:
    return path.as_posix()
def read_text_utf8(path: Path) -> tuple[str | None, bool]:
    try:
        return path.read_text(encoding="utf-8"), True
    except (OSError, UnicodeDecodeError):
        return None, False
def resolve_source(root: Path, source: object) -> tuple[Path | None, str | None]:
    if not isinstance(source, str) or not source.strip():
        return None, "missing"
    try:
        target = (root / Path(source.strip())).resolve()
    except OSError:
        return None, "missing"
    try:
        target.relative_to(root.resolve())
    except ValueError:
        return None, "outside"
    if not target.is_file():
        return None, "missing"
    return target, None
def parse_index_status(root: Path, findings: list[str]) -> dict[str, str]:
    path = root / INDEX_REL
    out: dict[str, str] = {}
    if not path.is_file():
        findings.append(f"{rel(INDEX_REL)}: no index")
        return out
    text, ok = read_text_utf8(path)
    if not ok or text is None:
        findings.append(f"{rel(INDEX_REL)}: not valid UTF-8")
        return out
    for line in text.splitlines():
        if not line.strip().startswith("|"):
            continue
        parts = [p.strip() for p in line.split("|")]
        if len(parts) < 6:
            continue
        row = parts[1:-1]
        if len(row) != 4:
            continue
        page_id, _page_path, status, _summary = row
        if page_id.lower() == "id" or page_id.startswith("---"):
            continue
        out[page_id] = status
    return out
def parse_handoff_markers(root: Path, refs: list[str], findings: list[str], warnings: list[str]) -> dict[str, str]:
    markers: dict[str, str] = {}
    numbers: list[int] = []
    for ref_name in refs:
        ref_path, ref_err = resolve_source(root, ref_name)
        if ref_err == "outside":
            findings.append(f"{rel(RECORD_REL)}: source outside root handoff_refs {ref_name}")
            continue
        if ref_path is None:
            findings.append(f"{rel(RECORD_REL)}: missing handoff file {ref_name}")
            continue
        text, ok = read_text_utf8(ref_path)
        source_rel = rel(ref_path.relative_to(root))
        if not ok or text is None:
            findings.append(f"{source_rel}: not valid UTF-8")
            continue
        for line in text.splitlines():
            line = line.strip()
            if not line.startswith("Boundary"):
                continue
            match = MARKER_LINE_RE.match(line)
            if not match:
                marker_bits = line.split(":", 1)[0].split(maxsplit=1)
                marker = marker_bits[1] if len(marker_bits) > 1 else "(missing)"
                findings.append(f"{source_rel}: unknown boundary marker {marker}")
                continue
            marker, hash_value = match.group(1), match.group(2)
            marker_match = BOUNDARY_RE.fullmatch(marker)
            if not marker_match:
                findings.append(f"{source_rel}: unknown boundary marker {marker}")
                continue
            marker_num = int(marker_match.group(1))
            if marker in markers:
                warnings.append(f"{source_rel}: warning repeated boundary marker {marker}")
            if numbers and marker_num < numbers[-1]:
                findings.append(f"{source_rel}: boundary out of order {marker}")
            numbers.append(marker_num)
            markers[marker] = hash_value
    return markers
def git_has_commit(root: Path, sha: str) -> bool:
    try:
        result = subprocess.run(
            ["git", "rev-parse", "--verify", f"{sha}^{{commit}}"],
            cwd=str(root),
            capture_output=True,
            text=True,
            encoding="utf-8",
        )
    except OSError:
        return False
    return result.returncode == 0
def is_git_repo(root: Path) -> bool:
    try:
        result = subprocess.run(
            ["git", "rev-parse", "--is-inside-work-tree"],
            cwd=str(root),
            capture_output=True,
            text=True,
            encoding="utf-8",
        )
    except OSError:
        return False
    return result.returncode == 0 and result.stdout.strip() == "true"
def git_last_touch_commit(root: Path, rel_path: str) -> str | None:
    """The newest commit that touched the file, or None when git has none (never committed)."""
    try:
        result = subprocess.run(
            ["git", "log", "-n", "1", "--format=%H", "--", rel_path],
            cwd=str(root), capture_output=True, text=True, encoding="utf-8",
        )
    except OSError:
        return None
    commit = result.stdout.strip() if result.returncode == 0 else ""
    return commit if HASH_RE.fullmatch(commit) else None
def git_is_ancestor(root: Path, older: str, newer: str) -> bool:
    try:
        result = subprocess.run(
            ["git", "merge-base", "--is-ancestor", older, newer],
            cwd=str(root), capture_output=True, text=True, encoding="utf-8",
        )
    except OSError:
        return False
    return result.returncode == 0
KIND_RESTART = "restart"
KIND_CONTEXT_READING = "context_reading"
REPORT_FIELDS = ("next_card", "next_step", "boundary_hash", "rules_read")
REPORT_TOKENS_RE = re.compile(r"subagent_tokens\s*[:=]\s*([0-9][0-9,]*(?:\.[0-9]+)?)(k?)", re.I)
def _split_rules(value: object) -> set[str]:
    if isinstance(value, str):
        return {part.strip() for part in re.split(r"[,;\s]+", value) if part.strip()}
    if isinstance(value, list):
        return {item.strip() for item in value if isinstance(item, str) and item.strip()}
    return set()
def parse_restart_report(path: Path) -> dict[str, object] | None:
    """Fields of a restart report: JSON (top level or one nested object) or 'name: value' lines.
    Keys: tokens (int or None), has_fields (bool), next_card, next_step, boundary_hash, rules_read (set)."""
    text, ok = read_text_utf8(path)
    if not ok or text is None:
        return None
    out: dict[str, object] = {"tokens": None, "has_fields": False, "next_card": None, "next_step": None,
                              "boundary_hash": None, "rules_read": set()}
    if path.suffix.lower() == ".json":
        try:
            data = json.loads(text)
        except json.JSONDecodeError:
            return None
        if not isinstance(data, dict):
            return None
        flat: dict[str, object] = {}
        for key, value in data.items():
            if isinstance(value, dict):
                for inner_key, inner in value.items():
                    flat.setdefault(inner_key, inner)
            else:
                flat[key] = value
        for key in ("subagent_tokens", "tokens_used"):
            if _number(flat.get(key)) is not None:
                out["tokens"] = _number(flat.get(key))
                break
        for key in ("next_card", "next_step", "boundary_hash"):
            if flat.get(key) is not None:
                out[key] = str(flat[key]).strip()
                out["has_fields"] = True
        rules = flat.get("rules_read", flat.get("rule_pages_read"))
        if rules is not None:
            out["rules_read"] = _split_rules(rules)
            out["has_fields"] = True
        return out
    match = REPORT_TOKENS_RE.search(text)
    if match:
        number = float(match.group(1).replace(",", ""))
        out["tokens"] = int(round(number * 1000)) if match.group(2) else int(round(number))
    rules: set[str] = set()
    for line in text.splitlines():
        if ":" not in line:
            continue
        name, value = [part.strip() for part in line.split(":", 1)]
        name = name.lstrip("-* ").lower()
        if name in ("next_card", "next_step", "boundary_hash"):
            out[name] = value
            out["has_fields"] = True
        elif name in ("rules_read", "rule_page", "rule_pages_read"):
            rules |= _split_rules(value)
            out["has_fields"] = True
    out["rules_read"] = rules
    return out
def hashes_agree(left: object, right: object) -> bool:
    """Two commit hashes agree when one is a prefix of the other (7 characters at least)."""
    if not isinstance(left, str) or not isinstance(right, str):
        return False
    left, right = left.strip().lower(), right.strip().lower()
    return len(left) >= 7 and len(right) >= 7 and (left.startswith(right) or right.startswith(left))
def compute_restart_misses(key: dict[str, object], report: dict[str, object]) -> list[str]:
    """One miss per key rule page the report omits, plus one for a wrong next_step, next_card or boundary_hash."""
    misses: list[str] = []
    for page in sorted(_split_rules(key.get("rules_to_apply"))):
        if page not in report["rules_read"]:
            misses.append(f"rule {page}")
    if key.get("expected_boundary_hash") is not None and not hashes_agree(key.get("expected_boundary_hash"), report["boundary_hash"]):
        misses.append("boundary_hash")
    if key.get("next_step") is not None and str(key.get("next_step")).strip() != report["next_step"]:
        misses.append("next_step")
    if key.get("next_card") is not None and str(key.get("next_card")).strip() != report["next_card"]:
        misses.append("next_card")
    return misses
USAGE_PREFIX = ".mob/records/234/usage/"
SETUP_BASELINE = 50000
MEDIAN_BASELINE = 125000
TEXT_NUM_LINE_RE = re.compile(r"^\s*[\w.\-]+\s*:\s*[0-9][0-9,.]*k?\s*$", re.M)
def _number(value: object) -> int | None:
    if isinstance(value, int) and not isinstance(value, bool):
        return value
    return None
def _json_lookup(data: object, key: str) -> int | None:
    cur = data
    for match in re.finditer(r"([^.\[\]]+)|\[(\d+)\]", key):
        name, index = match.groups()
        if index is not None:
            if not isinstance(cur, list) or int(index) >= len(cur):
                return None
            cur = cur[int(index)]
        else:
            if not isinstance(cur, dict) or name not in cur:
                return None
            cur = cur[name]
    return _number(cur)
def _text_lookup(text: str, key: str) -> int | None:
    match = re.search(r"^\s*" + re.escape(key) + r"\s*:\s*([0-9][0-9,]*(?:\.[0-9]+)?)(k?)\s*$", text, re.M)
    if not match:
        return None
    number = float(match.group(1).replace(",", ""))
    return int(round(number * 1000)) if match.group(2) else int(round(number))
def lookup_source_number(path: Path, key: str) -> int | None:
    """Value a committed file holds under key: JSON dotted key (a+b sums parts) or a 'NAME: N' text line."""
    text, ok = read_text_utf8(path)
    if not ok or text is None:
        return None
    data = None
    if path.suffix.lower() == ".json":
        try:
            data = json.loads(text)
        except json.JSONDecodeError:
            return None
    total = 0
    for part in key.split("+"):
        part = part.strip()
        value = _json_lookup(data, part) if data is not None else _text_lookup(text, part)
        if value is None:
            return None
        total += value
    return total
def source_needs_key(path: Path) -> bool:
    if path.suffix.lower() == ".json":
        return True
    text, ok = read_text_utf8(path)
    return bool(ok and text and TEXT_NUM_LINE_RE.search(text))
def compare_with_source(
    root: Path, source: object, key: object, value: object, field: str, where: str,
    findings: list[str], require_key: bool = False,
) -> None:
    """A recorded number must equal the number its committed source file holds under its key."""
    path = rel(RECORD_REL)
    source_path, _err = resolve_source(root, source)
    if source_path is None:
        return
    if key is None:
        if require_key or source_needs_key(source_path):
            findings.append(f"{path}: missing {field}_source_key {where}")
        return
    if not isinstance(key, str) or not key.strip():
        findings.append(f"{path}: missing {field}_source_key {where}")
        return
    found = lookup_source_number(source_path, key.strip())
    if found is None:
        findings.append(f"{path}: source key not found {field} {where}")
    elif found != value:
        findings.append(f"{path}: source mismatch {field} {where}: record {value}, source {found}")
def source_holds_number_for(path: Path, ctx: str, key: object) -> int | None:
    if isinstance(key, str) and key.strip():
        found = lookup_source_number(path, key.strip())
        if found is not None:
            return found
    text, ok = read_text_utf8(path)
    if not ok or text is None:
        return None
    try:
        data = json.loads(text)
    except json.JSONDecodeError:
        return None
    rows = data.get("seats") if isinstance(data, dict) else None
    if isinstance(rows, list):
        for row in rows:
            if isinstance(row, dict) and row.get("context") == ctx and _number(row.get("tokens")) is not None:
                return _number(row.get("tokens"))
    return None
def validate_seat_metrics(root: Path, record: dict[str, object], findings: list[str]) -> None:
    record_rel = rel(RECORD_REL)
    task_force = record.get("task_force")
    if not isinstance(task_force, list):
        findings.append(f"{record_rel}: no task force")
        contexts: list[str] = []
    else:
        contexts = [
            item["context"].strip()
            for item in task_force
            if isinstance(item, dict) and isinstance(item.get("context"), str) and item["context"].strip()
        ]
    seat_metrics = record.get("seat_metrics")
    if not isinstance(seat_metrics, dict):
        findings.append(f"{record_rel}: no seat metrics")
        return
    for ctx in contexts:
        entry = seat_metrics.get(ctx)
        if entry is None:
            findings.append(f"{record_rel}: missing seat {ctx}")
            continue
        if not isinstance(entry, dict):
            findings.append(f"{record_rel}: bad token value setup_tokens {ctx}")
            continue
        measured = ("setup_tokens", "end_tokens", "provider_usage_source")
        unmeasured = ("not_measured_reason", "available_evidence_source")
        has_measured = any(key in entry for key in measured)
        has_unmeasured = any(key in entry for key in unmeasured)
        if has_measured and has_unmeasured:
            findings.append(f"{record_rel}: measured and not measured {ctx}")
            continue
        if has_unmeasured and not has_measured:
            reason = entry.get("not_measured_reason")
            if not isinstance(reason, str) or not reason.strip():
                findings.append(f"{record_rel}: missing not_measured_reason {ctx}")
            evidence = entry.get("available_evidence_source")
            if not isinstance(evidence, str) or not evidence.strip():
                findings.append(f"{record_rel}: missing available_evidence_source {ctx}")
            else:
                source_path, source_err = resolve_source(root, evidence)
                if source_err == "outside":
                    findings.append(f"{record_rel}: source outside root available_evidence_source {ctx}")
                elif source_path is None:
                    findings.append(f"{record_rel}: source not found available_evidence_source {ctx}")
                else:
                    held = source_holds_number_for(source_path, ctx, entry.get("tokens_source_key"))
                    if held is not None:
                        findings.append(f"{record_rel}: not measured but source holds {held} {ctx}")
            continue
        for field in ("setup_tokens", "end_tokens"):
            if field not in entry:
                findings.append(f"{record_rel}: missing {field} {ctx}")
            elif not is_token(entry.get(field)):
                findings.append(f"{record_rel}: bad token value {field} {ctx}")
        if "provider_usage_source" not in entry:
            findings.append(f"{record_rel}: missing provider_usage_source {ctx}")
        else:
            source = entry.get("provider_usage_source")
            if not isinstance(source, str) or not source.strip():
                findings.append(f"{record_rel}: missing provider_usage_source {ctx}")
            else:
                source_path, source_err = resolve_source(root, source)
                if source_err == "outside":
                    findings.append(f"{record_rel}: source outside root provider_usage_source {ctx}")
                elif source_path is None:
                    findings.append(f"{record_rel}: source not found provider_usage_source {ctx}")
                else:
                    for field in ("setup_tokens", "end_tokens"):
                        if is_token(entry.get(field)):
                            compare_with_source(root, source, entry.get(f"{field}_source_key"),
                                                entry.get(field), field, ctx, findings)
                    for extra in entry.get("unverified_rows") or []:
                        if isinstance(extra, dict):
                            compare_with_source(root, source, extra.get("source_key"),
                                                extra.get("tokens"), "unverified_rows", ctx, findings, True)
def validate_restart_evidence(
    root: Path, restart: dict[str, object], boundary_hash: object, fac_tokens: object, in_git_repo: bool,
    findings: list[str], notes: list[str], where: str,
) -> list[str] | None:
    """A kind 'restart' entry needs a committed key and a committed report; returns the computed miss list
    (None when the misses cannot be computed because a file is missing or unreadable)."""
    path = rel(RECORD_REL)
    sources: dict[str, Path | None] = {}
    for field in ("restart_key_source", "restart_report_source"):
        source_path, source_err = resolve_source(root, restart.get(field))
        sources[field] = source_path
        if source_err == "outside":
            findings.append(f"{path}: source outside root {field} {where}")
        elif source_path is None:
            if restart.get(field) is None:
                findings.append(f"{path}: missing {field} {where}")
            else:
                findings.append(f"{path}: source not found {field} {where}")
    key_path, report_path = sources["restart_key_source"], sources["restart_report_source"]
    key: dict[str, object] | None = None
    if key_path is not None:
        key_text, key_ok = read_text_utf8(key_path)
        try:
            loaded = json.loads(key_text) if key_ok and key_text else None
        except json.JSONDecodeError:
            loaded = None
        if isinstance(loaded, dict):
            key = loaded
        else:
            findings.append(f"{path}: restart key not a JSON object {where}")
    if key is not None:
        for field in ("expected_boundary_hash", "next_card", "next_step", "rules_to_apply"):
            if key.get(field) in (None, "", []):
                findings.append(f"{path}: restart key missing {field} {where}")
        if not isinstance(key.get("rules_to_apply"), list):
            findings.append(f"{path}: restart key missing rules_to_apply {where}")
        if isinstance(boundary_hash, str) and key.get("expected_boundary_hash") and not hashes_agree(key.get("expected_boundary_hash"), boundary_hash):
            findings.append(f"{path}: restart key boundary mismatch {where}")
    if key_path is not None and report_path is not None and in_git_repo:
        key_commit = git_last_touch_commit(root, rel(key_path.relative_to(root.resolve())))
        report_commit = git_last_touch_commit(root, rel(report_path.relative_to(root.resolve())))
        if not key_commit or not report_commit:
            findings.append(f"{path}: restart key or report not committed {where}")
        elif key_commit == report_commit or not git_is_ancestor(root, key_commit, report_commit):
            findings.append(f"{path}: key commit is not older than report commit {where}")
    report = parse_restart_report(report_path) if report_path is not None else None
    if report_path is not None and report is None:
        findings.append(f"{path}: restart report not readable {where}")
    if report is not None and is_token(fac_tokens):
        if report["tokens"] is None:
            findings.append(f"{path}: source key not found facilitator_context_tokens {where}")
        elif report["tokens"] != fac_tokens:
            findings.append(f"{path}: source mismatch facilitator_context_tokens {where}: record {fac_tokens}, report {report['tokens']}")
    if key is None or report is None:
        return None
    if not report["has_fields"]:
        if report_path is not None and report_path.suffix.lower() == ".json":
            notes.append(f"restart report {where} carries no fields: misses not computable")
            return None
        findings.append(f"{path}: restart report has no fields {where}")
        return None
    return compute_restart_misses(key, report)
def validate_restart(
    root: Path,
    restart: object,
    idx: int,
    index_status: dict[str, str],
    markers: dict[str, str],
    in_git_repo: bool,
    findings: list[str],
    notes: list[str],
) -> None:
    path = rel(RECORD_REL)
    where = f"restart {idx}"
    if not isinstance(restart, dict):
        findings.append(f"{path}: missing boundary_marker {where}")
        return
    kind = restart.get("kind")
    if kind is not None and kind not in (KIND_RESTART, KIND_CONTEXT_READING):
        findings.append(f"{path}: bad kind {where}")
    marker = restart.get("boundary_marker")
    marker = marker.strip() if isinstance(marker, str) else ""
    if not marker:
        findings.append(f"{path}: missing boundary_marker {where}")
        marker = ""
    boundary_hash = restart.get("boundary_hash")
    if boundary_hash is None:
        findings.append(f"{path}: missing boundary_hash {where}")
    elif not isinstance(boundary_hash, str) or not HASH_RE.fullmatch(boundary_hash):
        findings.append(f"{path}: bad boundary hash {where}")
    elif in_git_repo and not git_has_commit(root, boundary_hash):
        findings.append(f"{path}: unknown boundary hash {where}")
    pages = restart.get("wiki_pages_read")
    page_set: set[str] = set()
    if pages is None:
        findings.append(f"{path}: missing wiki_pages_read {where}")
        pages = []
    if isinstance(pages, list):
        for page in pages:
            if not isinstance(page, str):
                continue
            page = page.strip()
            if page in page_set:
                findings.append(f"{path}: repeated page id {page} {where}")
            page_set.add(page)
            status = index_status.get(page)
            if status is None:
                findings.append(f"{path}: unknown page id {page} {where}")
            elif status != "enabled":
                findings.append(f"{path}: disabled page id {page} {where}")
    if "core/index" not in page_set:
        findings.append(f"{path}: missing core/index {where}")
    refs_value = restart.get("handoff_refs")
    refs: list[str] = []
    if refs_value is None:
        findings.append(f"{path}: missing handoff_refs {where}")
    elif isinstance(refs_value, list):
        refs = [item.strip() for item in refs_value if isinstance(item, str) and item.strip()]
        if "HANDOFF.md" not in refs:
            findings.append(f"{path}: handoff refs without HANDOFF.md {where}")
        for ref_name in refs:
            ref_path, ref_err = resolve_source(root, ref_name)
            if ref_err == "outside":
                findings.append(f"{path}: source outside root handoff_refs {where}")
            elif ref_path is None:
                findings.append(f"{path}: missing handoff file {ref_name}")
            else:
                text, ok = read_text_utf8(ref_path)
                if not ok or text is None:
                    findings.append(f"{rel(ref_path.relative_to(root))}: not valid UTF-8")
    else:
        findings.append(f"{path}: missing handoff_refs {where}")
    fac_tokens = restart.get("facilitator_context_tokens")
    if fac_tokens is None:
        findings.append(f"{path}: missing facilitator_context_tokens {where}")
    elif not is_token(fac_tokens):
        findings.append(f"{path}: bad token value facilitator_context_tokens {where}")
    if "facilitator_provider_usage_source" not in restart:
        findings.append(f"{path}: missing facilitator_provider_usage_source {where}")
    else:
        source = restart.get("facilitator_provider_usage_source")
        if not isinstance(source, str) or not source.strip():
            findings.append(f"{path}: missing facilitator_provider_usage_source {where}")
        else:
            source_path, source_err = resolve_source(root, source)
            if source_err == "outside":
                findings.append(f"{path}: source outside root facilitator_provider_usage_source {where}")
            elif source_path is None:
                findings.append(f"{path}: source not found facilitator_provider_usage_source {where}")
            elif is_token(fac_tokens):
                compare_with_source(root, source, restart.get("facilitator_context_tokens_source_key"),
                                    fac_tokens, "facilitator_context_tokens", where, findings)
    baseline = restart.get("baseline_tokens")
    if baseline != BASELINE_TOKENS:
        findings.append(f"{path}: baseline mismatch {where}")
    delta = restart.get("delta_tokens")
    if (
        is_token(fac_tokens)
        and isinstance(baseline, int)
        and not isinstance(baseline, bool)
        and isinstance(delta, int)
        and not isinstance(delta, bool)
        and delta != fac_tokens - baseline
    ):
        findings.append(f"{path}: delta mismatch {where}")
    elif delta is not None and not isinstance(delta, int):
        findings.append(f"{path}: delta mismatch {where}")
    if marker:
        expected = markers.get(marker)
        if expected is None:
            findings.append(f"{path}: unknown boundary marker {marker} {where}")
        elif isinstance(boundary_hash, str) and HASH_RE.fullmatch(boundary_hash) and boundary_hash != expected:
            findings.append(f"{path}: boundary hash mismatch {marker} {where}")
    computed: list[str] | None = None
    if kind == KIND_RESTART:
        computed = validate_restart_evidence(root, restart, boundary_hash, fac_tokens, in_git_repo, findings, notes, where)
    if "rule_miss_cases" not in restart:
        findings.append(f"{path}: missing rule_miss_cases {where}")
        return
    misses = restart.get("rule_miss_cases")
    if not isinstance(misses, list):
        findings.append(f"{path}: rule_miss_cases not a list {where}")
        return
    if not misses:
        zero_source = restart.get("rule_miss_cases_zero_source")
        if not isinstance(zero_source, str) or not zero_source.strip():
            findings.append(f"{path}: rule_miss_cases empty without source {where}")
            return
        zero_path, zero_err = resolve_source(root, zero_source)
        if zero_err == "outside":
            findings.append(f"{path}: source outside root rule_miss_cases_zero_source {where}")
        elif zero_path is None:
            findings.append(f"{path}: source not found rule_miss_cases_zero_source {where}")
        elif not rel(zero_path.relative_to(root.resolve())).startswith(USAGE_PREFIX):
            findings.append(f"{path}: rule_miss_cases_zero_source not a usage file {where}")
        elif kind == KIND_RESTART and zero_path != resolve_source(root, restart.get("restart_report_source"))[0]:
            findings.append(f"{path}: rule_miss_cases_zero_source is not the restart report {where}")
        if computed:
            findings.append(f"{path}: rule_miss_cases mismatch {where}: record 0, computed {len(computed)} ({', '.join(computed)})")
        return
    if computed is not None and len(misses) != len(computed):
        findings.append(f"{path}: rule_miss_cases mismatch {where}: record {len(misses)}, computed {len(computed)} ({', '.join(computed) or 'none'})")
    for miss_idx, miss in enumerate(misses, start=1):
        if not isinstance(miss, dict):
            findings.append(f"{path}: bad rule miss case {miss_idx} object {where}")
            continue
        for field in ("context", "boundary", "missed_page_id", "source"):
            value = miss.get(field)
            if not isinstance(value, str) or not value.strip():
                findings.append(f"{path}: bad rule miss case {miss_idx} {field} {where}")
        source = miss.get("source")
        if isinstance(source, str) and source.strip():
            source_path, source_err = resolve_source(root, source)
            if source_err == "outside":
                findings.append(f"{path}: source outside root source {where}")
            elif source_path is None:
                findings.append(f"{path}: source not found source {where}")
        missed_page = miss.get("missed_page_id")
        if isinstance(missed_page, str) and missed_page.strip():
            missed_page = missed_page.strip()
            if missed_page not in index_status:
                findings.append(f"{path}: missed page unknown {missed_page} {where}")
            elif missed_page in page_set:
                findings.append(f"{path}: missed page was read {missed_page} {where}")
        miss_boundary = miss.get("boundary")
        if isinstance(miss_boundary, str) and miss_boundary.strip() and miss_boundary.strip() not in markers:
            findings.append(f"{path}: unknown boundary marker {miss_boundary.strip()} {where}")
def validate_seat_ab(root: Path, record: dict[str, object], findings: list[str], notes: list[str]) -> None:
    pairs = record.get("seat_ab")
    if pairs is None:
        return
    path = rel(RECORD_REL)
    if not isinstance(pairs, list):
        findings.append(f"{path}: seat_ab not a list")
        return
    for idx, pair in enumerate(pairs, start=1):
        where = f"seat_ab {idx}"
        if not isinstance(pair, dict):
            findings.append(f"{path}: bad seat_ab pair {where}")
            continue
        if pair.get("setup_baseline_tokens") != SETUP_BASELINE:
            findings.append(f"{path}: baseline mismatch setup_baseline_tokens {where}")
        if pair.get("median_baseline_tokens") != MEDIAN_BASELINE:
            findings.append(f"{path}: baseline mismatch median_baseline_tokens {where}")
        unverified = pair.get("unverified") is True
        values = {}
        for side in ("full", "brief"):
            tokens = pair.get(f"{side}_tokens")
            if not is_token(tokens):
                findings.append(f"{path}: bad token value {side}_tokens {where}")
                continue
            values[side] = tokens
            source = pair.get(f"{side}_source")
            source_path, source_err = resolve_source(root, source)
            if source_path is None and unverified:
                notes.append(f"unverified {side}_tokens {where} ({tokens}): no committed raw source")
                continue
            if source_err == "outside":
                findings.append(f"{path}: source outside root {side}_source {where}")
            elif source_path is None:
                findings.append(f"{path}: source not found {side}_source {where}")
            else:
                compare_with_source(root, source, pair.get(f"{side}_source_key"), tokens,
                                    f"{side}_tokens", where, findings, True)
        if len(values) == 2:
            ratio = values["brief"] / values["full"]
            note = (f"{where} {pair.get('step')}: full {values['full']}, brief {values['brief']}, "
                    f"brief/full {ratio:.2f}; vs {SETUP_BASELINE}: full {values['full'] - SETUP_BASELINE:+d}, "
                    f"brief {values['brief'] - SETUP_BASELINE:+d}; vs {MEDIAN_BASELINE}: full "
                    f"{values['full'] - MEDIAN_BASELINE:+d}, brief {values['brief'] - MEDIAN_BASELINE:+d}")
            if ratio > 1:
                note += " (brief above full, reported)"
            notes.append(note)
def seat_summary(record: dict[str, object], notes: list[str]) -> None:
    metrics = record.get("seat_metrics")
    if not isinstance(metrics, dict):
        return
    verified_ends = []
    for ctx, entry in metrics.items():
        if not isinstance(entry, dict) or not is_token(entry.get("end_tokens")):
            continue
        if "end_tokens" in (entry.get("unverified_fields") or []):
            notes.append(f"unverified end_tokens {ctx} ({entry['end_tokens']}): left out of the median")
        else:
            verified_ends.append(entry["end_tokens"])
        for extra in entry.get("unverified_rows") or []:
            if isinstance(extra, dict):
                notes.append(f"unverified row {ctx} {extra.get('source_key')} ({extra.get('tokens')})")
    if verified_ends:
        ordered = sorted(verified_ends)
        mid = len(ordered) // 2
        median = ordered[mid] if len(ordered) % 2 else (ordered[mid - 1] + ordered[mid]) / 2
        notes.append(f"median verified end_tokens {median:g} over {len(ordered)} contexts vs {MEDIAN_BASELINE}: "
                     f"{median - MEDIAN_BASELINE:+g}")
def run(root: Path) -> int:
    findings: list[str] = []
    warnings: list[str] = []
    notes: list[str] = []
    index_status = parse_index_status(root, findings)
    record_path = root / RECORD_REL
    if not record_path.is_file():
        findings.append(f"{rel(RECORD_REL)}: no record")
    else:
        text, ok = read_text_utf8(record_path)
        if not ok or text is None:
            findings.append(f"{rel(RECORD_REL)}: record not valid JSON")
        else:
            try:
                record = json.loads(text)
            except json.JSONDecodeError:
                record = None
                findings.append(f"{rel(RECORD_REL)}: record not valid JSON")
            if record is not None and not isinstance(record, dict):
                findings.append(f"{rel(RECORD_REL)}: record not valid JSON")
            if isinstance(record, dict):
                validate_seat_metrics(root, record, findings)
                validate_seat_ab(root, record, findings, notes)
                seat_summary(record, notes)
                restarts = record.get("restarts")
                if not isinstance(restarts, list) or not restarts:
                    findings.append(f"{rel(RECORD_REL)}: no restart")
                else:
                    handoff_refs = sorted(
                        set(
                            ref.strip()
                            for restart in restarts
                            if isinstance(restart, dict)
                            for ref in restart.get("handoff_refs", [])
                            if isinstance(ref, str) and ref.strip()
                        )
                    ) or ["HANDOFF.md"]
                    markers = parse_handoff_markers(root, handoff_refs, findings, warnings)
                    in_git_repo = is_git_repo(root)
                    if all(isinstance(r, dict) and r.get("kind") == KIND_CONTEXT_READING for r in restarts):
                        findings.append(f"{rel(RECORD_REL)}: no restart (every entry is a context reading)")
                    for idx, restart in enumerate(restarts, start=1):
                        validate_restart(root, restart, idx, index_status, markers, in_git_repo, findings, notes)
    for line in warnings:
        print(line, file=sys.stderr)
    if findings:
        for line in findings:
            print(line, file=sys.stderr)
        return 1
    restart_count = 0
    try:
        loaded = json.loads(record_path.read_text(encoding="utf-8"))
        if isinstance(loaded, dict) and isinstance(loaded.get("restarts"), list):
            restart_count = len(loaded["restarts"])
    except (OSError, UnicodeDecodeError, json.JSONDecodeError):
        pass
    print(f"restart-record: OK ({restart_count} restarts)")
    for line in notes:
        print(f"restart-record: {line}")
    return 0
def main() -> int:
    parser = argparse.ArgumentParser(add_help=True)
    parser.add_argument("--root", default=".", help="Repository root to check.")
    args = parser.parse_args()
    root = Path(args.root)
    if not root.is_dir():
        print("not a directory", file=sys.stderr)
        return 2
    try:
        return run(root.resolve())
    except Exception:
        print(f"{rel(RECORD_REL)}: record not valid JSON", file=sys.stderr)
        return 1
if __name__ == "__main__":
    sys.exit(main())
