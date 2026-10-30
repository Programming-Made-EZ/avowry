import contextlib, datetime as dt, importlib.util, io, json, subprocess, tempfile, unittest
from pathlib import Path
from unittest import mock

SPEC = importlib.util.spec_from_file_location("mob_process", Path(__file__).resolve().parents[1] / "scripts/mob_process.py")
mob = importlib.util.module_from_spec(SPEC); SPEC.loader.exec_module(mob)

def issue(card, labels=("card",), closed="2099-01-01T00:00:00Z", events=()):
    return {"number": card + 1000, "title": f"({card}) work", "labels": list(labels), "closedAt": closed, "events": list(events)}

class MobProcessTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.root = Path(self.temp.name)
        for name in (".mob/records", "docs/decisions", "docs/retros", ".github/workflows", "board"):
            (self.root / name).mkdir(parents=True, exist_ok=True)
        digest = mob.cards_hash([])
        patcher = mock.patch.object(mob, "FROZEN_LEGACY_SHA256", digest); patcher.start(); self.addCleanup(patcher.stop)
        (self.root / ".mob/records/LEGACY.json").write_text(json.dumps({"schema": 1, "decision": "D134", "cutoff": "2026-10-29T20:35:37Z", "cards": [], "cards_sha256": digest}), encoding="utf-8")
        (self.root / "docs/decisions/process-and-staffing.md").write_text(f"D134 sha256 of the list: {digest}\n<a id=\"d7\"></a> Card 201", encoding="utf-8")
        (self.root / "DRIVERS.md").write_text("| Driver | Status |\n|---|---|\n| Codex | available |\n| Claude | available |", encoding="utf-8")
        workflow = """on:
  push:
    branches: [main]
jobs:
  mob-process:
    steps:
      - env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: python3 scripts/mob_process.py check
      - env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: git show ${{ github.event.before }}:scripts/mob_process.py > mob_process_prev.py && python3 mob_process_prev.py check --root . --skip-regeneration
"""
        (self.root / ".github/workflows/ci.yml").write_text(workflow, encoding="utf-8")
        event_workflow = """on:
  issues:
    types: [closed, reopened, unlabeled]
  schedule:
    - cron: '*/30 * * * *'
jobs:
  mob-process:
    steps:
      - env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: python3 scripts/mob_process.py check
"""
        (self.root / ".github/workflows/mob-process.yml").write_text(event_workflow, encoding="utf-8")
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

    def test_same_model_seats_pass_when_contexts_differ(self):
        # D145 C4: seat identity is role and context, so Claude ctx-4 green and Claude ctx-7 signoff are distinct seats.
        record=self.valid_record(204); seats={s["role"]:s for s in record["seats"]}
        seats["green"]["seat"]="Claude ctx-4"; seats["signoff"]["seat"]="Claude ctx-7"
        (self.root/".mob/records/204.json").write_text(json.dumps(record),encoding="utf-8")
        self.assertNotIn("Card 204 signoff seat must differ from green seat",mob.check_records(self.root,[issue(204)],self.git))
        seats["signoff"]["seat"]="  claude   CTX-4 "
        (self.root/".mob/records/204.json").write_text(json.dumps(record),encoding="utf-8")
        self.assertIn("Card 204 signoff seat must differ from green seat",mob.check_records(self.root,[issue(204)],self.git))

    def test_seat_identity_is_driver_and_context_id(self):
        # D145 C4 review of f04ac7f: same driver needs two distinct ctx ids; context-less or punctuation-varied seats are refused.
        msg="Card 204 signoff seat must differ from green seat"
        def errors(green,signoff):
            record=self.valid_record(204); seats={s["role"]:s for s in record["seats"]}
            seats["green"]["seat"]=green; seats["signoff"]["seat"]=signoff
            (self.root/".mob/records/204.json").write_text(json.dumps(record),encoding="utf-8")
            return mob.check_records(self.root,[issue(204)],self.git)
        for green,signoff in (("Claude","Claude Sonnet"),("Claude","claude (sonnet)"),("Claude ctx-4","Claude ctx-4."),("Claude ctx-4","Claude (ctx-4)"),("Claude ctx-4","Claude"),("Claude ctx-4 ctx-7","Claude ctx-7"),("Claude ctx-7","Claude ctx-4 ctx-7"),("Claude CTX-04","Claude ctx-4"),("Claude ctx-4","Claude ctx-004")):
            with self.subTest(green=green,signoff=signoff): self.assertIn(msg,errors(green,signoff))
        for green,signoff in (("Claude ctx-4","Claude ctx-7"),("Codex","Claude")):
            with self.subTest(green=green,signoff=signoff): self.assertNotIn(msg,errors(green,signoff))

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

    def test_legacy_list_must_equal_the_pin_inside_the_checker(self):
        # Given a legacy list extended together with its own hash and the decision text (all consistent with each other)
        legacy = json.loads((self.root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))
        legacy["cards"] = [1]; legacy["cards_sha256"] = mob.cards_hash([1])
        (self.root / ".mob/records/LEGACY.json").write_text(json.dumps(legacy), encoding="utf-8")
        decision = self.root / "docs/decisions/process-and-staffing.md"
        decision.write_text(f"D134 sha256 of the list: {mob.cards_hash([1])}\n", encoding="utf-8")
        # When the legacy check runs
        errors = mob.check_legacy(self.root, [])
        # Then only the pin held in the checker itself catches the extension
        self.assertEqual(["LEGACY.json differs from the frozen list pinned in the checker (D134/D135)"], errors)

    def test_optional_refactor_seat_is_accepted_and_empty_fields_are_rejected(self):
        # Given a valid record with an extra refactor seat
        record = self.valid_record(); path = self.root / ".mob/records/201.json"
        decision = self.root / "docs/decisions/process-and-staffing.md"
        decision.write_text(decision.read_text(encoding="utf-8") + '\n<a id="d2016"></a> Card 201', encoding="utf-8")
        record["seats"].append({"role": "refactor", "seat": "Codex", "model": "test", "evidence": "D2016"})
        path.write_text(json.dumps(record), encoding="utf-8")
        # When the records are checked
        accepted = mob.check_records(self.root, [issue(201)], self.git)
        # Then the extra seat is fine, but a duplicate role or an empty model is not
        self.assertEqual([], accepted)
        record["seats"].append(dict(record["seats"][-1])); path.write_text(json.dumps(record), encoding="utf-8")
        self.assertIn("Card 201 must have exactly the six required role seats", mob.check_records(self.root, [issue(201)], self.git))
        record["seats"].pop(); record["seats"][0]["model"] = " "; path.write_text(json.dumps(record), encoding="utf-8")
        self.assertIn("Card 201 has a seat with an empty seat, model or evidence field", mob.check_records(self.root, [issue(201)], self.git))

    def test_ci_skips_a_previous_checker_that_predates_the_flags(self):
        # Given the real ci.yml of this repository
        text = (Path(__file__).resolve().parents[1] / ".github/workflows/ci.yml").read_text(encoding="utf-8")
        # When it runs the previous checker version
        step = text[text.index("Check with previous checker"):]
        # Then it first checks that the old file knows --skip-regeneration, so the first rollout of the flags cannot turn main red
        self.assertIn('grep -q -- "--skip-regeneration" "$RUNNER_TEMP/mob_process_prev.py"', step)
        self.assertLess(step.index('grep -q -- "--skip-regeneration"'), step.index("check --root . --skip-regeneration"))

    def test_non_string_empty_or_malformed_seats_are_reported_not_crashes(self):
        # Given a valid record whose seat fields are damaged in the ways a careless or hostile record could be
        record = self.valid_record(); path = self.root / ".mob/records/201.json"
        message = "Card 201 has a seat with an empty seat, model or evidence field"
        for seat_index, key, value in ((0, "model", None), (0, "model", 0), (1, "seat", None), (2, "evidence", ["x"])):
            damaged = json.loads(json.dumps(record)); damaged["seats"][seat_index][key] = value
            path.write_text(json.dumps(damaged), encoding="utf-8")
            # When the records are checked
            errors = mob.check_records(self.root, [issue(201)], self.git)
            # Then the damage is a reported violation, never an exception or a pass
            self.assertIn(message, errors, (seat_index, key, value))
        damaged = json.loads(json.dumps(record)); damaged["seats"][0] = "not a seat"
        path.write_text(json.dumps(damaged), encoding="utf-8")
        self.assertIn(message, mob.check_records(self.root, [issue(201)], self.git))

    def test_a_wrongly_typed_legacy_field_fails_the_check_instead_of_crashing(self):
        # Given a legacy list whose cards field is a number instead of a list
        legacy_path = self.root / ".mob/records/LEGACY.json"
        legacy = json.loads(legacy_path.read_text(encoding="utf-8")); legacy["cards"] = 5
        legacy_path.write_text(json.dumps(legacy), encoding="utf-8")
        # When the whole detector runs
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            code = mob.run_check(self.root, [issue(201)], git=self.git)
        # Then the run fails closed with a reported error, not an exception
        self.assertEqual(1, code)
        self.assertIn("check could not read its input", out.getvalue())

    def test_a_seat_with_an_invented_role_is_rejected(self):
        # Given a valid record with an extra seat whose role is not one of the known roles
        record = self.valid_record(); path = self.root / ".mob/records/201.json"
        record["seats"].append({"role": "wizard", "seat": "Codex", "model": "test", "evidence": "D2010"})
        path.write_text(json.dumps(record), encoding="utf-8")
        # When the records are checked
        errors = mob.check_records(self.root, [issue(201)], self.git)
        # Then only the six required roles and the optional refactor role are accepted
        self.assertIn("Card 201 must have exactly the six required role seats", errors)

    def test_a_record_retro_that_does_not_list_the_card_is_rejected(self):
        # Given a valid record whose retro file has all sections but a Cards line for another card
        self.valid_record()
        (self.root / "docs/retros/r.md").write_text("Cards: 999\n# Individual\n# Team\n# Organisation\n# Product\nProcess improvement pick\n", encoding="utf-8")
        # When the records are checked
        errors = mob.check_records(self.root, [issue(201)], self.git)
        # Then the record is reported as not covered by its retro
        self.assertIn("Card 201 record retro does not cover the card", errors)

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

    def test_record_first_commit_after_issue_closure_is_reported_exactly(self):
        # Given Morgan audits a record whose issue closed one hour before the record's own commit
        self.valid_record()
        committed = dt.datetime.fromisoformat(self.git("show", "-s", "--format=%cI", "HEAD"))
        closed = (committed - dt.timedelta(hours=1)).isoformat()
        # When the trail record is checked
        errors = mob.check_records(self.root, [issue(201, closed=closed)], self.git)
        # Then the temporal violation is named exactly
        self.assertIn("Card 201 record was first committed after issue closure", errors)

    def test_commit_hash_evidence_succeeds_for_a_reachable_matching_commit(self):
        # Given a real reachable commit names the card and predates its trail record
        (self.root / "work.txt").write_text("done", encoding="utf-8")
        self.git("add", "work.txt")
        self.git("commit", "-m", "Card 201: working increment")
        evidence = self.git("rev-parse", "HEAD")
        record = self.valid_record()
        record["seats"][0]["evidence"] = evidence
        (self.root / ".mob/records/201.json").write_text(json.dumps(record), encoding="utf-8")
        closed = (dt.datetime.fromisoformat(self.git("show", "-s", "--format=%cI", evidence)) + dt.timedelta(hours=1)).isoformat()
        # When commit evidence is checked
        errors = mob.check_records(self.root, [issue(201, closed=closed)], self.git)
        # Then no commit-evidence error is reported
        self.assertFalse(any(f"Card 201 evidence {evidence}" in error for error in errors), errors)

    def test_commit_hash_evidence_not_reachable_from_head_is_reported_exactly(self):
        # Given a trail record cites a real commit that is not reachable from HEAD
        record = self.valid_record()
        self.git("switch", "--detach")
        self.git("commit", "--allow-empty", "-m", "Card 201: detached evidence")
        evidence = self.git("rev-parse", "HEAD")
        self.git("switch", "main")
        record["seats"][0]["evidence"] = evidence
        (self.root / ".mob/records/201.json").write_text(json.dumps(record), encoding="utf-8")
        # When commit evidence is checked
        errors = mob.check_records(self.root, [issue(201)], self.git)
        # Then the reachability failure is named exactly
        self.assertIn(f"Card 201 evidence {evidence} is not a reachable matching commit", errors)

    def test_commit_hash_evidence_message_must_name_the_card(self):
        # Given a reachable commit does not name the card in its message
        record = self.valid_record()
        evidence = self.git("rev-parse", "HEAD~1")
        record["seats"][0]["evidence"] = evidence
        (self.root / ".mob/records/201.json").write_text(json.dumps(record), encoding="utf-8")
        # When commit evidence is checked
        errors = mob.check_records(self.root, [issue(201)], self.git)
        # Then the matching-message failure is named exactly
        self.assertIn(f"Card 201 evidence {evidence} is not a reachable matching commit", errors)

    def test_commit_hash_evidence_cannot_be_the_records_own_commit(self):
        # Given a trail record cites the commit that first added that record
        record = self.valid_record()
        evidence = self.git("rev-parse", "HEAD")
        record["seats"][0]["evidence"] = evidence
        (self.root / ".mob/records/201.json").write_text(json.dumps(record), encoding="utf-8")
        # When commit evidence is checked
        errors = mob.check_records(self.root, [issue(201)], self.git)
        # Then self-evidence is named exactly
        self.assertIn(f"Card 201 evidence {evidence} is the record's own commit", errors)

    def test_commit_hash_evidence_after_closure_is_reported_exactly(self):
        # Given a matching commit is dated one hour after issue closure
        self.valid_record()
        evidence = self.git("rev-parse", "HEAD")
        record = json.loads((self.root / ".mob/records/201.json").read_text(encoding="utf-8"))
        record["seats"][0]["evidence"] = evidence
        (self.root / ".mob/records/201.json").write_text(json.dumps(record), encoding="utf-8")
        committed = dt.datetime.fromisoformat(self.git("show", "-s", "--format=%cI", evidence))
        # When commit evidence is checked against an earlier closure
        errors = mob.check_records(self.root, [issue(201, closed=(committed - dt.timedelta(hours=1)).isoformat())], self.git)
        # Then the closure violation is named exactly
        self.assertIn(f"Card 201 evidence {evidence} is dated after issue closure", errors)

    def test_commit_hash_evidence_cannot_be_reused_by_another_card(self):
        # Given one real commit names two cards and both records cite it
        (self.root / "shared.txt").write_text("done", encoding="utf-8")
        self.git("add", "shared.txt")
        self.git("commit", "-m", "Card 201 and Card 202: shared evidence")
        evidence = self.git("rev-parse", "HEAD")
        for card in (201, 202):
            record = self.valid_record(card)
            record["seats"][0]["evidence"] = evidence
            (self.root / f".mob/records/{card:03}.json").write_text(json.dumps(record), encoding="utf-8")
        closed = (dt.datetime.fromisoformat(self.git("show", "-s", "--format=%cI", evidence)) + dt.timedelta(hours=1)).isoformat()
        # When both records are checked
        errors = mob.check_records(self.root, [issue(201, closed=closed), issue(202, closed=closed)], self.git)
        # Then reuse by the later card is named exactly
        self.assertIn(f"Card 202 evidence {evidence} is reused by Card 201", errors)

    def test_cards_line_rejects_one_to_three_words(self):
        # Given a retro uses words instead of the numeric range syntax
        (self.root / "docs/retros/r.md").write_text("Cards: 1 to 3\n", encoding="utf-8")
        # When the retro is parsed
        errors = mob.retro_cards(self.root)[1]
        # Then the invalid line is reported exactly
        self.assertEqual([f"{(self.root / 'docs/retros/r.md').as_posix()} has an invalid Cards line"], errors)

    def test_cards_line_rejects_letters(self):
        # Given a retro uses letters in its Cards line
        (self.root / "docs/retros/r.md").write_text("Cards: abc\n", encoding="utf-8")
        # When the retro is parsed
        errors = mob.retro_cards(self.root)[1]
        # Then the invalid line is reported exactly
        self.assertEqual([f"{(self.root / 'docs/retros/r.md').as_posix()} has an invalid Cards line"], errors)

    def test_cards_line_rejects_a_trailing_comma(self):
        # Given a retro has a trailing comma in its Cards line
        (self.root / "docs/retros/r.md").write_text("Cards: 1,\n", encoding="utf-8")
        # When the retro is parsed
        errors = mob.retro_cards(self.root)[1]
        # Then the invalid line is reported exactly
        self.assertEqual([f"{(self.root / 'docs/retros/r.md').as_posix()} has an invalid Cards line"], errors)

    def test_missing_mob_process_workflow_is_reported(self):
        # Given the event workflow is missing
        (self.root / ".github/workflows/mob-process.yml").unlink()
        # When workflow wiring is checked
        errors = mob.check_workflows(self.root)
        # Then the missing workflow is reported exactly
        self.assertIn("mob-process.yml must run for issue events and on a schedule", errors)

    def test_card_number_comes_from_title_prefix_not_issue_number(self):
        # Given issue 1201 has card 9 in its title prefix
        item = issue(9)
        item["number"] = 1201
        # When its card number is read
        card = mob.card_number(item)
        # Then the title prefix wins
        self.assertEqual(9, card)

    def test_read_issues_flattens_slurped_timeline_pages(self):
        # Given GitHub returns timeline pages as a list of lists
        def runner(args):
            if args[:2] == ["repo", "view"]:
                return {"nameWithOwner": "o/r"}
            if args[0] == "api":
                return [[{"event": "closed"}], [{"event": "reopened"}]]
            return [issue(1)]
        # When issues are collected
        items = mob.read_issues(runner)
        # Then the timeline is flattened in order
        self.assertEqual([{"event": "closed"}, {"event": "reopened"}], items[0]["events"])

    def test_read_issues_accepts_a_flat_timeline_list(self):
        # Given a fake runner returns an already-flat timeline
        def runner(args):
            if args[:2] == ["repo", "view"]:
                return {"nameWithOwner": "o/r"}
            if args[0] == "api":
                return [{"event": "closed"}, {"event": "reopened"}]
            return [issue(1)]
        # When issues are collected
        items = mob.read_issues(runner)
        # Then each event remains an event rather than being iterated as a dictionary
        self.assertEqual([{"event": "closed"}, {"event": "reopened"}], items[0]["events"])

    def test_legacy_json_is_read_as_utf8(self):
        # Given LEGACY.json contains a non-ASCII note and the platform default is unusable
        path = self.root / ".mob/records/LEGACY.json"
        data = json.loads(path.read_text(encoding="utf-8"))
        data["note"] = "café"
        path.write_text(json.dumps(data, ensure_ascii=False), encoding="utf-8")
        # When the legacy waiver is checked
        errors = mob.check_legacy(self.root, [])
        # Then UTF-8 content is readable
        self.assertEqual([], errors)

    def test_workflow_rejects_if_false(self):
        # Given a checker job is disabled with if false
        block = "mob-process:\n  if: false\n  run: python3 scripts/mob_process.py check\n"
        # When it is validated
        valid = mob.valid_job(block)
        # Then it is unsafe
        self.assertFalse(valid)

    def test_workflow_rejects_continue_on_error(self):
        # Given a checker job tolerates failure
        block = "mob-process:\n  continue-on-error: true\n  run: python3 scripts/mob_process.py check\n"
        # When it is validated
        valid = mob.valid_job(block)
        # Then it is unsafe
        self.assertFalse(valid)

    def test_workflow_rejects_shell_or_true(self):
        # Given a checker command masks failure with shell or true
        block = "mob-process:\n  run: python3 scripts/mob_process.py check || true\n"
        # When it is validated
        valid = mob.valid_job(block)
        # Then it is unsafe
        self.assertFalse(valid)

    def test_ci_workflow_requires_the_previous_checker(self):
        # Given a safe current-checker job has no previous-checker step
        block = "mob-process:\n  run: python3 scripts/mob_process.py check\n"
        # When CI-level validation requires previous behavior
        valid = mob.valid_job(block, previous=True)
        # Then the job is incomplete
        self.assertFalse(valid)

    def test_event_workflow_excludes_the_previous_checker(self):
        # Given the issue-and-schedule workflow runs only the current checker
        block = mob.job_block((self.root / ".github/workflows/mob-process.yml").read_text(encoding="utf-8"))
        # When event-workflow validation does not require a previous revision
        valid = mob.valid_job(block, previous=False)
        # Then the job is valid
        self.assertTrue(valid)

    def test_every_checker_step_requires_gh_token(self):
        # Given a checker step omits GH_TOKEN
        block = "mob-process:\n  steps:\n    - run: python3 scripts/mob_process.py check\n"
        # When the job is validated
        valid = mob.valid_job(block)
        # Then it is rejected because GitHub CLI cannot authenticate
        self.assertFalse(valid)

    def test_skip_regeneration_preserves_other_mob_checks(self):
        # Given MOB.md has valid state but differs from regenerated presentation bytes
        base = self.git("rev-parse", "--short", "HEAD")
        original = mob.render_mob(self.root, [], self.git, base)
        (self.root / "MOB.md").write_text(original + "changed\n", encoding="utf-8")
        # When only regeneration comparison is skipped
        errors = mob.check_mob(self.root, [], self.git, skip_regeneration=True)
        # Then the byte difference is skipped and all structural checks still pass
        self.assertEqual([], errors)

    def test_skip_regeneration_does_not_skip_the_done_list_check(self):
        # Given MOB.md has presentation drift and a false done list
        base = self.git("rev-parse", "--short", "HEAD")
        original = mob.render_mob(self.root, [], self.git, base)
        changed = original.replace('"done":[]', '"done":[999]') + "changed\n"
        (self.root / "MOB.md").write_text(changed, encoding="utf-8")
        # When byte-for-byte regeneration is skipped
        errors = mob.check_mob(self.root, [], self.git, skip_regeneration=True)
        # Then the independent done-list invariant still fails exactly
        self.assertEqual(["MOB.md done list does not equal record and legacy cards"], errors)

    # Shared helper (card 213 stand-down): a minimal valid tree (MOB.md as in the Done check 1 test, an empty decisions folder), optionally one complete window.
    def minimal_tree(self, window=None, **fields):
        (self.root / "MOB.md").write_bytes(mob.render_mob(self.root, [], self.git).encode("utf-8"))
        folder = self.root / ".mob/records/decisions"; folder.mkdir(parents=True, exist_ok=True)
        if window is None: return
        seat = {"model": "m", "context": "c", "started_at": "2026-10-30T07:30:05Z", "reply": "NO OBJECTION", "condition": None, "raw": "NO OBJECTION", "probe_reference": "p"}
        data = {"window": window, "category": "slice decision", "ended_at": "2026-10-30T07:38:24Z", "replies": [dict(seat, role="architect"), dict(seat, role="QA engineer")]}
        data.update(fields); (folder / f"{window}.json").write_text(json.dumps(data), encoding="utf-8")

    # Card 213 unit layer 1 (D153 L3b), ZOMBIES: Z (no decisions folder: run_check passes, covered by the existing tests), O (one bad window makes run_check return 1),
    # M (a second incomplete window is also reported), B (the error line is an ::error::mob-process: line), I (the line names the file and the field).
    # E: N/A because invalid JSON is later work. S: N/A because nothing simpler exists.
    def test_run_check_wires_in_the_window_check(self):
        (self.root / "MOB.md").write_bytes(mob.render_mob(self.root, [], self.git).encode("utf-8"))
        folder = self.root / ".mob/records/decisions"; folder.mkdir(parents=True)
        for name, fields in (("330-a.json", {"category": "sixth", "ended_at": "x"}), ("331-b.json", {"category": "slice decision", "ended_at": ""})):
            (folder / name).write_text(json.dumps(fields), encoding="utf-8")
        out = io.StringIO()
        with contextlib.redirect_stdout(out): code = mob.run_check(self.root, [], git=self.git)
        lines = out.getvalue().splitlines()
        self.assertEqual(1, code)
        self.assertTrue(any(x.startswith("::error::mob-process:") and "330-a.json" in x and "category" in x for x in lines), lines)
        self.assertTrue(any(x.startswith("::error::mob-process:") and "331-b.json" in x and "ended_at" in x for x in lines), lines)

    # Card 213 Done check 1, ZOMBIES: Z (no decisions folder, empty folder: born-green guard), O (one complete window passes: born-green guard),
    # M (several windows, only the incomplete one is named), B (category outside the closed L1 list, different capitals, missing or empty ended_at),
    # I (the message names the file and the field). E (invalid JSON) and S are left to later increments. The real windows must also pass (born-green guard).
    def test_window_files_need_a_closed_list_category_and_an_end_time(self):
        (self.root / "MOB.md").write_bytes(mob.render_mob(self.root, [], self.git).encode("utf-8"))
        def check():
            out = io.StringIO()
            with contextlib.redirect_stdout(out): code = mob.run_check(self.root, [], git=self.git)
            return code, out.getvalue()
        def window(name, **fields):
            data = {"window": name, "category": "slice decision", "ended_at": "2026-10-30T07:38:24Z", "replies": []}
            data.update(fields); folder = self.root / ".mob/records/decisions"; folder.mkdir(parents=True, exist_ok=True)
            (folder / f"{name}.json").write_text(json.dumps(data), encoding="utf-8")
        # Given no decisions folder, then an empty one (guards), the check passes
        self.assertEqual(0, check()[0]); (self.root / ".mob/records/decisions").mkdir(); self.assertEqual(0, check()[0])
        # And one complete window, then every listed category (guards), passes
        window("301-ok"); self.assertEqual(0, check()[0])
        for i, category in enumerate(("task-force declaration", "slice decision", "loop-boundary re-rank", "card-text or declaration fix", "mid-card addition")):
            window(f"31{i}-listed", category=category)
        self.assertEqual(0, check()[0])
        # And the real windows of this repository pass
        for real in (Path(__file__).resolve().parents[1] / ".mob/records/decisions").glob("*.json"):
            (self.root / ".mob/records/decisions" / real.name).write_bytes(real.read_bytes())
        self.assertEqual(0, check()[0])
        # When a window has a category outside the list, wrong capitals, no end time or an empty one, only that file is named with its field
        for name, fields, field in (("320-sixth", {"category": "sixth kind"}, "category"), ("321-caps", {"category": "Slice Decision"}, "category"),
                                    ("322-noend", {"ended_at": None}, "ended_at"), ("323-emptyend", {"ended_at": ""}, "ended_at")):
            with self.subTest(name):
                window(name, **fields)
                # Then the check fails, names the file and the field, and names no complete window
                code, text = check(); (self.root / ".mob/records/decisions" / f"{name}.json").unlink()
                self.assertEqual(1, code); self.assertTrue(any(name in x and field in x for x in text.splitlines()), text)
                self.assertNotIn("301-ok", text)


    # Card 213 Done check 2 (D153 L3b), ZOMBIES: Z (a seat with all fields and an empty replies list: born-green guards), O (one complete window passes: born-green guard),
    # M (two seats, one defective: only that seat is named), B (model, context, reply, probe_reference each missing or empty; a reply outside NO OBJECTION/OBJECTION; OBJECTION with an empty condition;
    # an OBJECTION with a condition passes: guard; attempt-1 entries in an attempts list are not replies: guard; the real windows pass: guard),
    # E (a seat without a reply entry is named with its own field), I (the error line names the file, the seat and the field). S: N/A because nothing simpler exists.
    @unittest.skip('PENDING(213-2)')
    def test_every_seat_in_a_window_needs_model_context_answer_and_probe_reference(self):
        def check():
            out = io.StringIO()
            with contextlib.redirect_stdout(out): code = mob.run_check(self.root, [], git=self.git)
            return code, out.getvalue()
        path = self.root / ".mob/records/decisions/340-seats.json"
        # Given a complete window (guard), then an objection that carries a condition and an attempts entry that is not a reply (guards), the check passes
        self.minimal_tree("340-seats"); self.assertEqual(0, check()[0])
        data = json.loads(path.read_text(encoding="utf-8")); data["replies"][1].update(reply="OBJECTION", condition="adopt X")
        data["attempts"] = [{"attempt": 1, "result": "not counted", "seats": ["architect a"], "started_at": "t"}]
        path.write_text(json.dumps(data), encoding="utf-8"); self.assertEqual(0, check()[0])
        # And the real windows of this repository pass (guard)
        for real in (Path(__file__).resolve().parents[1] / ".mob/records/decisions").glob("*.json"):
            (self.root / ".mob/records/decisions" / real.name).write_bytes(real.read_bytes())
        self.assertEqual(0, check()[0])
        for real in (Path(__file__).resolve().parents[1] / ".mob/records/decisions").glob("*.json"): (self.root / ".mob/records/decisions" / real.name).unlink()
        # When one seat misses or empties a field, answers wrongly, or objects without a condition
        absent = object()
        cases = [(f"{how} {field}", {field: value}, field) for field in ("model", "context", "reply", "probe_reference") for how, value in (("empty", ""), ("null", None), ("absent", absent))]
        cases += [("answer outside the two", {"reply": "MAYBE"}, "reply"), ("objection without condition", {"reply": "OBJECTION", "condition": None}, "condition"),
                  ("objection with empty condition", {"reply": "OBJECTION", "condition": ""}, "condition")]
        for label, change, field in cases:
            with self.subTest(label):
                base = json.loads(path.read_text(encoding="utf-8")); base["attempts"] = []; base["replies"][1] = dict(base["replies"][1], reply="NO OBJECTION", condition=None)
                for key, value in change.items():
                    if value is absent: del base["replies"][1][key]
                    else: base["replies"][1][key] = value
                path.write_text(json.dumps(base), encoding="utf-8")
                # Then the check fails, and the error line names the file, only the defective seat and the field
                code, text = check(); lines = [x for x in text.splitlines() if x.startswith("::error::mob-process:")]
                self.assertEqual(1, code)
                self.assertTrue(any("340-seats.json" in x and "QA engineer" in x and field in x for x in lines), text)
                self.assertFalse(any("architect" in x for x in lines), text)

