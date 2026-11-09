"""Build the backends file the facilitator's runners pass to decider.py: the REAL core/backends.json of mob-crosscli (so Muse, Grok's
--prompt-file launcher, the Codex launcher and any later driver come from the one source), with ONLY the Claude seat's model pinned to
sonnet (the real file's Claude entry has no default model and its probe model is haiku). Rebuilt at every launch, so it never goes stale.
The cause of the Muse drop-out found 2026-11-08: the runners used a pinned COPY made before Muse was added, so decider.py could not seat it."""
import json

REAL = "C:/Users/<user>/mob-crosscli/core/backends.json"
OUT = "C:/Users/<user>/AppData/Local/Temp/claude/1ff08f82160bcdeb/2ca12683-042f-40b7-b61a-623ae180176b/scratchpad/backends-current.json"


def build():
    d = json.load(open(REAL, encoding="utf-8"))
    claude = d["backends"]["claude"]
    claude["probe_model"] = "sonnet"
    claude.setdefault("models", {})["default"] = "sonnet"
    d["_comment"] = "Derived from core/backends.json at launch; only the Claude seat's model is pinned to sonnet."
    json.dump(d, open(OUT, "w", encoding="utf-8", newline="\n"), indent=1)
    return OUT


if __name__ == "__main__":
    print(build())
