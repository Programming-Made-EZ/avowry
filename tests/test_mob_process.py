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

if __name__ == "__main__": unittest.main()