WINDOW_CATEGORIES = ("task-force declaration", "slice decision", "loop-boundary re-rank", "card-text or declaration fix", "mid-card addition")

class WindowErrorsTests(unittest.TestCase):
    path = Path(".mob/records/decisions/301-x.json")
    def complete(self, **fields):
        data = {"category": "slice decision", "ended_at": "2026-10-30T07:38:24Z", "replies": [], "raw": "extra"}
        data.update(fields); return data

    # Card 213 unit layer 3 (D153 L3b), ZOMBIES: Z (empty data: both fields reported), O (one complete window: no errors; extra keys ignored),
    # M (category and ended_at both bad: two errors), B (each listed category passes; sixth, other capitals, padded, non-string category fail; empty, blank, non-string, missing ended_at fail),
    # I (each message names the file and the field). E: N/A because invalid JSON is read by check_windows (layer 2), the pure function gets parsed data. S: N/A because nothing simpler exists.
    def test_zero_empty_data_reports_both_fields(self):
        errors = mob.window_errors(self.path, {})
        self.assertEqual(2, len(errors)); self.assertTrue(any("category" in e for e in errors)); self.assertTrue(any("ended_at" in e for e in errors))
    def test_one_complete_window_has_no_errors_and_extra_keys_are_ignored(self):
        self.assertEqual([], mob.window_errors(self.path, self.complete()))
    def test_many_bad_fields_give_one_error_each(self):
        self.assertEqual(2, len(mob.window_errors(self.path, self.complete(category="sixth", ended_at=""))))
    def test_boundary_every_listed_category_passes(self):
        for category in WINDOW_CATEGORIES:
            with self.subTest(category): self.assertEqual([], mob.window_errors(self.path, self.complete(category=category)))
    def test_boundary_category_outside_the_list_fails(self):
        for category in ("sixth kind", "Slice Decision", " slice decision", "", None, 5, ["slice decision"]):
            with self.subTest(category): self.assertEqual(1, len(mob.window_errors(self.path, self.complete(category=category))))
    def test_boundary_missing_or_empty_end_time_fails(self):
        for ended in ("", "  ", None, 5):
            with self.subTest(ended): self.assertEqual(1, len(mob.window_errors(self.path, self.complete(ended_at=ended))))
        data = self.complete(); del data["ended_at"]; self.assertEqual(1, len(mob.window_errors(self.path, data)))
    def test_interface_messages_name_the_file_and_the_field(self):
        self.assertIn("301-x.json", mob.window_errors(self.path, self.complete(category="no"))[0]); self.assertIn("category", mob.window_errors(self.path, self.complete(category="no"))[0])
        self.assertIn("301-x.json", mob.window_errors(self.path, self.complete(ended_at=""))[0]); self.assertIn("ended_at", mob.window_errors(self.path, self.complete(ended_at=""))[0])

