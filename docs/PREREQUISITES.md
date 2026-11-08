# Prerequisites

This page says what you need before you install and run Avowry. It matches what the CI workflows test.

CI builds and tests the solution with .NET SDK `8.0.x` on `ubuntu-latest`. That includes the packed-tool test, which packs the CLI and installs it from a local feed into a scratch folder.

CPU architecture is unpinned; no architecture is named by the workflows, so the only tested system is the CI image above.

Any other operating system than the CI image is untested, and so is any CPU architecture you run it on. If you try anyway, you need the .NET 8 SDK.

A newer SDK than 8.0.x is untested and does not replace the .NET 8 SDK. Install the .NET 8 SDK next to the newer one; `dotnet --list-sdks` should then list an 8.0.x entry.

v0.1 is .NET-first: the emitter writes C# for net8.0, so building and running the output needs the .NET 8 SDK. A newer ASP.NET Core runtime alone is not enough.

The package is named `Avowry`. It is built from the project `Avowry.Cli`. It is installed with `dotnet tool install Avowry --tool-path <dir> --add-source <feed>` from a local feed. It is not published to a public registry.

## Before you start

The repository is a clone of the avowry repository. The repository root is its top folder.

In the commands below, `<feed>` is a folder you choose to hold the packed package. `<dir>` is a new, empty folder the tool is installed into. Type real paths in their place, without the angle brackets; PowerShell rejects angle brackets. A path may be relative or absolute; absolute is safest.

## Check the SDK

Run from: any directory
Needs: the `dotnet` command

```bash
dotnet --list-sdks
```

Same in PowerShell.

## Pack the CLI into a local feed

Run from: the repository root, a clone of the avowry repository
Needs: the .NET 8 SDK

```bash
dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o <feed>
```

Same in PowerShell.

## Install the CLI

Run from: the repository root
Needs: the .NET 8 SDK and a packed local feed

```bash
dotnet tool install Avowry --tool-path <dir> --add-source <feed>
```

Same in PowerShell.

To check the install worked, run the installed command by its path in `<dir>` with `--version`. It prints `Avowry` followed by a version.
