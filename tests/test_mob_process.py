import contextlib, importlib.util, io, json, subprocess, tempfile, unittest
from pathlib import Path
from unittest import mock

SPEC = importlib.util.spec_from_file_location("mob_process", Path(__file__).resolve().parents[1] / "scripts/mob_process.py")
mob = importlib.util.module_from_spec(SPEC); SPEC.loader.exec_module(mob)

def issue(card, labels=("card",), closed="2026-10-29T20:00:00Z", events=()):
    return {"number": card + 1000, "title": f"({card}) work", "labels": list(labels), "closedAt": closed, "events": list(events)}

class MobProcessTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.root = Path(self.temp.name)
        for name in (".mob/records", "docs/decisions", "docs/retros", ".github/workflows", "board"):
            (self.root / name).mkdir(parents=True, exist_ok=True)
        digest = mob.cards_hash([])
        (self.root / ".mob/records/LEGACY.json").write_text(json.dumps({"schema": 1, "decision": "D134", "cutoff": "2026-10-29T20:35:37Z", "cards": [], "cards_sha256": digest}), encoding="utf-8")
        (self.root / "docs/decisions/process-and-staffing.md").write_text(f"D134 sha256 of the list: {digest}\n<a id=\"d7\"></a> Card 201", encoding="utf-8")
        (self.root / "DRIVERS.md").write_text("| Driver | Status |\n|---|---|\n| Codex | available |\n| Claude | available |", encoding="utf-8")
        workflow = "jobs:\n  mob-process:\n    steps:\n      - run: python3 scripts/mob_process.py check\n      - run: git show HEAD~1:scripts/mob_process.py > mob_process_prev.py && python3 mob_process_prev.py check --root .\n"
        (self.root / ".github/workflows/ci.yml").write_text(workflow, encoding="utf-8")
        (self.root / ".github/workflows/mob-process.yml").write_text("issues: [closed, reopened, unlabeled]\nschedule:\n" + workflow.replace("      - run: git show", "      - if: github.event_name != 'schedule'\n        run: git show"), encoding="utf-8")
        protected = ("/scripts/mob_process.py", "/.github/workflows/", "/.mob/records/LEGACY.json", "/docs/decisions/process-and-staffing.md", "/.github/CODEOWNERS", "/.githooks/", "/tests/test_mob_process.py", "/docs/PROCESS-CHECKS.md", "/.mob/records/process.json", "/.mob/records/held.json")
        (self.root / ".github/CODEOWNERS").write_text("\n".join(f"{p} @owner" for p in protected), encoding="utf-8")
        self.git("init", "-b", "main"); self.git("config", "user.email", "test@example.com"); self.git("config", "user.name", "Test"); self.git("add", "."); self.git("commit", "-m", "base")

    def tearDown(self): self.temp.cleanup()
    def git(self, *args): return subprocess.run(["git", *args], cwd=self.root, check=True, capture_output=True, text=True, encoding="utf-8").stdout.strip()

    def valid_record(self, card=201):
        seats=[]; decision=self.root / "docs/decisions/process-and-staffing.md"
        for i, role in enumerate(mob.REQUIRED_ROLES):
            evidence="D7" if i == 0 and card == 201 else f"D{card}{i}"
            seats.append({"role":role,"seat":"Claude" if role=="signoff" else "Codex","model":"test","evidence":evidence})
            if evidence != "D7": decision.write_text(decision.read_text(encoding="utf-8")+f"\n<a id=\"{evidence.lower()}\"></a> Card {card}", encoding="utf-8")
        record={"schema":1,"card":card,"title":"work","process":"mob","seats":seats,"retro":"docs/retros/r.md","notes":""}
        (self.root/f".mob/records/{card:03}.json").write_text(json.dumps(record),encoding="utf-8")
        (self.root/"docs/retros/r.md").write_text(f"Cards: {card}\n# Individual\n# Team\n# Organisation\n# Product\nProcess improvement pick\n",encoding="utf-8")
        self.git("add", "."); self.git("commit", "-m", f"Card {card}: trail"); return record

    def test_persona_story_reports_annotations_and_names_missing_card(self):
        # Given Morgan audits a closed card with no process trail. When the detector runs.
        out=io.StringIO()
        with contextlib.redirect_stdout(out): code=mob.run_check(self.root,[issue(201)],git=self.git)
        # Then CI is red, annotations are valid, and the card is named.
        self.assertEqual(1,code); self.assertTrue(all(x.startswith("::error::mob-process: ") for x in out.getvalue().splitlines())); self.assertIn("Card 201 has no trail record",out.getvalue())

    def test_schema_exact_roles_eligibility_and_signoff(self):
        record=self.valid_record(); path=self.root/".mob/records/201.json"; record["seats"].pop(); path.write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 201 must have exactly the six required role seats",mob.check_records(self.root,[issue(201)],self.git))
        record=self.valid_record(202); record["seats"][0]["seat"]="Mallory"; (self.root/".mob/records/202.json").write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 202 has a seat not listed in DRIVERS.md",mob.check_records(self.root,[issue(202)],self.git))
        record=self.valid_record(203); record["seats"][-1]["seat"]="Codex"; (self.root/".mob/records/203.json").write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 203 signoff seat must differ from green seat",mob.check_records(self.root,[issue(203)],self.git))

    def test_evidence_success_failure_distinctness_and_boundary(self):
        record=self.valid_record(); path=self.root/".mob/records/201.json"
        self.assertNotIn("Card 201 evidence D7 decision does not mention Card 201",mob.check_records(self.root,[issue(201)],self.git))
        decision=self.root/"docs/decisions/process-and-staffing.md"; decision.write_text(decision.read_text(encoding="utf-8").replace("Card 201","Card 2010",1),encoding="utf-8")
        self.assertIn("Card 201 evidence D7 decision does not mention Card 201",mob.check_records(self.root,[issue(201)],self.git))
        record["seats"][1]["evidence"]=record["seats"][0]["evidence"]; path.write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 201 seats require distinct evidence",mob.check_records(self.root,[issue(201)],self.git))
        record["seats"][0]["evidence"]="deadbeef"; path.write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 201 evidence deadbeef is not a reachable matching commit",mob.check_records(self.root,[issue(201)],self.git))

    def test_reopened_notes_unlabeled_card_only_and_was_card(self):
        self.valid_record(); events=({"event":"unlabeled","label":{"name":"card"}},{"event":"reopened"})
        self.assertEqual(["Card 201 has an unlabeled event","Card 201 was reopened but its record notes are empty"],mob.check_events(self.root,[issue(201,events=events)]))
        self.assertNotIn("Card 201 has an unlabeled event",mob.check_events(self.root,[issue(201,events=({"event":"unlabeled","label":{"name":"bug"}},))]))
        item=issue(201,labels=(),events=({"event":"labeled","label":{"name":"card"}},{"event":"unlabeled","label":{"name":"card"}}))
        self.assertEqual(["Card 201 has an unlabeled event"],mob.check_events(self.root,[item]))
        record=json.loads((self.root/".mob/records/201.json").read_text(encoding="utf-8")); record["notes"]="audited"; (self.root/".mob/records/201.json").write_text(json.dumps(record),encoding="utf-8")
        self.assertEqual([],mob.check_events(self.root,[issue(201,events=({"event":"reopened"},))]))

    def test_legacy_hash_and_cutoff_are_separate(self):
        legacy=json.loads((self.root/".mob/records/LEGACY.json").read_text(encoding="utf-8")); legacy["cards"]=[1]; (self.root/".mob/records/LEGACY.json").write_text(json.dumps(legacy),encoding="utf-8")
        errors=mob.check_legacy(self.root,[issue(1,closed="2026-10-30T00:00:00Z")])
        self.assertIn("LEGACY.json schema, decision, or cards_sha256 is not frozen",errors); self.assertIn("LEGACY.json card hash does not equal the D134 pinned hash",errors); self.assertIn("Legacy Card 001 closed after the cutoff",errors)

    def test_retro_bounds_target_cover_and_substance(self):
        retro=self.root/"docs/retros/r.md"
        for line,message in (("Cards: 9-1\n","starts after"),("Cards: 1-252\n","wider than 250"),("Cards: 999\n","exceeds the audit maximum")):
            retro.write_text(line,encoding="utf-8"); self.assertIn(message,mob.retro_cards(self.root,maximum=50)[1][0])
        self.valid_record(); retro.write_text("Cards: 201\n",encoding="utf-8")
        self.assertIn("Card 201 record retro is missing required sections or Process improvement pick",mob.check_records(self.root,[issue(201)],self.git))

    def test_closed_card_without_retro_cover_is_reported(self):
        # Given a retro whose Cards line covers card 5 only
        (self.root / "docs/retros/r.md").write_text("Cards: 5\n", encoding="utf-8")
        # When the detector checks closed cards 5 and 6
        errors = mob.check_retros(self.root, {5, 6})
        # Then exactly card 6 is reported (TAKEOVER by the facilitator: the mutant that empties check_retros survived round 2)
        self.assertEqual(["Card 006 has no retro Cards entry"], errors)

    def test_workflow_bypasses_previous_checker_and_codeowners(self):
        self.assertEqual([],mob.check_workflows(self.root))
        (self.root/".github/workflows/ci.yml").write_text("jobs:\n  mob-process:\n    continue-on-error: true\n    steps:\n      - run: python3 scripts/mob_process.py check || true\n",encoding="utf-8")
        self.assertIn("ci.yml must contain the mob-process job running the checker",mob.check_workflows(self.root))
        (self.root/".github/CODEOWNERS").write_text("",encoding="utf-8"); self.assertIn("CODEOWNERS must protect /.github/CODEOWNERS",mob.check_workflows(self.root))

    def test_labels_and_missing_title_prefix_are_exact(self):
        self.assertEqual(["Closed issue #1009 has neither card nor not-card label"],mob.check_issue_labels([issue(9,labels=())]))
        item=issue(201); item["title"]="work"; self.assertEqual(["Closed card issue #1201 title has no (N) prefix"],mob.check_issue_labels([item]))

    def test_mob_stale_ancestor_done_and_bytes_are_separate(self):
        base=self.git("rev-parse","--short","HEAD"); original=mob.render_mob(self.root,[],self.git,base); (self.root/"MOB.md").write_text(original,encoding="utf-8",newline="\n"); self.git("add","MOB.md"); self.git("commit","-m","mob")
        for n in range(4): (self.root/f"x{n}").write_text(str(n),encoding="utf-8"); self.git("add",f"x{n}"); self.git("commit","-m",f"change {n}")
        self.assertIn("MOB.md is 4 non-MOB commits stale (maximum 3)",mob.check_mob(self.root,[],self.git))
        (self.root/"MOB.md").write_text(original.replace(f'"as_of":"{base}"','"as_of":"deadbeef"'),encoding="utf-8"); self.assertEqual(["MOB.md as_of is not an ancestor of HEAD"],mob.check_mob(self.root,[],self.git))
        (self.root/"MOB.md").write_text(original.replace('"done":[]','"done":[999]'),encoding="utf-8"); self.assertIn("MOB.md done list does not equal record and legacy cards",mob.check_mob(self.root,[],self.git))
        (self.root/"MOB.md").write_text(original+"changed\n",encoding="utf-8"); self.assertIn("MOB.md is not byte-identical to write-mob-md output",mob.check_mob(self.root,[],self.git))
        (self.root/"MOB.md").write_bytes(original.replace("\n", "\r\n").encode("utf-8")); self.assertIn("MOB.md is not byte-identical to write-mob-md output",mob.check_mob(self.root,[],self.git))

    def test_generator_is_deterministic(self):
        data=mob.render_mob(self.root,[],self.git); self.assertEqual(data,mob.render_mob(self.root,[],self.git)); self.assertIn(self.git("show","-s","--format=%cI","HEAD"),data); self.assertNotRegex(data,r"[0-9a-f]{40}")

    def test_issue_collection_limit_and_retries(self):
        calls=[]
        def runner(args): calls.append(args); return [{}]*mob.GH_LIMIT
        with self.assertRaisesRegex(RuntimeError,"audit is incomplete"): mob.read_issues(runner)
        self.assertIn("5000",calls[0])
        with mock.patch.object(mob,"command",side_effect=RuntimeError("offline")) as run, mock.patch.object(mob.time,"sleep"):
            with self.assertRaisesRegex(RuntimeError,"after 3 attempts"): mob.gh_json(["repo","view"])
        self.assertEqual(3,run.call_count)

if __name__ == "__main__": unittest.main()
