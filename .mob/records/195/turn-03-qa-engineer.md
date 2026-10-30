# Card 195 turn 3: qa-engineer (codex/gpt-5.6-sol)

Scrubbed reply (first 40 lines), from the card-mode whiteboard.

RED PROVED. Added `Slice=195` acceptance test `Built_tool_uses_Avowry_assembly_and_source_map_names` in `ProductNamedToolBoundary.cs`.

Executed:

`dotnet test tests/Phrase.Acceptance.Tests/Phrase.Acceptance.Tests.csproj --filter "Slice=195" --nologo --disable-build-servers`

Key failure:

```text
Missing ...\src\Avowry.Cli\bin\Debug\net8.0\Avowry.Cli.dll
Failed: 1, Passed: 0, Total: 1
```

The test also requires `Avowry.Adapters.CSharp`, `sourceFile`, kind `avow`, and explicitly rejects `phraseFile` and kind `phrase`. No production code or `.mob` files were changed.
diff --git a/tests/Phrase.Acceptance.Tests/ProductNamedToolBoundary.cs b/tests/Phrase.Acceptance.Tests/ProductNamedToolBoundary.cs
new file mode 100644
index [redacted]..[redacted]
--- /dev/null
+++ b/tests/Phrase.Acceptance.Tests/ProductNamedToolBoundary.cs
@@ -0,0 +1,46 @@
+using System.Reflection;
+using System.Text.Json;
+using Phrase.Acceptance.Tests.Support;
+
+namespace Phrase.Acceptance.Tests;
+
+// Feature: The adapter, CLI, and source map carry the Avowry product name
+//   As Morgan, I want the tool artifact to contain no Phrase-named production unit,
+//   so that the package presents one product name.
+public class ProductNamedToolBoundary
+{
+    [Fact]
+    [Trait("Slice", "195")]
+    public async Task Built_tool_uses_Avowry_assembly_and_source_map_names()
+    {
+        // Given a valid Avowry source and an empty output directory
+        using var workspace = TempWorkspace.Create();
+        var source = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry");
+
+        // When Morgan builds it through the real tool
