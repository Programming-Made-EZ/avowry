# Card 178 cold-reader report (D206): Cursor (cursor-agent, ask mode, a trial driver; its first real seat)

Seat: a fresh read-only context on the Cursor CLI (`cursor-agent -p --trust --mode ask --output-format text`), in a scratch directory that held ONLY a copy of docs/PREREQUISITES.md (the page as committed in 5012d9c) and a TASK.md asking it to read as Dana, with no other context. It changed and ran nothing. Time: about two minutes. The task text it was given is in this record's second section. Its reply is verbatim below, followed by the facilitator's disposition.

## The reply (verbatim)
(1) No, I can't tell for sure. Line 5 says the only tested setup is ubuntu-latest with .NET SDK 8.0.x. Line 7 says no CPU architecture is named. Line 9 says every other OS or architecture is untested.

(2) On Windows, macOS or ARM, I'd install the .NET 8 SDK, confirm it with dotnet --list-sdks, and try it, knowing it's untested (line 9). If it failed, I'd stop and ask.

(3) WRONG:
- Line 9 calls "any other CPU architecture" untested, but line 7 names no tested architecture. "Other" than what?
- Line 18 says the SDK check "Needs: the .NET 8 SDK". That's circular. It only needs the dotnet CLI.
- Line 40 says install needs only "a packed local feed". dotnet tool install also needs the SDK.

CONFUSING:
- Line 11, "A newer ASP.NET Core runtime alone is not enough": does a .NET 9 SDK work, or only 8?
- Line 13 uses <feed> and <dir> before saying what they are.
- Line 3 says the page "matches what the CI workflows test". Does CI test pack and install, or only the build?
- Line 43 installs package ID "Avowry", but the project is Avowry.Cli (line 32). I'd have to guess they match.

FINE TO LEAVE: Line 5. Lines 17–22. "Same in PowerShell" (lines 24, 35, 46).

(4) Yes, a few things are missing:
- Lines 28 and 39 say "repository root", but the page never says how to get the repo.
- It doesn't say whether <feed> is a relative or absolute path.
- Install has no step to add <dir> to PATH, and no way to check it worked.
- The PowerShell forms are fine once <feed> and <dir> are filled in. Typed with the angle brackets, PowerShell rejects them as reserved characters.

## The task text it was given
You are Dana: a developer who directs an AI agent to build services. You have NO other context about this project. Read only the file PREREQUISITES.md in this directory. Change nothing and run nothing.
Answer, in under 250 words: (1) Before installing, do you know whether Avowry will work on your machine? State what the page tells you about the supported .NET SDK, operating systems and CPU architecture. (2) What would you do if your machine is Windows or macOS or an ARM machine? (3) Which parts are WRONG (contradict each other or are false by your reading), CONFUSING (you would stop or guess) and FINE TO LEAVE? Cite the line or sentence of the file for each. (4) Is any command missing where it should run from, what it needs, or a PowerShell form? Reply as plain text; nothing else.


## Facilitator's disposition
The checks of the card's test pass, but the story promises "I know before installing whether it will work for me" and the reader found items that break that promise. The card acts on them (a page-only green revision, the test unchanged): WRONG: "Needs: the .NET 8 SDK" on the SDK check is circular (it needs the dotnet command); "any other CPU architecture" has no tested architecture to be other than; the install needs the .NET 8 SDK as well. CONFUSING: whether a newer SDK works; `<feed>` and `<dir>` are used before they are explained; whether CI tests pack and install (it does, through the acceptance suite); the package ID Avowry against the project name Avowry.Cli. MISSING: how to get the repository; whether the feed path is relative or absolute; how to check the install worked; the placeholders must be typed without angle brackets in PowerShell. Not acted on: adding `<dir>` to PATH (optional; the page says to run the tool by its path).