class WindowSeatsTests(unittest.TestCase):
    path = Path(".mob/records/decisions/302-y.json")
    base = {"category": "slice decision", "ended_at": "2026-10-30T07:38:24Z"}

    # Card 213 unit layer 4 (D153 L3b), outside-in: window_errors consumes seat_errors, which is patched here (test double: unittest.mock.patch.object(mob, "seat_errors", ...)).
    # ZOMBIES: Z (no replies key, or an empty list: the double is never called), O (one reply: the double gets (path, 0, that reply) and its errors are returned),
    # M (two replies: both indexes are asked, errors are concatenated in order after the window field errors), B (the attempts list is never passed to the double),
    # E (a reply entry that is not an object is named by file and index and the double is not called for it), I (the file name is passed on to the double).
    # S: N/A because nothing simpler exists.
    def test_zero_no_replies_never_ask_the_seat_check(self):
        with mock.patch.object(mob, "seat_errors", return_value=["x"]) as double:
            self.assertEqual([], mob.window_errors(self.path, dict(self.base))); self.assertEqual([], mob.window_errors(self.path, dict(self.base, replies=[])))
        double.assert_not_called()
    def test_one_reply_is_passed_to_the_seat_check_and_its_errors_returned(self):
        reply = {"role": "architect"}
        with mock.patch.object(mob, "seat_errors", return_value=["seat says no"]) as double:
            self.assertEqual(["seat says no"], mob.window_errors(self.path, dict(self.base, replies=[reply])))
        double.assert_called_once_with(self.path, 0, reply)
    def test_many_replies_are_asked_in_order_after_the_window_field_errors(self):
        replies = [{"role": "a"}, {"role": "b"}]
        with mock.patch.object(mob, "seat_errors", side_effect=lambda path, index, reply: [f"seat {index}"]) as double:
            errors = mob.window_errors(self.path, dict(self.base, ended_at="", replies=replies))
        self.assertEqual(3, len(errors)); self.assertIn("ended_at", errors[0]); self.assertEqual(["seat 0", "seat 1"], errors[1:])
        self.assertEqual([mock.call(self.path, 0, replies[0]), mock.call(self.path, 1, replies[1])], double.call_args_list)
    def test_boundary_attempts_are_never_passed_to_the_seat_check(self):
        with mock.patch.object(mob, "seat_errors", return_value=[]) as double:
            mob.window_errors(self.path, dict(self.base, replies=[], attempts=[{"attempt": 1}]))
        double.assert_not_called()
    def test_exception_a_reply_entry_that_is_not_an_object_is_named_by_file_and_index(self):
        with mock.patch.object(mob, "seat_errors", return_value=[]) as double:
            errors = mob.window_errors(self.path, dict(self.base, replies=["x"]))
        self.assertEqual(1, len(errors)); self.assertIn("302-y.json", errors[0]); self.assertIn("seat 1", errors[0]); double.assert_not_called()

class CheckWindowsTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.root = Path(self.temp.name); self.addCleanup(self.temp.cleanup)
        self.folder = self.root / ".mob/records/decisions"
    def write(self, name, **fields):
        data = {"category": "slice decision", "ended_at": "2026-10-30T07:38:24Z"}; data.update(fields)
        self.folder.mkdir(parents=True, exist_ok=True); (self.folder / name).write_text(json.dumps(data), encoding="utf-8")

    # Card 213 unit layer 2 (D153 L3b), ZOMBIES: Z (no decisions folder, then an empty one: no errors), O (one complete window: no errors),
    # M (several windows, only the incomplete one is named), B (a file not named NNN-slug.json is not a window and is ignored; the repository's real windows pass),
    # I (the error names the file and the field). E: N/A because invalid JSON is criterion 2 or later work. S: N/A because nothing simpler exists.
    def test_zero_no_folder_and_empty_folder_have_no_errors(self):
        self.assertEqual([], mob.check_windows(self.root)); self.folder.mkdir(parents=True); self.assertEqual([], mob.check_windows(self.root))
    def test_one_complete_window_has_no_errors(self):
        self.write("301-ok.json"); self.assertEqual([], mob.check_windows(self.root))
    def test_many_windows_name_only_the_incomplete_one(self):
        self.write("301-ok.json"); self.write("302-bad.json", category="sixth"); self.write("303-ok.json")
        errors = mob.check_windows(self.root)
        self.assertEqual(1, len(errors)); self.assertIn("302-bad.json", errors[0])
    def test_boundary_files_that_are_not_windows_are_ignored(self):
        self.write("notes.json", category="sixth"); self.write("301-bad.txt", category="sixth"); self.assertEqual([], mob.check_windows(self.root))
    def test_boundary_the_real_windows_of_the_repository_pass(self):
        self.assertEqual([], mob.check_windows(Path(__file__).resolve().parents[1]))
    def test_interface_the_error_names_the_file_and_the_field(self):
        self.write("304-noend.json", ended_at=""); errors = mob.check_windows(self.root)
        self.assertEqual(1, len(errors)); self.assertIn("304-noend.json", errors[0]); self.assertIn("ended_at", errors[0])

if __name__ == "__main__": unittest.main()
