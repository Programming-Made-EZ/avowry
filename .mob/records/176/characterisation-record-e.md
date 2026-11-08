# Card 176 characterisation E (D157), QA (red) seat

Test: `Install_from_an_empty_feed_fails_without_reaching_a_public_source` (`tests/Avowry.Acceptance.Tests/InstallFromAnEmptyFeedFailsWithoutReachingAPublicSource.cs`). Traits: `Slice` = `176`, `Criterion` = `176-E`. C# line count: 74.

Declared CHARACTERISATION of the current cleared-source install: an empty local folder feed plus a nuget.config that `<clear>`s all sources and adds only that feed makes `dotnet tool install Avowry --tool-path ... --add-source ... --configfile ...` fail without reaching a public feed. The packed test relies on this isolation. It passes on the current tree and guards a future fall-through to nuget.org.

## Pass

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Install_from_an_empty_feed_fails"`

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 1 s - Avowry.Acceptance.Tests.dll (net8.0)
```

## Mutation

Temporarily replaced the `<clear />` in `IsolatedFeedConfig` with a `nuget.org` source `https://api.nuget.org/v3/index.json` (local-feed still present). Same filter:

```
Assert.DoesNotContain() Failure: Sub-string found
                                ↓ (pos 47)
String: ···"t feeds https://api.nuget.org/v3/index.js"···
Found:  "nuget.org"
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 1 s - Avowry.Acceptance.Tests.dll (net8.0)
```

The file was still untracked, so `git checkout -- tests/Avowry.Acceptance.Tests/InstallFromAnEmptyFeedFailsWithoutReachingAPublicSource.cs` did not match a git path; the `<clear />` config was restored by reversing that edit.

## Neighbouring tests

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~PackedToolIsVersioned|FullyQualifiedName~AvowryCommand|FullyQualifiedName~Tasks_demo_generated_csharp"`

```
Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)
```
