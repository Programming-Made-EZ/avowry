import tempfile
import unittest
from pathlib import Path
from unittest import mock

from scripts.board import check_real_board, generate_archive


class BodiesAfterCutoverTests(unittest.TestCase):
    def board(self, cut_over):
        tmp = tempfile.TemporaryDirectory(ignore_cleanup_errors=True)
        self.addCleanup(tmp.cleanup)
        board = Path(tmp.name)
        (board / "001-x.md").write_text("# X\n", encoding="utf-8")
        if cut_over:
            (board / "CUTOVER").write_text("cut over\n", encoding="utf-8")
        return board

    def test_after_the_cutover_bodies_are_checked_against_the_generated_archive(self):
        # Given a board that has cut over, the archive check (not the retired file migrator) decides
        with mock.patch.object(generate_archive, "main", return_value=0) as archive:
            count = check_real_board.verify_bodies(lambda *a, **k: "", board_dir=self.board(True))
        self.assertEqual((count, archive.call_args[0][0]), (1, ["--check"]))

    def test_after_the_cutover_a_stale_archive_fails_the_check(self):
        with mock.patch.object(generate_archive, "main", return_value=1):
            with self.assertRaises(RuntimeError):
                check_real_board.verify_bodies(lambda *a, **k: "", board_dir=self.board(True))


if __name__ == "__main__":
    unittest.main()
