#!/usr/bin/env python3
"""Mechanical guard for the product owner's safety rules (a ban with no detector isn't in force).

  * Never recursively delete a variable or computed path (a BaccaratSim agy dispatch once deleted PowerShell 7, ~/.dotnet,
    ~/.ssh and more by running rm -rf on a temp-dir variable that expanded wrong).
  * Never kill a process by name or pattern: only by exact PID, and only your own.

Three parts:
  find_unsafe(text)      detector: scan any text (a reply, a brief, a script diff) for the patterns above
  safe_delete(path, roots)  the one way to delete: refuses anything that is not a verified, strict child of a known scratch root
  SAFETY_RULE            the sentence appended to every dispatched prompt
"""

import re
import shutil
from dataclasses import dataclass
from pathlib import Path

SAFETY_RULE = ("Safety: never kill a process by name or pattern (exact PID only, and only your own); never delete "
               "recursively on a variable or computed path (use the safe_delete helper from mob-defaults, or a literal "
               "path inside the repo's scratch folder).")

KIND_DELETE = "recursive delete on a variable or unresolved path"
KIND_KILL = "kill by process name or pattern"
KIND_CLEAN = "git clean (deletes untracked files across the tree)"


@dataclass(frozen=True)
class Finding:
    kind: str
    snippet: str


_TOKEN = re.compile(r'"[^"]*"|\'[^\']*\'|\S+')
_SEPARATORS = re.compile(r"\s*(?:&&|\|\||[;|&\n])")
_DANGEROUS_LITERALS = {"/", "/*", ".", "..", "./", "../", "*", "~", "\\", ".\\", ".\\*", "./*", "~/", "~\\"}


def _tokens(text):
    return [t for t in _TOKEN.findall(text)]


def _unquote(token):
    token = token.strip()
    if len(token) >= 2 and token[0] == token[-1] and token[0] in "\"'":
        token = token[1:-1]
    return token


def _unsafe_target(token):
    """A delete target is unsafe if it is empty, a root-like literal, or built from a variable or wildcard."""
    t = _unquote(token).strip()
    if not t or t in _DANGEROUS_LITERALS:
        return True
    if re.fullmatch(r"[A-Za-z]:[\\/]*", t):  # a drive root
        return True
    if t.startswith("~"):
        return True
    if any(mark in t for mark in ("$", "%", "`", "*", "?")):
        return True
    if "<" in t and ">" in t:  # an unfilled placeholder
        return True
    return False


def _first_clause(text):
    """The part of a command line before the next shell separator."""
    m = _SEPARATORS.search(text)
    return text[:m.start()] if m else text


def _find_rm(text):
    out = []
    for m in re.finditer(r"(?i)(?<![\w-])rm\s+((?:-[A-Za-z-]+(?:[ \t]+|$))+)([^\n]*)", text, re.MULTILINE):
        flags = m.group(1)
        recursive = "--recursive" in flags or re.search(r"-[A-Za-z]*[rR]", flags.replace("--", "  "))
        if not recursive:
            continue
        targets = _tokens(_first_clause(m.group(2)))
        # the targets end where prose begins; judge the first target, and any further token that looks like a path
        path_like = [t for t in targets if re.search(r"[\\/$%~*.]", t) or t.startswith(('"', "'"))]
        if not targets or not path_like or any(_unsafe_target(t) for t in path_like):
            out.append(Finding(KIND_DELETE, (m.group(0).strip())[:160]))
    return out


def _find_remove_item(text):
    out = []
    for m in re.finditer(r"(?i)\bRemove-Item\b([^\n;|]*)", text):
        args = m.group(1)
        if not re.search(r"(?i)-Recurse\b", args):
            continue
        tokens, targets, i = _tokens(args), [], 0
        while i < len(tokens):
            tok = tokens[i]
            if tok.lower() in ("-path", "-literalpath") and i + 1 < len(tokens):
                targets.append(tokens[i + 1]); i += 2; continue
            if tok.startswith("-"):
                i += 1; continue
            targets.append(tok); i += 1
        if not targets or any(_unsafe_target(t) for t in targets):
            out.append(Finding(KIND_DELETE, ("Remove-Item" + args).strip()[:160]))
    return out


