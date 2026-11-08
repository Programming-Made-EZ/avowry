# Prerequisites

This page says what you need before you install and run Avowry. It matches what the CI workflows test.

CI runs on `ubuntu-latest` with .NET SDK `8.0.x`.

CPU architecture is unpinned; the workflows name none.

Any other operating system or CPU architecture is untested. If you try anyway, you need the .NET 8 SDK.

v0.1 is .NET-first: the emitter writes C# for net8.0, so building and running the output needs the .NET 8 SDK. A newer ASP.NET Core runtime alone is not enough.

The CLI is installed with `dotnet tool install Avowry --tool-path <dir> --add-source <feed>` from a local feed. It is not published to a public registry.

## Check the SDK

Run from: any directory
Needs: the .NET 8 SDK

```bash
dotnet --list-sdks
```

Same in PowerShell.

## Pack the CLI into a local feed

Run from: the repository root
Needs: the .NET 8 SDK

```bash
dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o <feed>
```

Same in PowerShell.

## Install the CLI

Run from: the repository root
Needs: a packed local feed

```bash
dotnet tool install Avowry --tool-path <dir> --add-source <feed>
```

Same in PowerShell.
