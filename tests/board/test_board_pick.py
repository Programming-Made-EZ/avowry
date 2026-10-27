import json
import unittest

from scripts.board import check_real_board


def item(number, status, rank=None, depends="none"):
    x = {"id": f"PVTI_{number}", "title": f"({number:03d}) card", "status": status, "card number": number, "content": {"number": number + 100}}
    if rank is not None:
        x["rank"] = rank
    if depends is not None:
        x["depends on"] = depends
    return x


class FakeGH:
    def __init__(self, items):
        self.items = items
        self.calls = []

    def __call__(self, args, **kwargs):
        self.calls.append(list(args))
        assert args[:2] == ["project", "item-list"], args
        return json.dumps({"items": self.items, "totalCount": len(self.items)})


class PickCheckTests(unittest.TestCase):
    def test_expected_pick_is_the_first_ready_by_rank_with_all_dependencies_done(self):
        items = [item(1, "Done", 1), item(2, "Ready", 2, "003"), item(3, "Ready", 3, "001"), item(4, "Ready", 4, "none (the stash Decider runs first)")]
        self.assertEqual(check_real_board.expected_pick(items), "003")

    def test_a_missing_depends_on_is_not_none_and_a_superseded_dependency_blocks(self):
        items = [item(1, "Superseded/Withdrawn", 1), item(2, "Ready", 2, None), item(3, "Ready", 3, "001"), item(4, "Ready", 4, "none")]
        self.assertEqual(check_real_board.expected_pick(items), "004")

    def test_verify_pick_passes_when_the_picker_agrees_and_fails_when_it_does_not(self):
        fake = FakeGH([item(1, "Done", 1), item(2, "Ready", 2, "001")])
        self.assertEqual(check_real_board.verify_pick(fake, lambda: "002 (002) card"), "002")
        with self.assertRaises(RuntimeError):
            check_real_board.verify_pick(fake, lambda: "009 (009) card")

    def test_verify_pick_is_read_only(self):
        fake = FakeGH([item(1, "Ready", 1)])
        check_real_board.verify_pick(fake, lambda: "001 (001) card")
        self.assertTrue(all(call[:2] == ["project", "item-list"] for call in fake.calls))


if __name__ == "__main__":
    unittest.main()