def _find_cmd_delete(text):
    out = []
    for m in re.finditer(r"(?i)\b(?:rd|rmdir|del)\s+([^\n&|;]*)", text):
        args = m.group(1)
        if not re.search(r"(?i)(?:^|\s)/s\b", args):
            continue
        targets = [t for t in _tokens(args) if not t.startswith("/")]
        if not targets or any(_unsafe_target(t) for t in targets):
            out.append(Finding(KIND_DELETE, m.group(0).strip()[:160]))
    return out


def _find_rmtree(text):
    out = []
    for m in re.finditer(r"shutil\.rmtree\(\s*([^,)\n]*)", text):
        arg = m.group(1).strip()
        literal = re.fullmatch(r"[rR]?(['\"])[^'\"$%{<*]*\1", arg)
        if not literal:
            out.append(Finding(KIND_DELETE, ("shutil.rmtree(" + arg + ")")[:160]))
    return out


def _find_git_clean(text):
    out = []
    for m in re.finditer(r"(?i)\bgit\s+clean\b([^\n;|]*)", text):
        if re.search(r"(?i)(?:^|\s)(?:-[a-z]*f[a-z]*|--force)\b", m.group(1)):
            out.append(Finding(KIND_CLEAN, m.group(0).strip()[:160]))
    return out


def _find_kills(text):
    out = []
    for pattern in (r"(?i)\btaskkill\b[^\n;|]*?\s/IM\b[^\n;|]*", r"(?i)\bpkill\b[^\n;|]*", r"(?i)\bkillall\b[^\n;|]*",
                    r"(?i)\bStop-Process\b[^\n;|]*?-Name\b[^\n;|]*", r"(?i)\bGet-Process\b[^\n]*\|\s*Stop-Process[^\n]*"):
        for m in re.finditer(pattern, text):
            out.append(Finding(KIND_KILL, m.group(0).strip()[:160]))
    return out


def find_unsafe(text):
    """Findings for every recursive delete of a variable/unresolved path and every kill by process name in `text`."""
    text = (text or "").replace(SAFETY_RULE, "")
    findings = []
    for finder in (_find_rm, _find_remove_item, _find_cmd_delete, _find_rmtree, _find_git_clean, _find_kills):
        findings.extend(finder(text))
    return findings


# ----------------------------------------------------------------------------------------------- safe_delete
class UnsafeDelete(Exception):
    """safe_delete refused: the path is not a verified, strict child of a known scratch root."""


def safe_delete(path, roots, dry_run=False):
    """Delete `path` (a file or a tree) only if it is a verified STRICT child of one of `roots`.

    Refuses: empty or non-absolute paths, paths that do not exist, symlinks, anything that resolves outside the roots
    (including via '..'), a root itself, an ancestor of a root, the home directory, and a drive or filesystem root.
    Returns the resolved path that was (or, with dry_run, would be) deleted."""
    if path is None or str(path).strip() == "":
        raise UnsafeDelete("refusing to delete an empty path")
    given = Path(str(path))
    if not given.is_absolute():
        raise UnsafeDelete(f"refusing a relative path: {given}")
    good_roots = []
    for r in roots or []:
        try:
            rr = Path(str(r)).resolve(strict=True)
        except (OSError, RuntimeError):
            continue
        if rr.is_dir() and rr != Path(rr.anchor):
            good_roots.append(rr)
    if not good_roots:
        raise UnsafeDelete("no valid scratch root was given")
    if given.is_symlink():
        raise UnsafeDelete(f"refusing a symlink: {given}")
    try:
        real = given.resolve(strict=True)
    except (OSError, RuntimeError):
        raise UnsafeDelete(f"refusing a path that does not exist: {given}")
    if real == Path(real.anchor):
        raise UnsafeDelete(f"refusing a filesystem or drive root: {real}")
    try:
        if real == Path.home().resolve():
            raise UnsafeDelete(f"refusing the home directory: {real}")
    except RuntimeError:
        pass
    for root in good_roots:
        if real == root:
            raise UnsafeDelete(f"refusing the scratch root itself: {real}")
        if real in root.parents:
            raise UnsafeDelete(f"refusing an ancestor of the scratch root: {real}")
    if not any(root in real.parents for root in good_roots):
        raise UnsafeDelete(f"refusing a path outside the scratch roots: {real}")
    if dry_run:
        return str(real)
    if real.is_dir():
        shutil.rmtree(real)
    else:
        real.unlink()
    return str(real)
